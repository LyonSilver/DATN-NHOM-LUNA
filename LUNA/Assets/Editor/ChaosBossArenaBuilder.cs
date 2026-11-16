#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ChaosBossArenaBuilder : EditorWindow
{
    private const string Root = "Assets/ChaosBossArena/Art/";
    private float skyWidth = 22f;
    private float ruinsWidth = 21f;
    private float arenaWidth = 19f;
    private Vector2 skyOffset = new Vector2(0, 2.8f);
    private Vector2 ruinsOffset = new Vector2(0, 0.5f);
    private Vector2 arenaOffset = new Vector2(0, -3.1f);
    private float cameraSize = 6.2f;

    [MenuItem("Tools/Chaos Fantasy/Build Boss Arena")]
    private static void ShowWindow() => GetWindow<ChaosBossArenaBuilder>("Chaos Boss Arena");

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Chaos Fantasy - Boss Arena", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Builds a separate, non-destructive background root in the active scene. Save your scene first.", MessageType.Info);
        skyWidth = EditorGUILayout.FloatField("Sky width", skyWidth);
        skyOffset = EditorGUILayout.Vector2Field("Sky position", skyOffset);
        ruinsWidth = EditorGUILayout.FloatField("Ruins width", ruinsWidth);
        ruinsOffset = EditorGUILayout.Vector2Field("Ruins position", ruinsOffset);
        arenaWidth = EditorGUILayout.FloatField("Arena width", arenaWidth);
        arenaOffset = EditorGUILayout.Vector2Field("Arena position", arenaOffset);
        cameraSize = EditorGUILayout.FloatField("Camera orthographic size", cameraSize);
        if (GUILayout.Button("Import PNG as Sprites")) ImportSprites();
        if (GUILayout.Button("Build / Rebuild Background")) Build();
    }

    private static void ImportSprites()
    {
        foreach (string name in new[] { "Sky", "Ruins", "Arena" })
        {
            string path = Root + name + ".png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) { Debug.LogError("Missing PNG: " + path); continue; }
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 100;
            importer.SaveAndReimport();
        }
        Debug.Log("Chaos Boss Arena: sprite import complete.");
    }

    private void Build()
    {
        ImportSprites();
        Sprite sky = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Sky.png");
        Sprite ruins = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Ruins.png");
        Sprite arena = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Arena.png");
        if (!sky || !ruins || !arena) { Debug.LogError("Missing sprites. Copy the Art folder into Assets/ChaosBossArena/Art."); return; }

        // Create a new root; do not delete or modify existing scene objects.
        GameObject root = new GameObject("ChaosBossArena_Background");
        Undo.RegisterCreatedObjectUndo(root, "Create Chaos Boss Arena");
        CreateLayer(root.transform, "01_Sky", sky, -30, skyWidth, skyOffset);
        CreateLayer(root.transform, "02_Ruins", ruins, -20, ruinsWidth, ruinsOffset);
        CreateLayer(root.transform, "03_Arena", arena, -10, arenaWidth, arenaOffset);

        Camera cam = Camera.main;
        if (!cam)
        {
            GameObject cameraObject = new GameObject("ChaosBossArena_Camera");
            Undo.RegisterCreatedObjectUndo(cameraObject, "Create Arena Camera");
            cam = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<AudioListener>();
        }
        else Undo.RecordObject(cam, "Configure Arena Camera");
        cam.orthographic = true;
        cam.orthographicSize = Mathf.Max(1f, cameraSize);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.075f, 0.045f, 0.11f, 1f);
        // Avoid repositioning an existing gameplay camera.
        if (cam.gameObject.name == "ChaosBossArena_Camera") cam.transform.position = new Vector3(0, 0, -10);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = root;
        Debug.Log("Chaos Boss Arena created. Adjust child Transform values as needed; save your scene.");
    }

    private static void CreateLayer(Transform parent, string name, Sprite sprite, int order, float width, Vector2 offset)
    {
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(offset.x, offset.y, 0f);
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order;
        float spriteWidth = sprite.bounds.size.x;
        float scale = Mathf.Max(0.1f, width) / Mathf.Max(0.001f, spriteWidth);
        go.transform.localScale = new Vector3(scale, scale, 1f);
    }
}
#endif
