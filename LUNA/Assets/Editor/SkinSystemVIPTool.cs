using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class SkinSystemVIPTool : EditorWindow
{
    [MenuItem("Tools/VIP Skin System/Tạo Toàn Bộ")]
    public static void Open() => GetWindow<SkinSystemVIPTool>("VIP Skin Setup");

    void OnGUI()
    {
        GUILayout.Label("TẠO HỆ THỐNG SKIN VIP (2.5D)", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("TẠO TOÀN BỘ HỆ THỐNG", GUILayout.Height(40)))
            CreateFullSystem();

        GUILayout.Space(8);
        EditorGUILayout.HelpBox("Tự tạo: Canvas + Preview Ortho 2.5D + Scroll + Manager + Prefab.\nBạn chỉ cần kéo model + icon + aura vào SkinData.", MessageType.Info);
    }

    static void CreateFullSystem()
    {
        string root = "Assets/SkinSystemVIP";
        Directory.CreateDirectory(root + "/Scripts");
        Directory.CreateDirectory(root + "/Prefabs");
        Directory.CreateDirectory(root + "/Data");
        Directory.CreateDirectory(root + "/UI");

        // RenderTexture - tạo asset trước, load lại rồi mới gán camera
        string rtPath = root + "/UI/SkinPreviewRT.renderTexture";
        var rt = new RenderTexture(512, 512, 16);
        rt.name = "SkinPreviewRT";
        AssetDatabase.CreateAsset(rt, rtPath);
        AssetDatabase.SaveAssets();
        rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);

        // === Preview 2.5D ===
        var previewRoot = new GameObject("=== SKIN PREVIEW 2.5D ===");
        var camGO = new GameObject("PreviewCamera");
        camGO.transform.SetParent(previewRoot.transform);
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f, 1f);
        cam.orthographic = true;
        cam.orthographicSize = 2.2f;
        cam.transform.position = new Vector3(0, 1f, -10f);
        cam.transform.rotation = Quaternion.identity;
        cam.targetTexture = rt;

        var anchor = new GameObject("PreviewAnchor");
        anchor.transform.SetParent(previewRoot.transform);
        anchor.transform.position = Vector3.up * 1f;

        var previewCtrl = previewRoot.AddComponent<SkinPreviewController>();
        previewCtrl.previewAnchor = anchor.transform;
        previewCtrl.previewCamera = cam;
        previewCtrl.is25D = true;

        // === Canvas ===
        var canvasGO = new GameObject("=== SKIN UI VIP ===");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        var panel = CreateUI("Panel", canvasGO.transform);
        panel.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        SetRect(panel, 0, 0, 1, 1);

        var rawGO = CreateUI("PreviewImage", panel.transform);
        var raw = rawGO.AddComponent<RawImage>();
        raw.texture = rt;
        SetRect(rawGO, 0.02f, 0.15f, 0.45f, 0.95f);
        previewCtrl.previewImage = raw;

        var scroll = CreateScrollView(panel.transform);
        SetRect(scroll, 0.48f, 0.15f, 0.98f, 0.85f);

        var nameText = CreateText("SkinName", panel.transform, "Chọn Skin", 28);
        SetRect(nameText.gameObject, 0.48f, 0.88f, 0.98f, 0.96f);

        var rarityText = CreateText("Rarity", panel.transform, "RARITY", 20);
        SetRect(rarityText.gameObject, 0.48f, 0.82f, 0.7f, 0.88f);

        var equipBtn = CreateButton("EquipButton", panel.transform, "EQUIP");
        SetRect(equipBtn, 0.72f, 0.82f, 0.98f, 0.88f);

        var btnPrefab = CreateSkinButtonPrefab(root);

        // === Manager ===
        var managerGO = new GameObject("=== SKIN MANAGER ===");
        var manager = managerGO.AddComponent<SkinManager>();
        if (manager.allSkins == null)
            manager.allSkins = new System.Collections.Generic.List<SkinData>();

        managerGO.AddComponent<PlayerSkinController>();

        var ui = panel.AddComponent<SkinSelectionUI>();
        ui.skinManager = manager;
        ui.preview = previewCtrl;
        ui.contentParent = scroll.GetComponent<ScrollRect>().content;
        ui.skinButtonPrefab = btnPrefab;
        ui.equipButton = equipBtn.GetComponent<Button>();
        ui.skinNameText = nameText;
        ui.rarityText = rarityText;

        // Sample
        var sample = ScriptableObject.CreateInstance<SkinData>();
        sample.skinId = "skin_default";
        sample.displayName = "Default Skin";
        sample.rarity = SkinRarity.Common;
        AssetDatabase.CreateAsset(sample, root + "/Data/Skin_Default.asset");
        manager.allSkins.Add(sample);

        AssetDatabase.SaveAssets();
        Selection.activeGameObject = canvasGO;

        EditorUtility.DisplayDialog("Xong",
            "Đã tạo xong hệ thống Skin VIP 2.5D!\n\n" +
            "• Preview Ortho\n" +
            "• Chỉ cần kéo model vào visualPrefab\n" +
            "• Icon + Aura vào SkinData\n" +
            "• Gắn visualParent cho Player", "OK");
    }

    static GameObject CreateUI(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static void SetRect(GameObject go, float xMin, float yMin, float xMax, float yMax)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static TextMeshProUGUI CreateText(string name, Transform parent, string text, int size)
    {
        var go = CreateUI(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    static GameObject CreateButton(string name, Transform parent, string label)
    {
        var go = CreateUI(name, parent);
        go.AddComponent<Image>().color = new Color(0.2f, 0.5f, 1f);
        go.AddComponent<Button>();
        var txt = CreateText("Text", go.transform, label, 18);
        SetRect(txt.gameObject, 0, 0, 1, 1);
        return go;
    }

    static GameObject CreateScrollView(Transform parent)
    {
        var scrollGO = CreateUI("ScrollView", parent);
        var scroll = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;

        var viewport = CreateUI("Viewport", scrollGO.transform);
        viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0.3f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        SetRect(viewport, 0, 0, 1, 1);

        var content = CreateUI("Content", viewport.transform);
        var layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);

        scroll.content = contentRT;
        scroll.viewport = viewport.GetComponent<RectTransform>();
        return scrollGO;
    }

    static GameObject CreateSkinButtonPrefab(string root)
    {
        var prefabGO = new GameObject("SkinButtonPrefab");
        prefabGO.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 80);
        prefabGO.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f);
        prefabGO.AddComponent<Button>();

        var border = CreateUI("RarityBorder", prefabGO.transform);
        border.AddComponent<Image>().color = Color.cyan;
        SetRect(border, 0, 0, 0.08f, 1);

        var icon = CreateUI("Icon", prefabGO.transform);
        icon.AddComponent<Image>();
        SetRect(icon, 0.1f, 0.1f, 0.35f, 0.9f);

        var name = CreateText("Name", prefabGO.transform, "Skin Name", 16);
        SetRect(name.gameObject, 0.38f, 0.3f, 0.85f, 0.8f);
        name.alignment = TextAlignmentOptions.Left;

        var lockIcon = CreateUI("Lock", prefabGO.transform);
        lockIcon.AddComponent<Image>().color = Color.red;
        SetRect(lockIcon, 0.88f, 0.3f, 0.98f, 0.7f);

        var vip = CreateUI("VIP", prefabGO.transform);
        vip.AddComponent<Image>().color = new Color(1f, 0.8f, 0.2f);
        SetRect(vip, 0.88f, 0.72f, 0.98f, 0.95f);

        var highlight = CreateUI("Highlight", prefabGO.transform);
        highlight.AddComponent<Image>().color = new Color(0.3f, 0.7f, 1f, 0.4f);
        SetRect(highlight, 0, 0, 1, 1);
        highlight.SetActive(false);

        var skinBtn = prefabGO.AddComponent<SkinButton>();
        skinBtn.icon = icon.GetComponent<Image>();
        skinBtn.rarityBorder = border.GetComponent<Image>();
        skinBtn.nameText = name;
        skinBtn.lockIcon = lockIcon;
        skinBtn.vipBadge = vip;
        skinBtn.selectedHighlight = highlight;

        var prefab = PrefabUtility.SaveAsPrefabAsset(prefabGO, root + "/Prefabs/SkinButton.prefab");
        DestroyImmediate(prefabGO);
        return prefab;
    }
}