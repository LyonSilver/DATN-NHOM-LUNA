#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SaveUIAutoLayoutTool : EditorWindow
{
    [MenuItem("Chaos Card/Auto Layout Save System UI")]
    public static void ShowWindow()
    {
        GetWindow<SaveUIAutoLayoutTool>("Save UI Layout Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("CHAOS CARD - SAVE UI AUTO LAYOUT", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Tool này sẽ tự động căn chỉnh Background, Title và xếp 3 Slot Panel (AutoSave, ManualSave, EmptySlot) thành 3 cột đẹp mắt.\n\nTất cả Event, Script và Data nối sẵn sẽ ĐƯỢC GIỮ NGUYÊN HOÀN TOÀN!", MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("🚀 TỰ ĐỘNG CĂN CHỈNH GIAO DIỆN (APPLY LAYOUT)", GUILayout.Height(40)))
        {
            ApplyAutoLayout();
        }
    }

    private static void ApplyAutoLayout()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Canvas trong Scene!", "OK");
            return;
        }

        // 1. Cấu hình Canvas Scaler
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            Undo.RecordObject(scaler, "Setup Canvas Scaler");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        // 2. Căn Background Fullscreen
        Transform bgTransform = canvas.transform.Find("Background");
        if (bgTransform != null && bgTransform.TryGetComponent<RectTransform>(out var bgRect))
        {
            Undo.RecordObject(bgRect, "Setup Background");
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
        }

        // 3. Căn Title lên Top-Center
        Transform titleTransform = canvas.transform.Find("Title");
        if (titleTransform != null && titleTransform.TryGetComponent<RectTransform>(out var titleRect))
        {
            Undo.RecordObject(titleRect, "Setup Title");
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -60);
            titleRect.sizeDelta = new Vector2(800, 100);

            if (titleTransform.TryGetComponent<TMP_Text>(out var tmp))
            {
                Undo.RecordObject(tmp, "Setup Title Text");
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 54;
            }
        }

        // 4. Tạo hoặc tìm SlotsContainer
        Transform containerTransform = canvas.transform.Find("SlotsContainer");
        GameObject containerObj;
        if (containerTransform == null)
        {
            containerObj = new GameObject("SlotsContainer", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(containerObj, "Create SlotsContainer");
            containerObj.transform.SetParent(canvas.transform, false);
        }
        else
        {
            containerObj = containerTransform.gameObject;
        }

        RectTransform containerRect = containerObj.GetComponent<RectTransform>();
        Undo.RecordObject(containerRect, "Setup SlotsContainer Rect");
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0, -30);
        containerRect.sizeDelta = new Vector2(1500, 650);

        // Thêm/cài đặt Horizontal Layout Group cho SlotsContainer
        HorizontalLayoutGroup group = containerObj.GetComponent<HorizontalLayoutGroup>();
        if (group == null) group = Undo.AddComponent<HorizontalLayoutGroup>(containerObj);
        
        Undo.RecordObject(group, "Setup Horizontal Group");
        group.padding = new RectOffset(30, 30, 20, 20);
        group.spacing = 40;
        group.childAlignment = TextAnchor.MiddleCenter;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = true;
        group.childForceExpandHeight = true;

        // 5. Đưa 3 Panel vào Container
        string[] panelNames = { "AutoSavePanel", "ManualSavePanel", "EmptySlotPanel" };
        foreach (string panelName in panelNames)
        {
            Transform panel = canvas.transform.Find(panelName);
            if (panel != null)
            {
                Undo.SetTransformParent(panel, containerObj.transform, "Move Panel to Container");
                
                // Thêm nền tối nhẹ cho Panel nếu có Image
                if (panel.TryGetComponent<Image>(out var img))
                {
                    Undo.RecordObject(img, "Setup Panel Image");
                    img.color = new Color(0.07f, 0.06f, 0.15f, 0.85f); // Tím xám tối Dark Fantasy
                }

                // Căn chỉnh con bên trong Panel
                VerticalLayoutGroup vGroup = panel.GetComponent<VerticalLayoutGroup>();
                if (vGroup == null) vGroup = Undo.AddComponent<VerticalLayoutGroup>(panel.gameObject);
                
                Undo.RecordObject(vGroup, "Setup Vertical Group");
                vGroup.padding = new RectOffset(20, 20, 30, 30);
                vGroup.spacing = 20;
                vGroup.childAlignment = TextAnchor.UpperCenter;
                vGroup.childControlWidth = true;
                vGroup.childControlHeight = false;
                vGroup.childForceExpandWidth = true;
                vGroup.childForceExpandHeight = false;
            }
        }

        // Căn BackButton
        Transform backBtn = canvas.transform.Find("BackButton");
        if (backBtn != null && backBtn.TryGetComponent<RectTransform>(out var backRect))
        {
            Undo.RecordObject(backRect, "Setup BackButton");
            backRect.anchorMin = new Vector2(0f, 0f);
            backRect.anchorMax = new Vector2(0f, 0f);
            backRect.pivot = new Vector2(0f, 0f);
            backRect.anchoredPosition = new Vector2(50, 40);
            backRect.sizeDelta = new Vector2(160, 50);
        }

        // Đánh dấu Scene đã thay đổi để lưu giữ kết quả lâu dài
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);

        EditorUtility.DisplayDialog("Thành công!", "Đã căn chỉnh xong giao diện Save System!\nHãy lưu Scene (Ctrl + S) để lưu thay đổi vĩnh viễn.", "OK");
    }
}
#endif