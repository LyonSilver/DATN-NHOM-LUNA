#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using ChaosBossIntro;

namespace ChaosBossIntro.Editor
{
    public sealed class ChaosBossIntroBuilder : EditorWindow
    {
        const string SceneFolder = "Assets/ChaosBossIntroGenerated";
        const string ScenePath = SceneFolder + "/ChaosBossIntro.unity";
        Sprite sky, vortex, ruins, arena, explosion;
        [MenuItem("Tools/Chaos Fantasy/Boss Intro Auto Builder")]
        static void Open() { GetWindow<ChaosBossIntroBuilder>("Boss Intro Builder"); }
        void OnGUI()
        {
            EditorGUILayout.LabelField("CHAOS BOSS INTRO - FADE IN", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Build creates a NEW scene. It does not edit your current scene or source textures. Assign sprites below.", MessageType.Info);
            sky = (Sprite)EditorGUILayout.ObjectField("Sky background", sky, typeof(Sprite), false);
            vortex = (Sprite)EditorGUILayout.ObjectField("Vortex (optional)", vortex, typeof(Sprite), false);
            ruins = (Sprite)EditorGUILayout.ObjectField("Ruins walls", ruins, typeof(Sprite), false);
            arena = (Sprite)EditorGUILayout.ObjectField("Arena floor", arena, typeof(Sprite), false);
            explosion = (Sprite)EditorGUILayout.ObjectField("Explosion ring (optional)", explosion, typeof(Sprite), false);
            if (GUILayout.Button("Auto Find Sprites")) AutoFind();
            EditorGUILayout.Space();
            if (GUILayout.Button("BUILD NEW BOSS INTRO SCENE", GUILayout.Height(36))) Build();
            if (GUILayout.Button("PREVIEW (Play Mode)")) Preview();
            if (GUILayout.Button("RESET PREVIEW (Stop Play Mode)")) ResetPreview();
            if (GUILayout.Button("OPEN GENERATED SCENE")) OpenScene();
            EditorGUILayout.HelpBox("Player/Boss/HUD are optional: assign them to the controller in the generated scene. Do not assign gameplay objects to a different scene.", MessageType.None);
        }
        void AutoFind()
        {
            sky = sky ? sky : FindSprite("Sky");
            vortex = vortex ? vortex : FindSprite("Vortex");
            ruins = ruins ? ruins : FindSprite("Ruins");
            arena = arena ? arena : FindSprite("Arena");
            explosion = explosion ? explosion : FindSprite("Explosion");
        }
        static Sprite FindSprite(string keyword)
        {
            string[] guids = AssetDatabase.FindAssets("t:Sprite " + keyword);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (s) return s;
                }
            }
            return null;
        }
        static SpriteRenderer AddSprite(Transform parent, string name, Sprite sprite, int order, Vector3 pos, float width)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            if (sprite && sprite.bounds.size.x > 0) go.transform.localScale = Vector3.one * (width / sprite.bounds.size.x);
            return sr;
        }
        static Transform Anchor(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            return go.transform;
        }
        void Build()
        {
            if (!sky || !ruins || !arena)
            {
                EditorUtility.DisplayDialog("Missing sprites", "Assign Sky, Ruins and Arena sprites first.", "OK");
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath))
            {
                EditorUtility.DisplayDialog("Scene already exists", "Generated scene already exists. It will NOT be overwritten. Move or rename it manually to create another.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!AssetDatabase.IsValidFolder(SceneFolder)) AssetDatabase.CreateFolder("Assets", "ChaosBossIntroGenerated");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("ChaosBossIntro_ROOT");
            var env = Anchor(root.transform, "Environment", Vector3.zero);
            var srSky = AddSprite(env, "Sky_STATIC", sky, -30, new Vector3(0, 1.4f, 0), 19.2f);
            var srVortex = AddSprite(env, "Vortex_ROTATING", vortex, -29, new Vector3(0, 2.2f, 0), 8f);
            var srRuins = AddSprite(env, "Ruins_FADE", ruins, -20, new Vector3(0, -0.3f, 0), 19.2f);
            var srArena = AddSprite(env, "Arena_FADE", arena, -10, new Vector3(0, -3.0f, 0), 15f);
            var srExplosion = AddSprite(env, "Explosion_OPTIONAL", explosion, -9, new Vector3(0, -1.3f, 0), 5f);
            var origin = Anchor(root.transform, "Lightning_Origin", new Vector3(0, 3.8f, 0));
            var target = Anchor(root.transform, "Lightning_Impact", new Vector3(0, -1.3f, 0));
            var lightningGo = new GameObject("Lightning_Procedural");
            lightningGo.transform.SetParent(root.transform, false);
            var line = lightningGo.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.sortingOrder = -15;
            line.numCapVertices = 3;
            line.numCornerVertices = 2;
            line.positionCount = 2;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader)
            {
                const string matPath = SceneFolder + "/BossIntro_Lightning.mat";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (!mat)
                {
                    mat = new Material(shader) { name = "BossIntro_Lightning" };
                    AssetDatabase.CreateAsset(mat, matPath);
                }
                line.sharedMaterial = mat;
            }
            var cameraGo = new GameObject("BossIntro_Camera");
            cameraGo.transform.position = new Vector3(0, 0, -10);
            var cam = cameraGo.AddComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.035f, 0.015f, 0.055f);
            cameraGo.tag = "MainCamera";
            var controller = root.AddComponent<ChaosBossIntroController>();
            controller.sky = srSky;
            controller.vortex = vortex ? srVortex : null;
            controller.ruins = srRuins;
            controller.arena = srArena;
            controller.explosion = explosion ? srExplosion : null;
            controller.lightningOrigin = origin;
            controller.lightningTarget = target;
            controller.lightning = line;
            // Keep optional sprite objects inactive if none was assigned.
            if (!vortex) srVortex.gameObject.SetActive(false);
            if (!explosion) srExplosion.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                EditorUtility.DisplayDialog("Save failed", "Could not save generated scene.", "OK");
                return;
            }
            Selection.activeObject = root;
            EditorGUIUtility.PingObject(root);
            Debug.Log("[ChaosBossIntro] New scene created: " + ScenePath);
        }
        static void OpenScene()
        {
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)) { Debug.LogWarning("Build scene first."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
        static void Preview()
        {
            if (EditorApplication.isPlaying) return;
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)) { Debug.LogWarning("Build scene first."); return; }
            OpenScene();
            if (SceneManager.GetActiveScene().path == ScenePath) EditorApplication.isPlaying = true;
        }
        static void ResetPreview()
        {
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
            else OpenScene();
        }
    }
}
#endif
