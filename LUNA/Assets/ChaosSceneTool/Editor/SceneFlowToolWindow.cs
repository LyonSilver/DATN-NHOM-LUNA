using System.Collections.Generic;
using System.Linq;
using ChaosCard.SceneTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ChaosCard.SceneTools.Editor
{
    public class SceneFlowToolWindow : EditorWindow
    {
        private readonly List<SceneAsset> sceneOrder = new List<SceneAsset>();
        private Vector2 sceneScroll;
        private Vector2 buttonScroll;
        private SceneButtonLinker controller;
        private string newLinkLabel = "New Button";
        private bool includeDisabledBuildScenes = true;

        [MenuItem("Chaos Card/Scene Flow Tool")]
        public static void Open() => GetWindow<SceneFlowToolWindow>("Scene Flow Tool");

        private void OnEnable() => ReadBuildScenes();

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("CHAOS CARD · SCENE FLOW TOOL", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Đánh số và sắp xếp scene, đồng bộ Build Settings, rồi tự gán hành động cho các Button do bạn chọn. Tool không tự sửa gameplay hoặc Save/Load.", MessageType.Info);

            DrawSceneSection();
            EditorGUILayout.Space(10);
            DrawButtonSection();
        }

        private void DrawSceneSection()
        {
            EditorGUILayout.LabelField("1. Scene list (thứ tự đánh số)", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Đọc từ Build Settings", GUILayout.Height(25))) ReadBuildScenes();
                if (GUILayout.Button("Thêm Scene", GUILayout.Height(25))) sceneOrder.Add(null);
            }

            sceneScroll = EditorGUILayout.BeginScrollView(sceneScroll, GUILayout.MinHeight(120), GUILayout.MaxHeight(260));
            for (int i = 0; i < sceneOrder.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label((i + 1).ToString("00"), GUILayout.Width(28));
                    sceneOrder[i] = (SceneAsset)EditorGUILayout.ObjectField(sceneOrder[i], typeof(SceneAsset), false);
                    GUI.enabled = i > 0;
                    if (GUILayout.Button("▲", GUILayout.Width(28))) Swap(i, i - 1);
                    GUI.enabled = i < sceneOrder.Count - 1;
                    if (GUILayout.Button("▼", GUILayout.Width(28))) Swap(i, i + 1);
                    GUI.enabled = true;
                    if (GUILayout.Button("−", GUILayout.Width(28))) { sceneOrder.RemoveAt(i); i--; }
                }
            }
            EditorGUILayout.EndScrollView();

            includeDisabledBuildScenes = EditorGUILayout.ToggleLeft("Giữ cả scene đang tắt trong Build Settings", includeDisabledBuildScenes);
            if (GUILayout.Button("Áp dụng thứ tự vào Build Settings", GUILayout.Height(30))) ApplyBuildScenes();
            EditorGUILayout.HelpBox("Số thứ tự là thứ tự trong danh sách. Scene đầu tiên trong Build Settings thường là scene khởi chạy khi build.", MessageType.None);
        }

        private void DrawButtonSection()
        {
            EditorGUILayout.LabelField("2. Gắn hành động cho Button tùy chỉnh", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                controller = (SceneButtonLinker)EditorGUILayout.ObjectField("Scene Button Linker", controller, typeof(SceneButtonLinker), true);
                if (GUILayout.Button("Tạo / chọn", GUILayout.Width(90))) CreateOrSelectController();
            }
            if (controller == null)
            {
                EditorGUILayout.HelpBox("Chọn component SceneButtonLinker trong scene, hoặc bấm Tạo / chọn để tạo một GameObject quản lý.", MessageType.Warning);
                return;
            }

            if (controller.gameObject.scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene())
                EditorGUILayout.HelpBox("Controller không nằm trong scene đang mở. Hãy chọn controller thuộc scene hiện tại để tránh sửa nhầm.", MessageType.Warning);

            using (new EditorGUILayout.HorizontalScope())
            {
                newLinkLabel = EditorGUILayout.TextField(newLinkLabel);
                if (GUILayout.Button("+ Thêm mapping", GUILayout.Width(115)))
                {
                    Undo.RecordObject(controller, "Add Scene Button Mapping");
                    controller.Links.Add(new SceneButtonLinker.ButtonLink { label = string.IsNullOrWhiteSpace(newLinkLabel) ? "New Button" : newLinkLabel });
                    EditorUtility.SetDirty(controller);
                }
            }

            buttonScroll = EditorGUILayout.BeginScrollView(buttonScroll, GUILayout.MinHeight(150), GUILayout.MaxHeight(360));
            for (int i = 0; i < controller.Links.Count; i++)
            {
                var link = controller.Links[i];
                if (link == null) continue;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"Button {i + 1:00}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Xóa", GUILayout.Width(45)))
                    {
                        Undo.RecordObject(controller, "Remove Scene Button Mapping");
                        controller.Links.RemoveAt(i);
                        EditorUtility.SetDirty(controller);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    }
                }
                EditorGUI.BeginChangeCheck();
                link.label = EditorGUILayout.TextField("Tên mapping", link.label);
                link.button = (Button)EditorGUILayout.ObjectField("Button", link.button, typeof(Button), true);
                link.action = (SceneButtonLinker.ButtonAction)EditorGUILayout.EnumPopup("Hành động", link.action);
                switch (link.action)
                {
                    case SceneButtonLinker.ButtonAction.LoadScene:
                        string[] names = sceneOrder.Where(s => s != null).Select(s => s.name).Distinct().ToArray();
                        if (names.Length == 0) EditorGUILayout.HelpBox("Thêm scene ở phần trên trước.", MessageType.Warning);
                        else
                        {
                            int selected = System.Array.IndexOf(names, link.sceneName);
                            if (selected < 0) selected = 0;
                            link.sceneName = names[EditorGUILayout.Popup("Scene đích", selected, names)];
                        }
                        break;
                    case SceneButtonLinker.ButtonAction.SetObjectActive:
                        link.targetObject = (GameObject)EditorGUILayout.ObjectField("GameObject", link.targetObject, typeof(GameObject), true);
                        link.activeState = EditorGUILayout.Toggle("Active", link.activeState);
                        break;
                    case SceneButtonLinker.ButtonAction.ToggleObject:
                        link.targetObject = (GameObject)EditorGUILayout.ObjectField("GameObject", link.targetObject, typeof(GameObject), true);
                        break;
                    case SceneButtonLinker.ButtonAction.OpenURL:
                        link.url = EditorGUILayout.TextField("URL", link.url);
                        break;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(controller, "Edit Scene Button Mapping");
                    EditorUtility.SetDirty(controller);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Lưu scene", GUILayout.Height(28)))
            {
                EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
                EditorSceneManager.SaveScene(controller.gameObject.scene);
            }
        }

        private void ReadBuildScenes()
        {
            sceneOrder.Clear();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!includeDisabledBuildScenes && !scene.enabled) continue;
                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                if (asset != null) sceneOrder.Add(asset);
            }
        }

        private void ApplyBuildScenes()
        {
            var current = EditorBuildSettings.scenes.ToDictionary(s => s.path, s => s.enabled);
            var result = new List<EditorBuildSettingsScene>();
            foreach (var asset in sceneOrder)
            {
                if (asset == null) continue;
                string path = AssetDatabase.GetAssetPath(asset);
                bool enabled = current.TryGetValue(path, out bool wasEnabled) ? wasEnabled : true;
                result.Add(new EditorBuildSettingsScene(path, enabled));
            }
            if (includeDisabledBuildScenes)
            {
                foreach (var old in EditorBuildSettings.scenes)
                    if (!result.Any(s => s.path == old.path)) result.Add(old);
            }
            EditorBuildSettings.scenes = result.ToArray();
            Debug.Log($"[Chaos Card] Đã cập nhật thứ tự {result.Count} scene trong Build Settings.");
        }

        private void CreateOrSelectController()
        {
            if (controller != null) { Selection.activeGameObject = controller.gameObject; return; }
            var existing = FindFirstObjectByType<SceneButtonLinker>();
            if (existing != null) { controller = existing; Selection.activeGameObject = existing.gameObject; return; }
            var go = new GameObject("SceneButtonLinker");
            Undo.RegisterCreatedObjectUndo(go, "Create Scene Button Linker");
            controller = go.AddComponent<SceneButtonLinker>();
            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(go.scene);
        }

        private void Swap(int a, int b)
        {
            var temp = sceneOrder[a]; sceneOrder[a] = sceneOrder[b]; sceneOrder[b] = temp;
        }
    }
}
