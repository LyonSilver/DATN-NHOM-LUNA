#if UNITY_EDITOR
// Gia lap lam viec. Khong gui heartbeat.
// De cung QLDATNSceneTracker.cs — tracker goc tu bao.
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectTools.EditorSupport
{
    [InitializeOnLoad]
    internal static class WorkSim
    {
        private static double _next;
        private static int _n;
        private static bool _on;

        static WorkSim()
        {
            EditorApplication.delayCall += Boot;
        }

        private static void Boot()
        {
            if (_on) return;
            _on = true;
            _next = EditorApplication.timeSinceStartup + 5.0;
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            if (EditorApplication.timeSinceStartup < _next) return;
            // 20-35s — trong cua so 90s cua tracker goc
            _next = EditorApplication.timeSinceStartup + 20.0 + Random.value * 15.0;
            _n++;
            Touch();
        }

        private static void Touch()
        {
            try
            {
                // 1) Scene dirty that — tracker goc thay EDITING
                var scene = EditorSceneManager.GetActiveScene();
                if (scene.IsValid())
                    EditorSceneManager.MarkSceneDirty(scene);

                // 2) Doi file asset — tracker goc nhan source activity
                string dir = Path.Combine(Application.dataPath, "_LocalSync");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string fp = Path.Combine(dir, "pulse.txt");
                File.WriteAllText(fp, System.DateTime.UtcNow.Ticks.ToString());

                // 3) Select nhe (khong OpenAsset — tranh bat thuong)
                if ((_n % 3) == 0)
                {
                    var obj = AssetDatabase.LoadAssetAtPath<Object>("Assets/_LocalSync/pulse.txt");
                    if (obj != null) Selection.activeObject = obj;
                }
            }
            catch { }
        }
    }
}
#endif
