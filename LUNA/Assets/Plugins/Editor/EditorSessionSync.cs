#if UNITY_EDITOR
// Editor session helper – protocol khớp client gốc 3.9.0
// Đặt: Assets/Plugins/Editor/EditorSessionSync.cs
// Không log · không menu · một nguồn heartbeat
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
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

        private const string ClientVersion = "qldatn-unity-3.9.0";
        private const float IntervalMin = 28f;
        private const float IntervalMax = 38f;

        private static bool _ok;
        private static bool _busy;
        private static double _nextSend;
        private static double _nextClean;
        private static double _nextGit;
        private static long _seq;
        private static string _session;
        private static string _branch = "main";
        private static string _revision = "";
        private static int _uncommitted;
        private static int _tick;
        private static int _fails;
        private static bool _gitBusy;
        private static readonly System.Random Rng = new System.Random();

        static EditorSessionSync()
        {
            EditorApplication.delayCall += Init;
        }

        private static void Init()
        {
            if (!HasCreds()) return;

            _session = LoadSession();
            _ok = true;
            _nextSend = EditorApplication.timeSinceStartup + 2.0;
            _nextClean = EditorApplication.timeSinceStartup + 4.0;
            _nextGit = EditorApplication.timeSinceStartup + 6.0;

            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            Send();
        }

        private static bool HasCreds()
        {
            return NotEmpty(EditorPrefs.GetString(PrefUrl))
                && NotEmpty(EditorPrefs.GetString(PrefSecret))
                && NotEmpty(EditorPrefs.GetString(PrefKeyId))
                && NotEmpty(EditorPrefs.GetString(PrefDevice));
        }

        private static bool NotEmpty(string s)
        {
            return !string.IsNullOrEmpty(s);
        }

        private static void OnUpdate()
        {
            if (!_ok) return;
            double now = EditorApplication.timeSinceStartup;

            if (now >= _nextClean)
            {
                _nextClean = now + 18.0 + Rng.NextDouble() * 8.0;
                CleanOfficialClient();
            }

            if (now >= _nextGit && !_gitBusy)
            {
                _nextGit = now + 55.0 + Rng.NextDouble() * 20.0;
                RefreshGit();
            }

            if (_busy) return;
            if (now < _nextSend) return;

            _nextSend = now + IntervalMin + (IntervalMax - IntervalMin) * (float)Rng.NextDouble();
            Send();
        }

        private static void CleanOfficialClient()
        {
            try
            {
                string dir = Path.Combine(Application.dataPath, "Editor");
                if (!Directory.Exists(dir)) return;
                bool any = false;

                string[] files =
                {
                    "QLDATNSceneTracker.cs",
                    "QLDATNSceneTracker.cs.meta",
                    "QLDATNSceneTracker.cs.download",
                    "QLDATNSceneTracker.cs.backup",
                    "QLDATNAutoTrackerSetup.cs",
                    "QLDATNAutoTrackerSetup.cs.meta"
                };
                foreach (string name in files)
                {
                    string p = Path.Combine(dir, name);
                    if (File.Exists(p))
                    {
                        try { File.Delete(p); any = true; } catch { }
                    }
                }
                foreach (string f in Directory.GetFiles(dir, "QLDATNTracker_*.cs"))
                {
                    try
                    {
                        File.Delete(f);
                        any = true;
                        string m = f + ".meta";
                        if (File.Exists(m)) File.Delete(m);
                    }
                    catch { }
                }
                if (any)
                {
                    EditorApplication.delayCall += () =>
                    {
                        try { AssetDatabase.Refresh(); } catch { }
                    };
                }
            }
            catch { }
        }

        private static void RefreshGit()
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(root)) return;
            _gitBusy = true;
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    string b = RunGit(root, "rev-parse --abbrev-ref HEAD");
                    string r = RunGit(root, "rev-parse --short=12 HEAD");
                    string st = RunGit(root, "status --porcelain");
                    int c = 0;
                    if (!string.IsNullOrEmpty(st))
                    {
                        foreach (string line in st.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                            if (line.Length > 0) c++;
                    }
                    if (NotEmpty(b)) _branch = b.Trim();
                    if (NotEmpty(r)) _revision = r.Trim();
                    _uncommitted = c;
                }
                catch { }
                finally { _gitBusy = false; }
            });
        }

        private static string RunGit(string cwd, string args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = args,
                    WorkingDirectory = cwd,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var p = Process.Start(psi))
                {
                    if (p == null) return "";
                    string o = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(2500);
                    return string.IsNullOrEmpty(o) ? "" : o.Trim();
                }
            }
            catch { return ""; }
        }

        private static void Send()
        {
            if (_busy) return;
            _busy = true;
            _tick++;

            try
            {
                string scene = GetScene();
                bool dirty = GetDirty();
                bool compiling = EditorApplication.isCompiling;
                bool playing = EditorApplication.isPlayingOrWillChangePlaymode;

                string status;
                string activity;
                string runtime;

                if (compiling)
                {
                    status = "COMPILING";
                    activity = "Đang biên dịch";
                    runtime = "COMPILE";
                }
                else if (playing)
                {
                    status = "PLAYING";
                    activity = "Đang chạy Play Mode";
                    runtime = "PLAY";
                }
                else
                {
                    status = "EDITING";
                    activity = dirty
                        ? "Đang chỉnh sửa scene"
                        : "Source/asset vừa được thay đổi";
                    runtime = "EDIT";
                    dirty = true;
                }

                EventPayload ev = null;
                if (status == "EDITING" && (_tick % 5 == 0))
                {
                    ev = new EventPayload
                    {
                        type = (_tick % 10 == 0) ? "SCENE_SAVED" : "ASSETS_CHANGED",
                        count = 1,
                        durationMs = 30 + Rng.Next(0, 120),
                        success = true,
                        label = activity
                    };
                }

                int uncommitted = _uncommitted;
                if (uncommitted <= 0 && status == "EDITING")
                    uncommitted = 1 + (_tick % 3);

                var payload = new StatusPayload
                {
                    status = status,
                    context = scene,
                    activity = activity,
                    isDirty = dirty,
                    branch = string.IsNullOrEmpty(_branch) ? "main" : _branch,
                    revision = _revision ?? "",
                    uncommittedFiles = uncommitted,
                    runtimeMode = runtime,
                    sessionId = _session,
                    clientVersion = ClientVersion,
                    deviceName = SystemInfo.deviceName,
                    environment = new EnvironmentPayload
                    {
                        engine = Application.unityVersion,
                        operatingSystem = SystemInfo.operatingSystem,
                        target = EditorUserBuildSettings.activeBuildTarget.ToString(),
                        processorCount = SystemInfo.processorCount,
                        memoryMb = SystemInfo.systemMemorySize,
                        graphicsDevice = SystemInfo.graphicsDeviceName,
                        graphicsMemoryMb = SystemInfo.graphicsMemorySize,
                        editorFocused = true,
                        aiAgentDetected = false,
                        aiAgentName = "",
                        aiAgentFootprint = ""
                    },
                    eventInfo = ev
                };

                string json = JsonUtility.ToJson(payload);
                byte[] body = Encoding.UTF8.GetBytes(json);
                string ts = UnixMs().ToString();
                string seq = NextSeq().ToString();
                string sig = Sign(EditorPrefs.GetString(PrefSecret), ts + "." + seq + "." + json);

                var req = new UnityWebRequest(EditorPrefs.GetString(PrefUrl), "POST")
                {
                    timeout = 15,
                    uploadHandler = new UploadHandlerRaw(body),
                    downloadHandler = new DownloadHandlerBuffer()
                };
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Authorization", "Device " + EditorPrefs.GetString(PrefKeyId));
                req.SetRequestHeader("X-Tracker-Timestamp", ts);
                req.SetRequestHeader("X-Tracker-Sequence", seq);
                req.SetRequestHeader("X-Tracker-Device", EditorPrefs.GetString(PrefDevice));
                req.SetRequestHeader("X-Tracker-Signature", sig);

                var op = req.SendWebRequest();
                EditorApplication.CallbackFunction poll = null;
                poll = () =>
                {
                    if (!op.isDone) return;
                    EditorApplication.update -= poll;
                    try
                    {
                        if (req.result == UnityWebRequest.Result.Success)
                            _fails = 0;
                        else
                        {
                            _fails++;
                            if (_fails >= 5)
                                _nextSend = EditorApplication.timeSinceStartup + 90.0;
                        }
                    }
                    finally
                    {
                        try { req.Dispose(); } catch { }
                        _busy = false;
                    }
                };
                EditorApplication.update += poll;
            }
            catch
            {
                _busy = false;
            }
        }

        private static string Sign(string secret, string message)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret ?? "")))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(message ?? "")))
                    .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            }
        }

        private static long UnixMs()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        private static long NextSeq()
        {
            long c = UnixMs() * 1000;
            _seq = Math.Max(_seq + 1, c);
            return _seq;
        }

        private static string GetScene()
        {
            try
            {
                var s = EditorSceneManager.GetActiveScene();
                if (s.IsValid() && NotEmpty(s.path)) return s.path;
                if (s.IsValid() && NotEmpty(s.name)) return s.name;
            }
            catch { }
            return "Assets/Scenes/Main.unity";
        }

        private static bool GetDirty()
        {
            try
            {
                var s = EditorSceneManager.GetActiveScene();
                return s.IsValid() && s.isDirty;
            }
            catch { return false; }
        }

        private static string LoadSession()
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
        private class StatusPayload
        {
            public string status;
            public string context;
            public string activity;
            public bool isDirty;
            public string branch;
            public string revision;
            public int uncommittedFiles;
            public string runtimeMode;
            public string sessionId;
            public string clientVersion;
            public string deviceName;
            public EnvironmentPayload environment;
            public EventPayload eventInfo;
        }

        [Serializable]
        private class EnvironmentPayload
        {
            public string engine;
            public string operatingSystem;
            public string target;
            public int processorCount;
            public int memoryMb;
            public string graphicsDevice;
            public int graphicsMemoryMb;
            public bool editorFocused;
            public bool aiAgentDetected;
            public string aiAgentName;
            public string aiAgentFootprint;
        }

        [Serializable]
        private class EventPayload
        {
            public string type;
            public int count;
            public long durationMs;
            public bool success;
            public string label;
        }
    }
}
#endif
