#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChaosBossArena.Editor
{
    public sealed class ChaosBossAutoBuilderWindow : EditorWindow
    {
        Sprite sky, ruins, arena, vortex;
        string outputPath = "Assets/ChaosBossArenaGenerated/ChaosBossArena.unity";
        float cameraSize = 5.4f;
        float skyWidth = 21f, ruinsWidth = 20f, arenaWidth = 18f, vortexWidth = 10f;
        Vector2 skyPosition = new Vector2(0f, 1.2f);
        Vector2 ruinsPosition = new Vector2(0f, .5f);
        Vector2 arenaPosition = new Vector2(0f, -2.5f);
        Vector2 vortexPosition = new Vector2(1.2f, 2.5f);
        bool createHUD = true;

        [MenuItem("Tools/Chaos Fantasy/Boss Arena Auto Builder")]
        public static void Open() { GetWindow<ChaosBossAutoBuilderWindow>("Boss Arena Builder").minSize = new Vector2(410, 560); }

        void OnGUI()
        {
            EditorGUILayout.LabelField("CHAOS BOSS ARENA | AUTO BUILDER", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Creates a NEW scene. Does not overwrite existing scenes or modify source sprites. Assign four transparent PNG sprites; vortex is optional.", MessageType.Info);
            sky = (Sprite)EditorGUILayout.ObjectField("Sky (static)", sky, typeof(Sprite), false);
            ruins = (Sprite)EditorGUILayout.ObjectField("Ruins / Walls", ruins, typeof(Sprite), false);
            arena = (Sprite)EditorGUILayout.ObjectField("Arena Floor", arena, typeof(Sprite), false);
            vortex = (Sprite)EditorGUILayout.ObjectField("Vortex (transparent)", vortex, typeof(Sprite), false);
            if (GUILayout.Button("Auto Find Sprites")) AutoFind();
            EditorGUILayout.Space(8);
            outputPath = EditorGUILayout.TextField("Scene Output", outputPath);
            cameraSize = EditorGUILayout.Slider("Camera Orthographic Size", cameraSize, 3f, 12f);
            skyWidth = EditorGUILayout.Slider("Sky Width", skyWidth, 8f, 35f);
            ruinsWidth = EditorGUILayout.Slider("Ruins Width", ruinsWidth, 8f, 35f);
            arenaWidth = EditorGUILayout.Slider("Arena Width", arenaWidth, 8f, 35f);
            vortexWidth = EditorGUILayout.Slider("Vortex Width", vortexWidth, 2f, 25f);
            skyPosition = EditorGUILayout.Vector2Field("Sky Position", skyPosition);
            ruinsPosition = EditorGUILayout.Vector2Field("Ruins Position", ruinsPosition);
            arenaPosition = EditorGUILayout.Vector2Field("Arena Position", arenaPosition);
            vortexPosition = EditorGUILayout.Vector2Field("Vortex Position", vortexPosition);
            createHUD = EditorGUILayout.Toggle("Create Card Battle HUD Mockup", createHUD);
            EditorGUILayout.Space(12);
            using (new EditorGUI.DisabledScope(sky == null || ruins == null || arena == null))
                if (GUILayout.Button("BUILD NEW BOSS SCENE", GUILayout.Height(44))) Build();
            EditorGUILayout.HelpBox("The HUD is a layout placeholder only: it does not implement combat, turns, cards, or HP logic. Place actual Player/Boss prefabs at the generated anchors.", MessageType.None);
        }

        void AutoFind()
        {
            sky = sky != null ? sky : FindSprite("Sky");
            ruins = ruins != null ? ruins : FindSprite("Ruins", "Wall");
            arena = arena != null ? arena : FindSprite("Arena", "Floor");
            vortex = vortex != null ? vortex : FindSprite("Vortex", "Swirl");
        }
        static Sprite FindSprite(params string[] names)
        {
            foreach (string name in names)
            {
                string[] guids = AssetDatabase.FindAssets(name + " t:Sprite", new[] { "Assets" });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileNameWithoutExtension(path).IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        if (s != null) return s;
                    }
                }
            }
            return null;
        }
        static GameObject Node(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }
        static GameObject Layer(string name, Transform parent, Sprite sprite, int order, Vector2 position, float width)
        {
            GameObject go = Node(name, parent);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            go.transform.localPosition = new Vector3(position.x, position.y, 0f);
            if (sprite != null && sprite.bounds.size.x > .001f)
            {
                float scale = width / sprite.bounds.size.x;
                go.transform.localScale = new Vector3(scale, scale, 1f);
            }
            return go;
        }
        static Text HudText(string name, Transform parent, string value, Vector2 anchor, Vector2 size, int fontSize)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(.5f, .5f);
            rt.sizeDelta = size; rt.anchoredPosition = Vector2.zero;
            Text t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize; t.alignment = TextAnchor.MiddleCenter;
            t.color = new Color(.93f, .85f, .81f, 1f); t.text = value;
            return t;
        }
        static void MakeHud(Transform parent)
        {
            GameObject go = new GameObject("CardBattleHUD_Placeholder", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            Canvas canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            HudText("BossName", go.transform, "BOSS  |  CHAOS SOVEREIGN", new Vector2(.5f, .94f), new Vector2(900, 70), 32);
            HudText("Turn", go.transform, "TURN 01", new Vector2(.1f, .92f), new Vector2(260, 70), 26);
            HudText("PlayerHP", go.transform, "PLAYER HP / MANA", new Vector2(.15f, .13f), new Vector2(450, 70), 28);
            HudText("HandCards", go.transform, "[ CARD 1 ]    [ CARD 2 ]    [ CARD 3 ]    [ CARD 4 ]    [ CARD 5 ]", new Vector2(.5f, .08f), new Vector2(1150, 95), 27);
            HudText("EndTurn", go.transform, "END TURN", new Vector2(.9f, .12f), new Vector2(280, 80), 28);
        }
        void Build()
        {
            if (sky == null || ruins == null || arena == null) { EditorUtility.DisplayDialog("Missing sprites", "Assign Sky, Ruins, and Arena sprites.", "OK"); return; }
            if (!outputPath.StartsWith("Assets/", StringComparison.Ordinal) || !outputPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
            { EditorUtility.DisplayDialog("Invalid path", "Scene path must start with Assets/ and end with .unity", "OK"); return; }
            string finalPath = AssetDatabase.GenerateUniqueAssetPath(outputPath);
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = Node("ChaosBossArena_AutoBuilt");
            Transform environment = Node("Environment", root.transform).transform;
            Layer("Sky_Background_STATIC", environment, sky, -30, skyPosition, skyWidth);
            if (vortex != null)
            {
                GameObject swirl = Layer("Sky_Vortex_ROTATING", environment, vortex, -29, vortexPosition, vortexWidth);
                swirl.AddComponent<ChaosVortexMotion>();
            }
            Layer("Ruins_Walls", environment, ruins, -20, ruinsPosition, ruinsWidth);
            Layer("Arena_Floor", environment, arena, -10, arenaPosition, arenaWidth);
            Transform anchors = Node("GameplayAnchors", root.transform).transform;
            Node("PlayerSpawn", anchors).transform.position = new Vector3(-4.3f, -1.6f, 0f);
            Node("BossSpawn", anchors).transform.position = new Vector3(4.3f, -.9f, 0f);
            Node("VFX_Fog_Optional", root.transform);
            if (createHUD) MakeHud(root.transform);
            GameObject cameraObject = Node("BossArenaCamera", root.transform);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = cameraSize;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.065f, .035f, .09f);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            string directory = Path.GetDirectoryName(finalPath).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(directory))
            {
                string current = "Assets";
                string[] parts = directory.Substring("Assets".Length).Trim('/').Split('/');
                foreach (string part in parts)
                {
                    if (string.IsNullOrEmpty(part)) continue;
                    string next = current + "/" + part;
                    if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, part);
                    current = next;
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, finalPath);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Scene created", "New boss scene saved:\n" + finalPath, "OK");
            Selection.activeGameObject = root;
        }
    }
}
#endif
