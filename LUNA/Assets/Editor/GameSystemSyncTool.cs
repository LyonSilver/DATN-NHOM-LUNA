using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class GameSystemSyncTool : EditorWindow
{
    private Vector2 scroll;
    private List<string> scenePaths = new List<string>();
    private Dictionary<string, SceneCheckResult> results = new Dictionary<string, SceneCheckResult>();

    private string[] requiredManagers = new string[]
    {
        "SaveLoadManager",
        "AutoSaveManager",
        "CoinManager",
        "Slot2Lock",
        "SaveNotificationUI"
    };

    [System.Serializable]
    public class SceneCheckResult
    {
        public string sceneName;
        public bool hasSaveLoadManager;
        public bool hasAutoSaveManager;
        public bool hasCoinManager;
        public bool hasSlot2Lock;
        public bool hasSaveNotificationUI;
        public List<string> issues = new List<string>();
    }

    [MenuItem("Chaos Card/System Sync Tool")]
    public static void Open()
    {
        var window = GetWindow<GameSystemSyncTool>("System Sync Tool");
        window.minSize = new Vector2(520, 480);
        window.Show();
    }

    private void OnEnable()
    {
        RefreshSceneList();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);
        GUILayout.Label("GAME SYSTEM SYNC TOOL", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool checks all scenes in Build Settings and reports missing Managers.\n" +
            "Recommended: Keep Managers only in one Bootstrap scene (DontDestroyOnLoad).",
            MessageType.Info);

        EditorGUILayout.Space(6);

        if (GUILayout.Button("Refresh Scene List", GUILayout.Height(28)))
            RefreshSceneList();

        if (GUILayout.Button("CHECK ALL SCENES", GUILayout.Height(34)))
            CheckAllScenes();

        EditorGUILayout.Space(10);
        GUILayout.Label("Scenes in Build Settings:", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        if (results.Count == 0)
            EditorGUILayout.HelpBox("Not checked yet. Click CHECK ALL SCENES.", MessageType.None);
        else
        {
            foreach (var kvp in results)
                DrawSceneResult(kvp.Value);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(12);
        GUILayout.Label("Quick Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Open First Scene (Bootstrap)"))
        {
            if (scenePaths.Count > 0)
                EditorSceneManager.OpenScene(scenePaths[0]);
        }

        if (GUILayout.Button("Create Bootstrap Managers (in current scene)"))
            CreateBootstrapManagers();
    }

    private void RefreshSceneList()
    {
        scenePaths.Clear();
        results.Clear();

        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenePaths.Add(scene.path);
        }
        Repaint();
    }

    private void CheckAllScenes()
    {
        results.Clear();
        string currentScene = EditorSceneManager.GetActiveScene().path;

        foreach (string path in scenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var result = new SceneCheckResult { sceneName = scene.name };

            var allObjects = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var mb in allObjects)
            {
                if (mb == null) continue;
                string typeName = mb.GetType().Name;

                if (typeName == "SaveLoadManager") result.hasSaveLoadManager = true;
                if (typeName == "AutoSaveManager") result.hasAutoSaveManager = true;
                if (typeName == "CoinManager") result.hasCoinManager = true;
                if (typeName == "Slot2Lock") result.hasSlot2Lock = true;
                if (typeName == "SaveNotificationUI") result.hasSaveNotificationUI = true;
            }

            if (!result.hasSaveLoadManager) result.issues.Add("Missing SaveLoadManager");
            if (!result.hasCoinManager) result.issues.Add("Missing CoinManager");

            results[path] = result;
        }

        if (!string.IsNullOrEmpty(currentScene) && File.Exists(currentScene))
            EditorSceneManager.OpenScene(currentScene);

        Repaint();
        Debug.Log($"[System Sync Tool] Checked {results.Count} scenes.");
    }

    private void DrawSceneResult(SceneCheckResult r)
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label(r.sceneName, EditorStyles.boldLabel);

        DrawStatus("SaveLoadManager", r.hasSaveLoadManager);
        DrawStatus("CoinManager", r.hasCoinManager);
        DrawStatus("AutoSaveManager", r.hasAutoSaveManager);
        DrawStatus("Slot2Lock", r.hasSlot2Lock);
        DrawStatus("SaveNotificationUI", r.hasSaveNotificationUI);

        if (r.issues.Count > 0)
            EditorGUILayout.HelpBox(string.Join("\n", r.issues), MessageType.Warning);
        else
            EditorGUILayout.HelpBox("Looks good", MessageType.Info);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(4);
    }

    private void DrawStatus(string name, bool ok)
    {
        var color = ok ? Color.green : Color.gray;
        var old = GUI.color;
        GUI.color = color;
        GUILayout.Label((ok ? "● " : "○ ") + name);
        GUI.color = old;
    }

    private void CreateBootstrapManagers()
    {
        CreateManagerIfMissing("SaveLoadManager");
        CreateManagerIfMissing("AutoSaveManager");
        CreateManagerIfMissing("CoinManager");
        CreateManagerIfMissing("Slot2Lock");
        CreateManagerIfMissing("SaveNotificationUI");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[System Sync Tool] Bootstrap Managers created/checked.");
    }

    private void CreateManagerIfMissing(string typeName)
    {
        var all = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var mb in all)
        {
            if (mb != null && mb.GetType().Name == typeName)
            {
                Debug.Log($"{typeName} already exists.");
                return;
            }
        }

        System.Type type = null;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName);
            if (type != null) break;
        }

        if (type == null)
        {
            Debug.LogWarning($"Cannot create {typeName} because the class does not exist yet.");
            return;
        }

        GameObject go = new GameObject(typeName);
        go.AddComponent(type);
        Debug.Log($"Created {typeName}");
    }
}