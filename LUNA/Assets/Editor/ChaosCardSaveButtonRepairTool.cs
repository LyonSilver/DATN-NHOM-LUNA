
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChaosCardSaveButtonRepairTool : EditorWindow
{
    [MenuItem("Chaos Card/Save Button Repair Tool")]
    public static void Open()
    {
        GetWindow<ChaosCardSaveButtonRepairTool>(
            "Save Button Repair"
        );
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField(
            "Chaos Card - Save Button Repair",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            "Tự nối các nút SAVE NEW, SAVE, LOAD, EXIT " +
            "trong scene đang mở. Không dựng lại UI và không " +
            "xóa listener hiện có.",
            MessageType.Info
        );

        if (GUILayout.Button("SCAN CURRENT SCENE", GUILayout.Height(32)))
            Scan();

        if (GUILayout.Button("FIX BUTTONS + SAVE SCENE", GUILayout.Height(40)))
            Fix();
    }

    private void Scan()
    {
        Scene scene = SceneManager.GetActiveScene();

        foreach (Button button in FindButtons(scene))
        {
            Debug.Log(
                $"Button: {GetPath(button.transform)} | " +
                $"Persistent listeners: " +
                $"{button.onClick.GetPersistentEventCount()}",
                button
            );
        }

        Debug.Log("Đã quét các Button trong scene hiện tại.");
    }

    private void Fix()
    {
        Scene scene = SceneManager.GetActiveScene();

        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("Scene hiện tại không hợp lệ.");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        SaveSlotUI slotUI = FindComponent<SaveSlotUI>(scene);

        if (slotUI == null)
        {
            Debug.LogError(
                "Không tìm thấy SaveSlotUI. " +
                "Tool không tự tạo UI mới."
            );
            return;
        }

        SaveSlotButtonActions actions =
            slotUI.GetComponent<SaveSlotButtonActions>();

        if (actions == null)
        {
            Undo.AddComponent<SaveSlotButtonActions>(
                slotUI.gameObject
            );

            actions = slotUI.GetComponent<SaveSlotButtonActions>();
        }

        SaveSystemTester tester =
            FindComponent<SaveSystemTester>(scene);

        Wire(FindButton(scene, "SaveNewButton", "SAVE NEW"),
            actions, actions.SaveNewSlot);

        Button save = FindButton(scene, "SaveButton", "SAVE");

        if (save != null)
        {
            if (tester != null)
                Wire(save, tester, tester.TestSave);
            else
                Wire(save, actions, actions.SaveManualSlot);
        }

        Wire(FindButton(scene, "LoadButton", "LOAD"),
            slotUI, slotUI.RefreshUI);

        Wire(FindButton(scene, "BackButton", "EXIT"),
            actions, actions.ExitToMenu);

        EditorUtility.SetDirty(actions);
        EditorSceneManager.MarkSceneDirty(scene);

        bool saved = EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log(
            saved
                ? "Đã nối các Button còn thiếu và lưu scene."
                : "Đã xử lý Button nhưng Unity chưa xác nhận lưu scene."
        );
    }

    private void Wire(
        Button button,
        SaveSlotButtonActions target,
        UnityEngine.Events.UnityAction callback)
    {
        WireInternal(button, target, callback);
    }

    private void Wire(
        Button button,
        SaveSystemTester target,
        UnityEngine.Events.UnityAction callback)
    {
        WireInternal(button, target, callback);
    }

    private void Wire(
        Button button,
        SaveSlotUI target,
        UnityEngine.Events.UnityAction callback)
    {
        WireInternal(button, target, callback);
    }

    private void WireInternal(
        Button button,
        Object target,
        UnityEngine.Events.UnityAction callback)
    {
        if (button == null || target == null)
            return;

        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentTarget(i) == target &&
                button.onClick.GetPersistentMethodName(i) ==
                callback.Method.Name)
            {
                Debug.Log(
                    $"Đã có listener, bỏ qua: {button.name}",
                    button
                );
                return;
            }
        }

        Undo.RecordObject(button, "Repair Save Button");

        UnityEventTools.AddPersistentListener(
            button.onClick, callback
        );

        EditorUtility.SetDirty(button);
        PrefabUtility.RecordPrefabInstancePropertyModifications(button);
    }

    private Button[] FindButtons(Scene scene)
    {
        var results = new System.Collections.Generic.List<Button>();

        foreach (GameObject root in scene.GetRootGameObjects())
            results.AddRange(root.GetComponentsInChildren<Button>(true));

        return results.ToArray();
    }

    private Button FindButton(
        Scene scene, string objectName, string label)
    {
        Button[] buttons = FindButtons(scene);

        foreach (Button button in buttons)
        {
            if (button.name == objectName)
                return button;
        }

        foreach (Button button in buttons)
        {
            var text = button.GetComponentInChildren<
                TMPro.TMP_Text>(true);

            if (text != null &&
                text.text.Trim().Equals(
                    label,
                    System.StringComparison.OrdinalIgnoreCase))
                return button;
        }

        return null;
    }

    private T FindComponent<T>(Scene scene)
        where T : Component
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            T component = root.GetComponentInChildren<T>(true);
            if (component != null)
                return component;
        }

        return null;
    }

    private string GetPath(Transform target)
    {
        string path = target.name;

        while (target.parent != null)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }

        return path;
    }
}
#endif
