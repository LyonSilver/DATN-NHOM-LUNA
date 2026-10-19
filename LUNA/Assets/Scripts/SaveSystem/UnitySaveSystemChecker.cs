#if UNITY_EDITOR
using System;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UnitySaveSystemChecker : EditorWindow
{
    private string report = "Bấm CHECK SCENE để kiểm tra hệ thống Save hiện tại.";
    private Vector2 scroll;

    [MenuItem("Chaos Card/Save System Checker")]
    public static void Open()
    {
        GetWindow<UnitySaveSystemChecker>("Save System Checker");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Công cụ chỉ kiểm tra và tạo báo cáo. Không tự sửa scene, không thêm/xóa component, không ghi đè script.",
            MessageType.Info);

        if (GUILayout.Button("CHECK SCENE", GUILayout.Height(34)))
            report = RunCheck();

        if (GUILayout.Button("COPY REPORT"))
            EditorGUIUtility.systemCopyBuffer = report;

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Báo cáo", EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.TextArea(report, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private static string RunCheck()
    {
        var sb = new StringBuilder();
        Scene scene = SceneManager.GetActiveScene();

        sb.AppendLine("=== CHAOS CARD | SAVE SYSTEM CHECK ===");
        sb.AppendLine("Scene: " + scene.name);
        sb.AppendLine("Scene path: " + scene.path);
        sb.AppendLine("Checked at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sb.AppendLine();

        var allBehaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>()
            .Where(x => x != null && x.gameObject.scene == scene && x.gameObject.scene.IsValid())
            .ToArray();

        string[] importantTypes =
        {
            "SaveLoadManager", "AutoSaveManager", "SaveSlotUI", "SaveSystemTester"
        };

        sb.AppendLine("--- 1. SAVE COMPONENTS ---");
        foreach (string typeName in importantTypes)
        {
            var matches = allBehaviours.Where(x => x.GetType().Name == typeName).ToArray();
            sb.AppendLine(typeName + ": " + matches.Length + " found");
            foreach (var component in matches)
            {
                sb.AppendLine("  Object: " + GetHierarchyPath(component.transform)
                    + " | Enabled: " + component.enabled
                    + " | Active: " + component.gameObject.activeInHierarchy);
                AppendSerializedObjectFields(sb, component);
            }
        }

        sb.AppendLine();
        sb.AppendLine("--- 2. BUTTONS AND ON CLICK EVENTS ---");
        var buttons = Resources.FindObjectsOfTypeAll<Button>()
            .Where(x => x != null && x.gameObject.scene == scene && x.gameObject.scene.IsValid())
            .ToArray();

        sb.AppendLine("Buttons found: " + buttons.Length);
        foreach (var button in buttons)
        {
            string label = GetButtonLabel(button);
            sb.AppendLine("Button: " + GetHierarchyPath(button.transform)
                + (string.IsNullOrEmpty(label) ? "" : " | Label: \"" + label + "\"")
                + " | Interactable: " + button.interactable
                + " | Active: " + button.gameObject.activeInHierarchy);

            int count = button.onClick.GetPersistentEventCount();
            sb.AppendLine("  Persistent OnClick events: " + count);
            if (count == 0)
            {
                sb.AppendLine("  [WARNING] Chưa có OnClick event được gán trong Inspector.");
                continue;
            }

            for (int i = 0; i < count; i++)
            {
                UnityEngine.Object target = button.onClick.GetPersistentTarget(i);
                string method = button.onClick.GetPersistentMethodName(i);
                UnityEventCallState state = button.onClick.GetPersistentListenerState(i);
                sb.AppendLine("  [" + i + "] Target: "
                    + (target == null ? "NULL" : target.name + " (" + target.GetType().Name + ")")
                    + " | Method: " + (string.IsNullOrEmpty(method) ? "NOT SET" : method)
                    + " | State: " + state);

                if (target == null || string.IsNullOrEmpty(method))
                    sb.AppendLine("      [WARNING] Event target hoặc method đang thiếu.");
            }
        }

        sb.AppendLine();
        sb.AppendLine("--- 3. TEXT / TMP COMPONENTS ---");
        var texts = Resources.FindObjectsOfTypeAll<TMP_Text>()
            .Where(x => x != null && x.gameObject.scene == scene && x.gameObject.scene.IsValid())
            .ToArray();
        sb.AppendLine("TMP_Text found: " + texts.Length);
        foreach (var t in texts)
            sb.AppendLine("  " + GetHierarchyPath(t.transform) + " | Text: \"" + t.text + "\"");

        sb.AppendLine();
        sb.AppendLine("--- 4. QUICK DIAGNOSIS ---");
        int managerCount = allBehaviours.Count(x => x.GetType().Name == "SaveLoadManager");
        int testerCount = allBehaviours.Count(x => x.GetType().Name == "SaveSystemTester");
        int autoCount = allBehaviours.Count(x => x.GetType().Name == "AutoSaveManager");
        int slotUICount = allBehaviours.Count(x => x.GetType().Name == "SaveSlotUI");

        if (managerCount == 0)
            sb.AppendLine("[WARNING] Không tìm thấy SaveLoadManager trong scene đang mở. Có thể nó được tạo ở scene khác hoặc bằng code lúc chạy.");
        else if (managerCount > 1)
            sb.AppendLine("[WARNING] Có nhiều hơn một SaveLoadManager trong scene. Kiểm tra Singleton để tránh trùng.");
        else
            sb.AppendLine("[OK] Tìm thấy 1 SaveLoadManager trong scene.");

        if (testerCount == 0)
            sb.AppendLine("[WARNING] Không tìm thấy SaveSystemTester trong scene; nút TestSave sẽ không thể gọi component này nếu chưa gán từ nơi khác.");
        if (autoCount == 0)
            sb.AppendLine("[WARNING] Không tìm thấy AutoSaveManager trong scene.");
        if (slotUICount == 0)
            sb.AppendLine("[WARNING] Không tìm thấy SaveSlotUI trong scene.");

        if (buttons.Length == 0)
            sb.AppendLine("[WARNING] Không tìm thấy UI Button trong scene đang mở.");

        sb.AppendLine();
        sb.AppendLine("LƯU Ý: Báo cáo kiểm tra những object trong scene đang mở, không tự xác nhận file JSON đã được ghi.");
        sb.AppendLine("Muốn kiểm tra kết quả runtime, vào Play Mode, bấm nút Save rồi gửi thêm log Console nếu cần.");
        return sb.ToString();
    }

    private static void AppendSerializedObjectFields(StringBuilder sb, MonoBehaviour component)
    {
        // Report public/serialized references without invoking methods or changing values.
        var so = new SerializedObject(component);
        SerializedProperty p = so.GetIterator();
        bool enterChildren = true;
        while (p.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (p.propertyPath == "m_Script")
                continue;

            if (p.propertyType == SerializedPropertyType.ObjectReference)
            {
                UnityEngine.Object value = p.objectReferenceValue;
                sb.AppendLine("    Ref " + p.propertyPath + ": "
                    + (value == null ? "NULL" : value.name + " (" + value.GetType().Name + ")"));
            }
            else if (p.propertyPath == "enableAutoSave" ||
                     p.propertyPath == "autoSaveInterval" ||
                     p.propertyPath == "chapter" ||
                     p.propertyPath == "playerLevel" ||
                     p.propertyPath == "gold")
            {
                string value;
                switch (p.propertyType)
                {
                    case SerializedPropertyType.Integer: value = p.intValue.ToString(); break;
                    case SerializedPropertyType.Float: value = p.floatValue.ToString(); break;
                    case SerializedPropertyType.Boolean: value = p.boolValue.ToString(); break;
                    case SerializedPropertyType.String: value = p.stringValue; break;
                    default: value = "(type " + p.propertyType + ")"; break;
                }
                sb.AppendLine("    " + p.propertyPath + ": " + value);
            }
        }
    }

    private static string GetButtonLabel(Button button)
    {
        var text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null) return text.text;
        var legacy = button.GetComponentInChildren<Text>(true);
        return legacy != null ? legacy.text : "";
    }

    private static string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
#endif
