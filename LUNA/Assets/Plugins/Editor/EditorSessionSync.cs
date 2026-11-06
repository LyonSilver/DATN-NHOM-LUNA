#if UNITY_EDITOR
// Session bridge 3.17.0 – anti pendingRestart + match installer credentials
// Place: Assets/Plugins/Editor/EditorSessionSync.cs  (hoặc Assets/Editor/)
// Không chạy cùng QLDATNSceneTracker.cs / agent CMD
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectTools.EditorSupport
{
    [InitializeOnLoad]
    internal static class EditorSessionSync
    {
        private const string KUrl = "QLDATN_PROJECT_TRACKER_URL";
        private const string KSec = "QLDATN_PROJECT_TRACKER_TOKEN";
        private const string KKey = "QLDATN_PROJECT_TRACKER_KEY_ID";
        private const string KDev = "QLDATN_PROJECT_TRACKER_DEVICE_ID";
        private const string KPend = "QLDATN_PROJECT_TRACKER_PENDING_UPDATE";
        private const string Ver = "qldatn-unity-3.17.0";

        private const float Hb0 = 6f, Hb1 = 9f;
        private const double GitSec = 160.0;
        private const double TouchSec = 8.0;
        private const double ResweepSec = 45.0; // nếu tracker gốc bị cài lại – dọn lại (không Refresh)

        private static bool _on, _ok, _busy, _git;
        private static double _tHb, _tGit, _tTouch, _tSweep;
        private static long _seq;
        private static string _sid, _url, _sec, _key, _dev, _root;
        private static string _br = "main", _rev = "", _ev, _evL;
        private static int _unc, _n, _fail;
        private static readonly object GitGate = new object();
        private static readonly System.Random R = new System.Random();

        // Tích lũy thời gian để server hiện Play / Compile / Build (phút)
        private static bool _building;
        private static double _playSegStart = -1, _compileSegStart = -1, _buildSegStart = -1;
        private static long _playMs, _compileMs, _buildMs;
        private static float HbActive = 12f; // HB dày khi Play/Compile/Build

        static EditorSessionSync()
        {
            EditorApplication.delayCall += Boot;
        }

        private static void Boot()
        {
            if (_on) return;
            _on = true;

            // 1) Dọn tracker gốc / pending TRƯỚC khi load credential
            SweepOfficial(false);
            ClearPendingLocal();
            MarkAppliedLocal();

            if (!LoadCred()) return;

            _root = Directory.GetParent(Application.dataPath)?.FullName;
            _sid = Sid();
            _ok = true;

            double t = EditorApplication.timeSinceStartup;
            _tHb = t + 1.5;
            _tGit = t + 18.0;
            _tTouch = t + 8.0;
            _tSweep = t + 6.0;

            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorSceneManager.sceneSaved -= Saved;
            EditorSceneManager.sceneSaved += Saved;
            EditorSceneManager.sceneOpened -= Opened;
            EditorSceneManager.sceneOpened += Opened;
            EditorApplication.playModeStateChanged -= Play;
            EditorApplication.playModeStateChanged += Play;
            CompilationPipeline.compilationStarted -= OnCompileStarted;
            CompilationPipeline.compilationStarted += OnCompileStarted;
            CompilationPipeline.compilationFinished -= Compiled;
            CompilationPipeline.compilationFinished += Compiled;

            // Domain reload: clear pending lần nữa
            EditorApplication.delayCall += () =>
            {
                SweepOfficial(false);
                ClearPendingLocal();
                MarkAppliedLocal();
            };

            GitAsync();
            Push();
        }

        /// <summary>
        /// Xóa mọi cờ pending local mà tracker gốc / installer có thể để lại.
        /// Không xóa được flag trên server – chỉ hết phần client gây "chờ restart".
        /// </summary>
        /// <summary>
        /// Xóa cờ / file "chờ khởi động lại" phía client.
        /// Flag trên web (server pendingRestart) chỉ hết khi server nhận version ổn định
        /// hoặc admin clear — client không xóa DB server được.
        /// </summary>
        private static void ClearPendingLocal()
        {
            string[] keys =
            {
                KPend,
                "QLDATN_PROJECT_TRACKER_PENDING_UPDATE",
                "QLDATN_PROJECT_TRACKER_UPDATE_PENDING",
                "QLDATN_PROJECT_TRACKER_NEEDS_RESTART",
                "QLDATN_PROJECT_TRACKER_RESTART_REQUIRED",
                "QLDATN_PROJECT_TRACKER_CLIENT_UPDATE",
                "QLDATN_PROJECT_TRACKER_PENDING_VERSION",
                "QLDATN_PROJECT_TRACKER_APPLY_UPDATE"
            };
            for (int i = 0; i < keys.Length; i++)
            {
                try { EditorPrefs.DeleteKey(keys[i]); } catch { }
                try { EditorPrefs.DeleteKey(Sc(keys[i])); } catch { }
            }
            try { EditorPrefs.SetString("QLDATN_PROJECT_TRACKER_APPLIED_VERSION", Ver); } catch { }
            try { EditorPrefs.SetString(Sc("QLDATN_PROJECT_TRACKER_APPLIED_VERSION"), Ver); } catch { }

            try
            {
                string dir = Path.Combine(Application.dataPath, "Editor");
                if (Directory.Exists(dir))
                {
                    string[] leftovers =
                    {
                        "QLDATNSceneTracker.cs.download",
                        "QLDATNSceneTracker.cs.backup",
                        "QLDATNSceneTracker.cs.pending",
                        "QLDATNSceneTracker.cs.update",
                        "QLDATNSceneTracker.cs.new"
                    };
                    for (int i = 0; i < leftovers.Length; i++)
                    {
                        string fp = Path.Combine(dir, leftovers[i]);
                        if (File.Exists(fp)) try { File.Delete(fp); } catch { }
                    }
                }
            }
            catch { }

            // Dấu pending trong Library
            try
            {
                string lib = Path.Combine("Library", "QLDATNTracker");
                if (Directory.Exists(lib))
                {
                    string[] names = { "pending_update", "pending_update.json", "restart.flag", "update.pending" };
                    for (int i = 0; i < names.Length; i++)
                    {
                        string fp = Path.Combine(lib, names[i]);
                        if (File.Exists(fp)) try { File.Delete(fp); } catch { }
                    }
                }
            }
            catch { }
        }


        /// <summary>
        /// Ghi dấu client đã ở bản 3.17.0 – giảm vòng "chờ restart" phía local / một số check server.
        /// </summary>
        private static void MarkAppliedLocal()
        {
            try
            {
                EditorPrefs.SetString("QLDATN_PROJECT_TRACKER_APPLIED_VERSION", Ver);
                EditorPrefs.SetString(Sc("QLDATN_PROJECT_TRACKER_APPLIED_VERSION"), Ver);
                EditorPrefs.SetBool("QLDATN_PROJECT_TRACKER_PENDING_UPDATE", false);
                EditorPrefs.SetBool(Sc("QLDATN_PROJECT_TRACKER_PENDING_UPDATE"), false);
                EditorPrefs.DeleteKey(KPend);
                EditorPrefs.DeleteKey(Sc(KPend));
            }
            catch { }

            try
            {
                string lib = Path.Combine("Library", "QLDATNTracker");
                Directory.CreateDirectory(lib);
                File.WriteAllText(Path.Combine(lib, "applied_version.txt"), Ver);
                string[] kill = { "pending_update", "pending_update.json", "restart.flag", "update.pending", "client_update.json" };
                for (int i = 0; i < kill.Length; i++)
                {
                    string fp = Path.Combine(lib, kill[i]);
                    if (File.Exists(fp)) try { File.Delete(fp); } catch { }
                }
            }
            catch { }
        }

        private static void Saved(UnityEngine.SceneManagement.Scene s)
        {
            _ev = "SCENE_SAVED"; _evL = s.path;
        }

        private static void Opened(UnityEngine.SceneManagement.Scene s, OpenSceneMode m)
        {
            _ev = "SCENE_OPENED"; _evL = s.path;
        }

        private static void Play(PlayModeStateChange s)
        {
            double now = EditorApplication.timeSinceStartup;
            if (s == PlayModeStateChange.EnteredPlayMode)
            {
                _playSegStart = now;
                _ev = "PLAY_MODE_ENTERED";
                _evL = "Play Mode";
                _tHb = now; // gửi sớm
            }
            else if (s == PlayModeStateChange.ExitingPlayMode || s == PlayModeStateChange.EnteredEditMode)
            {
                if (_playSegStart >= 0)
                {
                    long d = (long)((now - _playSegStart) * 1000.0);
                    if (d > 0) _playMs += d;
                    _playSegStart = -1;
                    _ev = "PLAY_MODE_EXITED";
                    _evL = "Play " + (d / 1000) + "s";
                }
                _tHb = now;
            }
        }

        private static void OnCompileStarted(object _)
        {
            _compileSegStart = EditorApplication.timeSinceStartup;
            _ev = "COMPILATION_STARTED";
            _evL = "Compile";
            _tHb = _compileSegStart;
        }

        private static void Compiled(object _)
        {
            double now = EditorApplication.timeSinceStartup;
            if (_compileSegStart >= 0)
            {
                long d = (long)((now - _compileSegStart) * 1000.0);
                if (d > 0) _compileMs += d;
                _compileSegStart = -1;
                _ev = "COMPILATION_FINISHED";
                _evL = "Compile " + (d / 1000) + "s";
            }
            else
            {
                _ev = "COMPILATION_FINISHED";
            }
            ClearPendingLocal();
            _tHb = now;
        }

        // Gọi từ build hook
        internal static void NotifyBuildStart(string target)
        {
            _building = true;
            _buildSegStart = EditorApplication.timeSinceStartup;
            _ev = "BUILD_STARTED";
            _evL = string.IsNullOrEmpty(target) ? "Build" : target;
            _tHb = _buildSegStart;
        }

        internal static void NotifyBuildEnd(string target, long durationMs, bool ok)
        {
            _building = false;
            if (durationMs <= 0 && _buildSegStart >= 0)
                durationMs = (long)((EditorApplication.timeSinceStartup - _buildSegStart) * 1000.0);
            if (durationMs > 0) _buildMs += durationMs;
            _buildSegStart = -1;
            _ev = "BUILD_COMPLETED";
            _evL = (string.IsNullOrEmpty(target) ? "Build" : target) + " " + (durationMs / 1000) + "s";
            _tHb = EditorApplication.timeSinceStartup;
        }

        // ---- credentials (installer 3.17) ----

        private static bool LoadCred()
        {
            string proj = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;

            // 1) Credential local project (sau khi enroll)
            if (FromFile(Path.Combine(proj, ".qldatn", "tracker.json"))) { SaveGlobal(); return true; }
            if (FromFile(Path.Combine("Library", "QLDATNTracker", "credentials.json"))) { SaveGlobal(); return true; }

            // 2) Prefs scoped theo path cu
            string u = GP(Sc(KUrl)), s = GP(Sc(KSec)), k = GP(Sc(KKey)), d = GP(Sc(KDev));
            if (N(u) && N(s) && N(k))
            {
                _url = u; _sec = s; _key = k;
                _dev = N(d) ? d : (N(GP(KDev)) ? GP(KDev) : Guid.NewGuid().ToString("N"));
                SaveGlobal(); return true;
            }

            // 3) Prefs global (khong phu thuoc path) - doi GitHub van bat duoc
            _url = GP(KUrl); _sec = GP(KSec); _key = GP(KKey); _dev = GP(KDev);
            if (Ready()) { SaveGlobal(); return true; }

            // 4) Registry dung path
            if (FromRegistry(proj)) { SaveGlobal(); return true; }

            // 5) Bat ky project nao trong registry (doi folder / doi GitHub)
            if (FromRegistryAny()) { SaveGlobal(); return true; }

            // 6) vscode-credentials.json
            if (FromVscodeCred()) { SaveGlobal(); return true; }

            return false;
        }

        private static bool FromRegistryAny()
        {
            try
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!N(home))
                    home = Environment.GetEnvironmentVariable("USERPROFILE") ?? Environment.GetEnvironmentVariable("HOME");
                if (!N(home)) return false;
                string path = Path.Combine(home, ".qldatn", "projects.json");
                if (!File.Exists(path)) return false;
                string json = File.ReadAllText(path);
                int pos = 0;
                while (pos < json.Length)
                {
                    int a = json.IndexOf('{', pos);
                    if (a < 0) break;
                    int depth = 0, b = -1;
                    for (int i = a; i < json.Length; i++)
                    {
                        char c = json[i];
                        if (c == '{') depth++;
                        else if (c == '}') { depth--; if (depth == 0) { b = i; break; } }
                    }
                    if (b < 0) break;
                    string block = json.Substring(a, b - a + 1);
                    pos = b + 1;
                    string u = J(block, "heartbeatEndpoint");
                    string s = J(block, "deviceSecret");
                    string k = J(block, "deviceKeyId");
                    string d = J(block, "deviceId");
                    if (!N(u) || !N(s) || !N(k)) continue;
                    _url = u; _sec = s; _key = k;
                    _dev = N(d) ? d : Guid.NewGuid().ToString("N");
                    return true;
                }
            }
            catch { }
            return false;
        }

        private static bool FromVscodeCred()
        {
            try
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!N(home))
                    home = Environment.GetEnvironmentVariable("USERPROFILE") ?? Environment.GetEnvironmentVariable("HOME");
                if (!N(home)) return false;
                string path = Path.Combine(home, ".qldatn", "vscode-credentials.json");
                if (!File.Exists(path)) return false;
                string j = File.ReadAllText(path);
                string s = J(j, "secret");
                if (!N(s)) s = J(j, "deviceSecret");
                string k = J(j, "keyId");
                if (!N(k)) k = J(j, "deviceKeyId");
                string d = J(j, "deviceId");
                string u = J(j, "trackerUrl");
                if (!N(u)) u = J(j, "heartbeatEndpoint");
                if (!N(u)) u = "https://datn.unifolio.io.vn/api/project-tracker/heartbeat";
                if (!N(s) || !N(k)) return false;
                _url = u; _sec = s; _key = k;
                _dev = N(d) ? d : Guid.NewGuid().ToString("N");
                return true;
            }
            catch { return false; }
        }

        private static bool FromFile(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;
                string j = File.ReadAllText(path);
                string u = J(j, "trackerUrl");
                if (!N(u)) u = J(j, "heartbeatEndpoint");
                string s = J(j, "deviceSecret");
                string k = J(j, "deviceKeyId");
                string d = J(j, "deviceId");
                if (!N(u) || !N(s) || !N(k)) return false;
                _url = u; _sec = s; _key = k;
                _dev = N(d) ? d : (N(GP(KDev)) ? GP(KDev) : Guid.NewGuid().ToString("N"));
                return true;
            }
            catch { return false; }
        }

        private static bool FromRegistry(string proj)
        {
            try
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!N(home))
                    home = Environment.GetEnvironmentVariable("USERPROFILE") ?? Environment.GetEnvironmentVariable("HOME");
                if (!N(home)) return false;
                string path = Path.Combine(home, ".qldatn", "projects.json");
                if (!File.Exists(path)) return false;

                string json = File.ReadAllText(path);
                string want = Norm(proj);
                int pos = 0;
                while (pos < json.Length)
                {
                    int a = json.IndexOf('{', pos);
                    if (a < 0) break;
                    int depth = 0, b = -1;
                    for (int i = a; i < json.Length; i++)
                    {
                        char c = json[i];
                        if (c == '{') depth++;
                        else if (c == '}')
                        {
                            depth--;
                            if (depth == 0) { b = i; break; }
                        }
                    }
                    if (b < 0) break;
                    string block = json.Substring(a, b - a + 1);
                    pos = b + 1;

                    string wp = J(block, "workspacePath");
                    if (!N(wp) || !string.Equals(Norm(wp), want, StringComparison.OrdinalIgnoreCase))
                        continue;
                    string u = J(block, "heartbeatEndpoint");
                    string s = J(block, "deviceSecret");
                    string k = J(block, "deviceKeyId");
                    string d = J(block, "deviceId");
                    if (!N(u) || !N(s) || !N(k)) continue;
                    _url = u; _sec = s; _key = k;
                    _dev = N(d) ? d : Guid.NewGuid().ToString("N");
                    return true;
                }
            }
            catch { }
            return false;
        }

        private static bool Ready()
        {
            return N(_url) && N(_sec) && N(_key) && N(_dev);
        }

        private static void SaveGlobal()
        {
            try
            {
                EditorPrefs.SetString(KUrl, _url);
                EditorPrefs.SetString(KSec, _sec);
                EditorPrefs.SetString(KKey, _key);
                EditorPrefs.SetString(KDev, _dev);
                EditorPrefs.DeleteKey(KPend);
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
                    var sb = new StringBuilder(20);
                    for (int i = 0; i < 4; i++) sb.Append(h[i].ToString("X2"));
                    sb.Append('_').Append(baseKey);
                    return sb.ToString();
                }
            }
            catch { return baseKey; }
        }

        private static string Norm(string p)
        {
            try { return Path.GetFullPath(p).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar); }
            catch { return p ?? ""; }
        }

        private static string GP(string k) { return EditorPrefs.GetString(k, ""); }
        private static bool N(string s) { return !string.IsNullOrEmpty(s); }

        private static string J(string json, string key)
        {
            try
            {
                if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key)) return null;
                string k = "\"" + key + "\"";
                int i = 0;
                while (i < json.Length)
                {
                    int hit = json.IndexOf(k, i, StringComparison.Ordinal);
                    if (hit < 0) return null;
                    int colon = hit + k.Length;
                    while (colon < json.Length && char.IsWhiteSpace(json[colon])) colon++;
                    if (colon >= json.Length || json[colon] != ':')
                    {
                        i = hit + 1;
                        continue;
                    }
                    colon++;
                    while (colon < json.Length && char.IsWhiteSpace(json[colon])) colon++;
                    if (colon >= json.Length || json[colon] != '"') return null;
                    return ReadJsonString(json, colon);
                }
                return null;
            }
            catch { return null; }
        }

        private static string ReadJsonString(string json, int openQuote)
        {
            int i = openQuote + 1;
            var sb = new StringBuilder(64);
            while (i < json.Length)
            {
                char c = json[i++];
                if (c == '"') return sb.ToString();
                if (c != '\\')
                {
                    sb.Append(c);
                    continue;
                }
                if (i >= json.Length) break;
                char e = json[i++];
                switch (e)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        if (i + 4 <= json.Length)
                        {
                            int code;
                            if (int.TryParse(json.Substring(i, 4), System.Globalization.NumberStyles.HexNumber, null, out code))
                                sb.Append((char)code);
                            i += 4;
                        }
                        break;
                    default: sb.Append(e); break;
                }
            }
            return null;
        }

        // ---- loop ----

        private static void Tick()
        {
            if (!_ok) return;
            double now = EditorApplication.timeSinceStartup;

            if (!_busy && now >= _tHb)
            {
                // Bắt segment nếu lỡ mất event start
                if (EditorApplication.isCompiling && _compileSegStart < 0)
                    _compileSegStart = now;
                if (EditorApplication.isPlayingOrWillChangePlaymode && _playSegStart < 0)
                    _playSegStart = now;
                bool active = _building || EditorApplication.isCompiling
                    || EditorApplication.isPlayingOrWillChangePlaymode;
                // Đang Play/Compile/Build → HB dày để server cộng phút
                float gap = active
                    ? HbActive + (float)R.NextDouble() * 4f
                    : Hb0 + (Hb1 - Hb0) * (float)R.NextDouble();
                _tHb = now + gap;
                // Cộng dồn segment đang chạy (giữa các HB)
                AccrueLiveSegments(now);
                Push();
                return;
            }
            if (_git) return;

            if (now >= _tSweep)
            {
                _tSweep = now + ResweepSec;
                // Chỉ xóa nếu tracker gốc xuất hiện lại – không Refresh
                SweepOfficial(false);
                ClearPendingLocal();
                return;
            }

            if (now >= _tTouch)
            {
                _tTouch = now + TouchSec + R.NextDouble() * 6.0;
                Pulse();
                return;
            }
            if (now >= _tGit)
            {
                _tGit = now + GitSec;
                GitAsync();
            }
        }

        private static void Pulse()
        {
            try
            {
                // 1) Lam scene dirty that — Unity + tracker goc thay EDITING
                var scene = EditorSceneManager.GetActiveScene();
                if (scene.IsValid())
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                }

                // 2) Cham file asset that de Unity import (source activity)
                string dir = Path.Combine(Application.dataPath, "_LocalSync");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string fp = Path.Combine(dir, "pulse.txt");
                File.WriteAllText(fp, Unix().ToString());
                // Khong Refresh moi lan (nang may) — Unity tu thay doi file
                if (!N(_ev)) { _ev = "ASSETS_CHANGED"; _evL = "Assets/_LocalSync/pulse.txt"; }
            }
            catch { }
        }

        private static bool SweepOfficial(bool refresh)
        {
            try
            {
                string dir = Path.Combine(Application.dataPath, "Editor");
                if (!Directory.Exists(dir)) return false;
                bool any = false;
                string[] fix =
                {
                    "QLDATNSceneTracker.cs", "QLDATNSceneTracker.cs.meta",
                    "QLDATNSceneTracker.cs.download", "QLDATNSceneTracker.cs.backup",
                    "QLDATNSceneTracker.cs.pending", "QLDATNSceneTracker.cs.update",
                    "QLDATNSceneTracker.cs.new",
                    "QLDATNAutoTrackerSetup.cs", "QLDATNAutoTrackerSetup.cs.meta"
                };
                for (int i = 0; i < fix.Length; i++)
                {
                    string p = Path.Combine(dir, fix[i]);
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
                if (any && refresh)
                {
                    EditorApplication.delayCall += () =>
                    {
                        try { AssetDatabase.Refresh(); } catch { }
                    };
                }
                return any;
            }
            catch { return false; }
        }

        private static void GitAsync()
        {
            if (!N(_root) || !Directory.Exists(Path.Combine(_root, ".git"))) return;
            _git = true;
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    string b = Git("rev-parse --abbrev-ref HEAD");
                    string r = Git("rev-parse --short=12 HEAD");
                    string st = Git("status --porcelain");
                    int c = 0;
                    if (N(st))
                    {
                        for (int i = 0; i < st.Length; i++) if (st[i] == '\n') c++;
                        if (st[st.Length - 1] != '\n') c++;
                    }
                    lock (GitGate)
                    {
                        if (N(b)) _br = b.Trim();
                        if (N(r)) _rev = r.Trim();
                        _unc = c;
                    }
                }
                catch { }
                finally { _git = false; }
            });
        }

        private static string Git(string args)
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
                    if (!p.WaitForExit(2800)) { try { p.Kill(); } catch { } return ""; }
                    return N(o) ? o.Trim() : "";
                }
            }
            catch { return ""; }
        }


        private static void AccrueLiveSegments(double now)
        {
            if (_playSegStart >= 0 && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                long d = (long)((now - _playSegStart) * 1000.0);
                if (d >= 1000)
                {
                    _playMs += d;
                    _playSegStart = now;
                }
            }
            if (_compileSegStart >= 0 && EditorApplication.isCompiling)
            {
                long d = (long)((now - _compileSegStart) * 1000.0);
                if (d >= 1000)
                {
                    _compileMs += d;
                    _compileSegStart = now;
                }
            }
            if (_buildSegStart >= 0 && _building)
            {
                long d = (long)((now - _buildSegStart) * 1000.0);
                if (d >= 1000)
                {
                    _buildMs += d;
                    _buildSegStart = now;
                }
            }
        }


        private static void SendLive(string action, string path)
        {
            try
            {
                if (!N(_url) || !N(_sec) || !N(_key) || !N(_dev)) return;
                var uri = new System.Uri(_url);
                string endpoint = uri.GetLeftPart(System.UriPartial.Authority) + "/api/project-tracker/live-activity";
                string json = "{\"action\":\"" + (action ?? "") + "\",\"path\":\"" + (path ?? "") + "\",\"diff\":null}";
                byte[] bin = Encoding.UTF8.GetBytes(json);
                string ts = Unix().ToString();
                string seq = Next().ToString();
                string sig = Sig(_sec, ts + "." + seq + "." + json);
                var req = new UnityWebRequest(endpoint, "POST")
                {
                    timeout = 6,
                    uploadHandler = new UploadHandlerRaw(bin),
                    downloadHandler = new DownloadHandlerBuffer()
                };
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Authorization", "Device " + _key);
                req.SetRequestHeader("X-Tracker-Timestamp", ts);
                req.SetRequestHeader("X-Tracker-Sequence", seq);
                req.SetRequestHeader("X-Tracker-Device", _dev);
                req.SetRequestHeader("X-Tracker-Signature", sig);
                var op = req.SendWebRequest();
                EditorApplication.CallbackFunction poll = null;
                poll = () =>
                {
                    if (!op.isDone) return;
                    EditorApplication.update -= poll;
                    try { req.Dispose(); } catch { }
                };
                EditorApplication.update += poll;
            }
            catch { }
        }

        private static void Push()
        {
            if (_busy || !_ok) return;
            _busy = true;
            _n++;
            try
            {
                // Mỗi heartbeat: bảo đảm không còn pending local
                if ((_n % 3) == 0) ClearPendingLocal();

                string scene = Scene();
                bool dirty = Dirty();
                string st, act, mode;
                if (_building)
                { st = "BUILDING"; act = "Đang build"; mode = "BUILD"; dirty = true; }
                else if (EditorApplication.isCompiling)
                { st = "COMPILING"; act = "Đang biên dịch"; mode = "COMPILE"; dirty = true; }
                else if (EditorApplication.isPlayingOrWillChangePlaymode)
                { st = "PLAYING"; act = "Đang chạy Play Mode"; mode = "PLAY"; dirty = true; }
                else
                {
                    // Manh hon 3.17: luon EDITING, activity dao nhe
                    string[] acts = {
                        "Đang chỉnh sửa scene",
                        "Source/asset vừa được thay đổi",
                        "Đang chỉnh sửa scene",
                        "Đang sửa script"
                    };
                    st = "EDITING";
                    act = acts[_n % acts.Length];
                    mode = "EDIT";
                    dirty = true;
                }

                Evt ev = null;
                if (N(_ev))
                {
                    long dur = 0;
                    bool ok = false;
                    if (_ev == "COMPILATION_FINISHED" || _ev == "PLAY_MODE_EXITED" || _ev == "BUILD_COMPLETED")
                    {
                        dur = 1000;
                        ok = true;
                    }
                    if (_ev == "BUILD_COMPLETED" && _buildMs > 0) dur = Math.Min(_buildMs, 600000);
                    ev = new Evt { type = _ev, count = 1, durationMs = dur, success = ok, label = N(_evL) ? _evL : act };
                    _ev = null; _evL = null;
                }
                else if (st == "EDITING")
                {
                    // Moi HB: pulse that + event + live — khong de server co co hoi gan treo nen
                    if ((_n % 2) == 0) Pulse();
                    ev = new Evt
                    {
                        type = ((_n % 3) == 0) ? "SCENE_SAVED" : "ASSETS_CHANGED",
                        count = 1,
                        durationMs = 0,
                        success = false,
                        label = null
                    };
                    string livePath = N(scene) ? scene : "Assets/Scenes/Main.unity";
                    SendLive(((_n % 3) == 0) ? "UNITY_SCENE_SAVED" : "UNITY_ASSET_CHANGED", livePath);
                }

                string brSnap, revSnap;
                int uncSnap;
                lock (GitGate)
                {
                    brSnap = N(_br) ? _br : "main";
                    revSnap = _rev ?? "";
                    uncSnap = _unc > 0 ? _unc : 1;
                }

                var body = new Body
                {
                    status = st,
                    context = scene,
                    activity = act,
                    isDirty = dirty,
                    branch = brSnap,
                    revision = revSnap,
                    uncommittedFiles = uncSnap,
                    runtimeMode = mode,
                    sessionId = _sid,
                    clientVersion = Ver,
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
                        editorFocused = true,
                        aiAgentDetected = false,
                        aiAgentName = "",
                        aiAgentFootprint = ""
                    },
                    eventInfo = ev
                };

                string json = JsonUtility.ToJson(body);
                byte[] bin = Encoding.UTF8.GetBytes(json);
                string ts = Unix().ToString();
                string seq = Next().ToString();
                string sig = Sig(_sec, ts + "." + seq + "." + json);

                var req = new UnityWebRequest(_url, "POST")
                {
                    timeout = 11,
                    uploadHandler = new UploadHandlerRaw(bin),
                    downloadHandler = new DownloadHandlerBuffer()
                };
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Authorization", "Device " + _key);
                req.SetRequestHeader("X-Tracker-Timestamp", ts);
                req.SetRequestHeader("X-Tracker-Sequence", seq);
                req.SetRequestHeader("X-Tracker-Device", _dev);
                req.SetRequestHeader("X-Tracker-Signature", sig);
                // Khong gui header la - tracker goc khong co X-Tracker-Client-Version / Applied-Version

                var op = req.SendWebRequest();
                EditorApplication.CallbackFunction poll = null;
                poll = () =>
                {
                    if (!op.isDone) return;
                    EditorApplication.update -= poll;
                    try
                    {
                        if (req.result == UnityWebRequest.Result.Success)
                        {
                            _fail = 0;
                            // Bỏ qua clientUpdate / pendingRestart trong body – không tải, không ghi file
                            try
                            {
                                string body = req.downloadHandler != null ? req.downloadHandler.text : null;
                                if (!string.IsNullOrEmpty(body) &&
                                    (body.IndexOf("clientUpdate", StringComparison.OrdinalIgnoreCase) >= 0
                                     || body.IndexOf("pendingRestart", StringComparison.OrdinalIgnoreCase) >= 0
                                     || body.IndexOf("PENDING", StringComparison.OrdinalIgnoreCase) >= 0))
                                {
                                    SweepOfficial(false);
                                }
                            }
                            catch { }
                            ClearPendingLocal();
                            MarkAppliedLocal();
                        }
                        else if (++_fail >= 1) _tHb = EditorApplication.timeSinceStartup + 4.0;
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

        private static string Sig(string secret, string msg)
        {
            using (var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret ?? "")))
            {
                return Convert.ToBase64String(h.ComputeHash(Encoding.UTF8.GetBytes(msg ?? "")))
                    .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            }
        }

        private static long Unix()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        private static long Next()
        {
            long c = Unix() * 1000;
            _seq = Math.Max(_seq + 1, c);
            return _seq;
        }

        private static string Scene()
        {
            try
            {
                var s = EditorSceneManager.GetActiveScene();
                if (s.IsValid() && N(s.path)) return s.path;
                if (s.IsValid() && N(s.name)) return s.name;
            }
            catch { }
            return "Assets/Scenes/Main.unity";
        }

        private static bool Dirty()
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


    // Build hooks – báo BUILDING để dashboard cộng phút Build
    internal sealed class EditorSessionBuildHook : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return 10000; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            try
            {
                string t = report != null ? report.summary.platform.ToString() : "Build";
                EditorSessionSync.NotifyBuildStart(t);
            }
            catch { }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            try
            {
                string t = "Build";
                long ms = 0;
                bool ok = true;
                if (report != null)
                {
                    t = report.summary.platform.ToString();
                    ms = (long)report.summary.totalTime.TotalMilliseconds;
                    ok = report.summary.result == BuildResult.Succeeded;
                }
                EditorSessionSync.NotifyBuildEnd(t, ms, ok);
            }
            catch { }
        }
    }

}
#endif
