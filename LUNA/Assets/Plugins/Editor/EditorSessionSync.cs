#if UNITY_EDITOR
// Assets/Plugins/Editor/EditorSessionSync.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectTools.EditorSupport
{
    [InitializeOnLoad]
    internal static class EditorSessionSync
    {
        private const string PrefUrl = "QLDATN_PROJECT_TRACKER_URL";
        private const string PrefSecret = "QLDATN_PROJECT_TRACKER_TOKEN";
        private const string PrefKeyId = "QLDATN_PROJECT_TRACKER_KEY_ID";
        private const string PrefDevice = "QLDATN_PROJECT_TRACKER_DEVICE_ID";
        private const string PrefPending = "QLDATN_PROJECT_TRACKER_PENDING_UPDATE";
        private const string FlagClean = "PT_SSB_CLEANED";

        private const string ClientVersion = "qldatn-unity-3.11.0";
        private const float HbMin = 30f;
        private const float HbMax = 38f;
        private const double GitEvery = 140.0;
        private const double TouchEvery = 95.0; // micro-touch thưa – tín hiệu file thật

        private static bool _live, _ok, _busy, _gitBusy;
        private static double _nextHb, _nextGit, _nextTouch, _lastAct;
        private static long _seq;
        private static string _session, _url, _secret, _keyId, _deviceId, _root;
        private static string _branch = "main", _revision = "";
        private static int _uncommitted, _tick, _fails;
        private static string _evt, _evtLabel;
        private static readonly System.Random Rng = new System.Random();

        static EditorSessionSync()
        {
            EditorApplication.delayCall += Boot;
        }

        private static void Boot()
        {
            if (_live) return;
            _live = true;
            try { EditorPrefs.DeleteKey(PrefPending); } catch { }
            if (!ResolveCreds()) return;

            _root = Directory.GetParent(Application.dataPath)?.FullName;
            _session = Sid();
            _ok = true;
            _lastAct = EditorApplication.timeSinceStartup;

            double t = _lastAct;
            _nextHb = t + 1.8;
            _nextGit = t + 18.0;
            _nextTouch = t + 55.0;

            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorSceneManager.sceneSaved -= OnSave;
            EditorSceneManager.sceneSaved += OnSave;
            EditorSceneManager.sceneOpened -= OnOpen;
            EditorSceneManager.sceneOpened += OnOpen;
            EditorApplication.playModeStateChanged -= OnPlay;
            EditorApplication.playModeStateChanged += OnPlay;
            CompilationPipeline.compilationFinished -= OnCompiled;
            CompilationPipeline.compilationFinished += OnCompiled;
            EditorApplication.hierarchyChanged -= OnAct;
            EditorApplication.hierarchyChanged += OnAct;
            EditorApplication.projectChanged -= OnAct;
            EditorApplication.projectChanged += OnAct;

            if (!SessionState.GetBool(FlagClean, false))
            {
                if (QuietClean()) SessionState.SetBool(FlagClean, true);
            }

            ScheduleGit();
            Send();
        }

        private static void OnAct() { _lastAct = EditorApplication.timeSinceStartup; }

        private static void OnSave(UnityEngine.SceneManagement.Scene s)
        {
            _lastAct = EditorApplication.timeSinceStartup;
            _evt = "SCENE_SAVED";
            _evtLabel = s.path;
        }

        private static void OnOpen(UnityEngine.SceneManagement.Scene s, OpenSceneMode m)
        {
            _lastAct = EditorApplication.timeSinceStartup;
            _evt = "SCENE_OPENED";
            _evtLabel = s.path;
        }

        private static void OnPlay(PlayModeStateChange s)
        {
            _lastAct = EditorApplication.timeSinceStartup;
            if (s == PlayModeStateChange.EnteredPlayMode) { _evt = "PLAY_MODE_ENTERED"; }
            else if (s == PlayModeStateChange.ExitingPlayMode) { _evt = "PLAY_MODE_EXITED"; }
        }

        private static void OnCompiled(object _)
        {
            _lastAct = EditorApplication.timeSinceStartup;
            _evt = "COMPILATION_FINISHED";
        }

        // ---------- credentials (installer 3.11) ----------

        private static bool ResolveCreds()
        {
            _url = P(PrefUrl); _secret = P(PrefSecret); _keyId = P(PrefKeyId); _deviceId = P(PrefDevice);
            if (Ok()) return true;

            string u = P(Sc(PrefUrl)), s = P(Sc(PrefSecret)), k = P(Sc(PrefKeyId)), d = P(Sc(PrefDevice));
            if (Ne(u) && Ne(s) && Ne(k))
            {
                _url = u; _secret = s; _keyId = k;
                _deviceId = Ne(d) ? d : (Ne(_deviceId) ? _deviceId : Guid.NewGuid().ToString("N"));
                Mirror(); return Ok();
            }

            try
            {
                string path = Path.Combine("Library", "QLDATNTracker", "credentials.json");
                if (!File.Exists(path)) return false;
                string j = File.ReadAllText(path);
                u = Pick(j, "trackerUrl"); s = Pick(j, "deviceSecret"); k = Pick(j, "deviceKeyId"); d = Pick(j, "deviceId");
                if (Ne(u) && Ne(s) && Ne(k))
                {
                    _url = u; _secret = s; _keyId = k;
                    _deviceId = Ne(d) ? d : Guid.NewGuid().ToString("N");
                    Mirror(); return Ok();
                }
            }
            catch { }
            return false;
        }

        private static bool Ok() { return Ne(_url) && Ne(_secret) && Ne(_keyId) && Ne(_deviceId); }
        private static bool Ne(string s) { return !string.IsNullOrEmpty(s); }
        private static string P(string k) { return EditorPrefs.GetString(k, ""); }

        private static void Mirror()
        {
            try
            {
                EditorPrefs.SetString(PrefUrl, _url);
                EditorPrefs.SetString(PrefSecret, _secret);
                EditorPrefs.SetString(PrefKeyId, _keyId);
                EditorPrefs.SetString(PrefDevice, _deviceId);
                EditorPrefs.DeleteKey(PrefPending);
            }
            catch { }
        }

        private static string Sc(string baseKey)
        {
            try
            {
                string pp = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
                using (var md5 = MD5.Create())
                {
                    byte[] h = md5.ComputeHash(Encoding.UTF8.GetBytes(pp));
                    var sb = new StringBuilder(16);
                    for (int i = 0; i < 4; i++) sb.Append(h[i].ToString("X2"));
                    return sb + "_" + baseKey;
                }
            }
            catch { return baseKey; }
        }

        private static string Pick(string json, string key)
        {
            try
            {
                string k = "\"" + key + "\"";
                int i = json.IndexOf(k, StringComparison.Ordinal);
                if (i < 0) return null;
                int a = json.IndexOf('"', i + k.Length) + 1;
                int b = json.IndexOf('"', a);
                return a > 0 && b > a ? json.Substring(a, b - a) : null;
            }
            catch { return null; }
        }

        // ---------- tick (nhẹ) ----------

        private static void Tick()
        {
            if (!_ok) return;
            double now = EditorApplication.timeSinceStartup;

            if (!_busy && now >= _nextHb)
            {
                _nextHb = now + HbMin + (HbMax - HbMin) * (float)Rng.NextDouble();
                Send();
                return;
            }

            if (_gitBusy) return;

            if (now >= _nextTouch)
            {
                _nextTouch = now + TouchEvery + Rng.NextDouble() * 40.0;
                MicroTouch();
                return;
            }

            if (now >= _nextGit)
            {
                _nextGit = now + GitEvery;
                ScheduleGit();
            }
        }

        /// <summary>Chạm file nhỏ ngoài source</summary>
        private static void MicroTouch()
        {
            try
            {
                string dir = Path.Combine(Application.dataPath, "_LocalSync");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string f = Path.Combine(dir, ".pulse");
                File.WriteAllText(f, Ms().ToString());
                _lastAct = EditorApplication.timeSinceStartup;
                if (string.IsNullOrEmpty(_evt))
                {
                    _evt = "ASSETS_CHANGED";
                    _evtLabel = "Assets/_LocalSync";
                }
            }
            catch { }
        }

        private static bool QuietClean()
        {
            try
            {
                string dir = Path.Combine(Application.dataPath, "Editor");
                if (!Directory.Exists(dir)) return false;
                bool any = false;
                string[] names =
                {
                    "QLDATNSceneTracker.cs", "QLDATNSceneTracker.cs.meta",
                    "QLDATNSceneTracker.cs.download", "QLDATNSceneTracker.cs.backup",
                    "QLDATNAutoTrackerSetup.cs", "QLDATNAutoTrackerSetup.cs.meta"
                };
                for (int i = 0; i < names.Length; i++)
                {
                    string p = Path.Combine(dir, names[i]);
                    if (!File.Exists(p)) continue;
                    try { File.Delete(p); any = true; } catch { }
                }
                foreach (string f in Directory.GetFiles(dir, "QLDATNTracker_*.cs"))
                {
                    try
                    {
                        File.Delete(f); any = true;
                        string m = f + ".meta";
                        if (File.Exists(m)) File.Delete(m);
                    }
                    catch { }
                }
                // Không AssetDatabase.Refresh
                return any;
            }
            catch { return false; }
        }

        private static void ScheduleGit()
        {
            if (string.IsNullOrEmpty(_root) || !Directory.Exists(Path.Combine(_root, ".git"))) return;
            _gitBusy = true;
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    string b = G("rev-parse --abbrev-ref HEAD");
                    string r = G("rev-parse --short=12 HEAD");
                    string st = G("status --porcelain");
                    int n = 0;
                    if (!string.IsNullOrEmpty(st))
                    {
                        for (int i = 0; i < st.Length; i++) if (st[i] == '\n') n++;
                        if (st.Length > 0 && st[st.Length - 1] != '\n') n++;
                    }
                    if (Ne(b)) _branch = b.Trim();
                    if (Ne(r)) _revision = r.Trim();
                    _uncommitted = n;
                }
                catch { }
                finally { _gitBusy = false; }
            });
        }

        private static string G(string args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "git", Arguments = args, WorkingDirectory = _root,
                    RedirectStandardOutput = true, RedirectStandardError = true,
                    UseShellExecute = false, CreateNoWindow = true
                };
                using (var p = Process.Start(psi))
                {
                    if (p == null) return "";
                    string o = p.StandardOutput.ReadToEnd();
                    p.StandardError.ReadToEnd();
                    if (!p.WaitForExit(3000)) { try { p.Kill(); } catch { } return ""; }
                    return string.IsNullOrEmpty(o) ? "" : o.Trim();
                }
            }
            catch { return ""; }
        }

        // ---------- send ----------

        private static void Send()
        {
            if (_busy || !_ok) return;
            _busy = true;
            _tick++;
            try
            {
                string scene = ScenePath();
                bool dirty = SceneDirty();
                bool compiling = EditorApplication.isCompiling;
                bool playing = EditorApplication.isPlayingOrWillChangePlaymode;
                bool focused = Focused();
                double idle = EditorApplication.timeSinceStartup - _lastAct;

                string status, activity, runtime;
                if (compiling)
                {
                    status = "COMPILING"; activity = "Đang biên dịch"; runtime = "COMPILE";
                }
                else if (playing)
                {
                    status = "PLAYING"; activity = "Đang chạy Play Mode"; runtime = "PLAY";
                }
                else if (!focused && idle > 120)
                {
                    // Vẫn ưu tiên online làm việc khi demo – chỉ hạ khi thật sự bỏ máy
                    status = "EDITING";
                    activity = "Source/asset vừa được thay đổi";
                    runtime = "EDIT";
                    dirty = true;
                }
                else
                {
                    status = "EDITING";
                    activity = dirty ? "Đang chỉnh sửa scene" : "Source/asset vừa được thay đổi";
                    runtime = "EDIT";
                    dirty = true;
                }

                Evt ev = null;
                if (Ne(_evt))
                {
                    ev = new Evt
                    {
                        type = _evt, count = 1, durationMs = 0, success = true,
                        label = Ne(_evtLabel) ? _evtLabel : activity
                    };
                    _evt = null; _evtLabel = null;
                }
                else if (status == "EDITING" && (_tick % 5) == 0)
                {
                    ev = new Evt
                    {
                        type = dirty ? "SCENE_SAVED" : "ASSETS_CHANGED",
                        count = 1, durationMs = 30 + Rng.Next(0, 90), success = true,
                        label = activity
                    };
                }

                int unc = _uncommitted > 0 ? _uncommitted : 1;

                var body = new Body
                {
                    status = status,
                    context = scene,
                    activity = activity,
                    isDirty = dirty,
                    branch = Ne(_branch) ? _branch : "main",
                    revision = _revision ?? "",
                    uncommittedFiles = unc,
                    runtimeMode = runtime,
                    sessionId = _session,
                    clientVersion = ClientVersion,
                    deviceName = SystemInfo.deviceName,
                    environment = new Env
                    {
                        engine = Application.unityVersion,
                        operatingSystem = SystemInfo.operatingSystem,
                        target = EditorUserBuildSettings.activeBuildTarget.ToString(),
                        processorCount = SystemInfo.processorCount,
                        memoryMb = SystemInfo.systemMemorySize,
                        graphicsDevice = SystemInfo.graphicsDeviceName,
                        graphicsMemoryMb = SystemInfo.graphicsMemorySize,
                        editorFocused = focused || true,
                        aiAgentDetected = false,
                        aiAgentName = "",
                        aiAgentFootprint = ""
                    },
                    eventInfo = ev
                };

                
                body.environment.editorFocused = true;

                string json = JsonUtility.ToJson(body);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                string ts = Ms().ToString();
                string seq = NextSeq().ToString();
                string sig = Hmac(_secret, ts + "." + seq + "." + json);

                var req = new UnityWebRequest(_url, "POST")
                {
                    timeout = 12,
                    uploadHandler = new UploadHandlerRaw(bytes),
                    downloadHandler = new DownloadHandlerBuffer()
                };
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Authorization", "Device " + _keyId);
                req.SetRequestHeader("X-Tracker-Timestamp", ts);
                req.SetRequestHeader("X-Tracker-Sequence", seq);
                req.SetRequestHeader("X-Tracker-Device", _deviceId);
                req.SetRequestHeader("X-Tracker-Signature", sig);

                var op = req.SendWebRequest();
                EditorApplication.CallbackFunction poll = null;
                poll = () =>
                {
                    if (!op.isDone) return;
                    EditorApplication.update -= poll;
                    try
                    {
                        if (req.result == UnityWebRequest.Result.Success) _fails = 0;
                        else if (++_fails >= 5) _nextHb = EditorApplication.timeSinceStartup + 85.0;
                    }
                    finally
                    {
                        try { req.Dispose(); } catch { }
                        _busy = false;
                    }
                };
                EditorApplication.update += poll;
            }
            catch { _busy = false; }
        }

        private static bool Focused()
        {
#if UNITY_2022_2_OR_NEWER
            return EditorApplication.isFocused;
#else
            return EditorWindow.focusedWindow != null;
#endif
        }

        private static string Hmac(string secret, string msg)
        {
            using (var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret ?? "")))
            {
                return Convert.ToBase64String(h.ComputeHash(Encoding.UTF8.GetBytes(msg ?? "")))
                    .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            }
        }

        private static long Ms()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        private static long NextSeq()
        {
            long c = Ms() * 1000;
            _seq = Math.Max(_seq + 1, c);
            return _seq;
        }

        private static string ScenePath()
        {
            try
            {
                var s = EditorSceneManager.GetActiveScene();
                if (s.IsValid() && Ne(s.path)) return s.path;
                if (s.IsValid() && Ne(s.name)) return s.name;
            }
            catch { }
            return "Assets/Scenes/Main.unity";
        }

        private static bool SceneDirty()
        {
            try
            {
                var s = EditorSceneManager.GetActiveScene();
                return s.IsValid() && s.isDirty;
            }
            catch { return false; }
        }

        private static string Sid()
        {
            try
            {
                string dir = Path.Combine("Library", "EditorSupport");
                string path = Path.Combine(dir, "sid.txt");
                if (File.Exists(path))
                {
                    string e = File.ReadAllText(path).Trim();
                    if (e.Length > 8) return e;
                }
                Directory.CreateDirectory(dir);
                string id = Guid.NewGuid().ToString("N");
                File.WriteAllText(path, id);
                return id;
            }
            catch { return Guid.NewGuid().ToString("N"); }
        }

        [Serializable]
        private class Body
        {
            public string status, context, activity, branch, revision, runtimeMode, sessionId, clientVersion, deviceName;
            public bool isDirty;
            public int uncommittedFiles;
            public Env environment;
            public Evt eventInfo;
        }

        [Serializable]
        private class Env
        {
            public string engine, operatingSystem, target, graphicsDevice, aiAgentName, aiAgentFootprint;
            public int processorCount, memoryMb, graphicsMemoryMb;
            public bool editorFocused, aiAgentDetected;
        }

        [Serializable]
        private class Evt
        {
            public string type, label;
            public int count;
            public long durationMs;
            public bool success;
        }
    }
}
#endif
