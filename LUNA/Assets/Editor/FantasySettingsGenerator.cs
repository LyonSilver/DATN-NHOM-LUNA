using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FantasySettingsGenerator : EditorWindow
{
    const string Root = "Assets/FantasySettings";
    Sprite backgroundSprite, panelSprite, buttonSprite;
    readonly Color gold = new Color(0.93f, 0.76f, 0.42f);
    readonly Color parchment = new Color(0.97f, 0.89f, 0.72f);
    readonly Color dark = new Color(0.12f, 0.16f, 0.15f);

    [MenuItem("Tools/Fantasy Settings/Scene Generator")]
    public static void ShowWindow() => GetWindow<FantasySettingsGenerator>("Fantasy Settings");

    void OnGUI()
    {
        GUILayout.Label("Fantasy 2D Settings Scene", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Creates a new SettingsScene. Save your current scene first. Existing SettingsScene will not be overwritten.", MessageType.Info);
        backgroundSprite = (Sprite)EditorGUILayout.ObjectField("Background Sprite", backgroundSprite, typeof(Sprite), false);
        panelSprite = (Sprite)EditorGUILayout.ObjectField("Panel Sprite", panelSprite, typeof(Sprite), false);
        buttonSprite = (Sprite)EditorGUILayout.ObjectField("Button Sprite", buttonSprite, typeof(Sprite), false);
        GUILayout.Space(12);
        if (GUILayout.Button("GENERATE SETTINGS SCENE", GUILayout.Height(44))) Generate();
    }

    void Generate()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        string path = Root + "/Scenes/SettingsScene.unity";
        if (File.Exists(path))
        {
            EditorUtility.DisplayDialog("Scene exists", "SettingsScene already exists. Rename or move it before generating a new scene.", "OK");
            return;
        }
        Directory.CreateDirectory(Root + "/Scenes");
        AssetDatabase.Refresh();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject canvasGO = new GameObject("Fantasy Settings Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        RectTransform root = canvasGO.GetComponent<RectTransform>();
        root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
        root.offsetMin = root.offsetMax = Vector2.zero;

        Image bg = ImageObject("Background", root, backgroundSprite, dark);
        Stretch(bg.rectTransform);
        Image overlay = ImageObject("Dark Overlay", root, null, new Color(0, 0, 0, 0.25f));
        Stretch(overlay.rectTransform);

        Image panel = ImageObject("Settings Panel", root, panelSprite, new Color(0.17f, 0.20f, 0.17f, 0.97f));
        SetRect(panel.rectTransform, new Vector2(980, 890), Vector2.zero);
        if (panelSprite) panel.type = Image.Type.Sliced;
        panel.gameObject.AddComponent<Outline>().effectColor = gold;
        Text("SETTINGS", panel.rectTransform, 52, new Vector2(840, 90), new Vector2(0, 365), gold);
        Text("A D V E N T U R E   O P T I O N S", panel.rectTransform, 19, new Vector2(800, 40), new Vector2(0, 310), parchment);
        var manager = new GameObject("Settings Manager").AddComponent<FantasySettingsManager>();
        float y = 242;
        manager.masterSlider = MakeSlider(panel.rectTransform, "Master Volume", y, out manager.masterValue); y -= 88;
        manager.musicSlider = MakeSlider(panel.rectTransform, "Music Volume", y, out manager.musicValue); y -= 88;
        manager.sfxSlider = MakeSlider(panel.rectTransform, "SFX Volume", y, out manager.sfxValue); y -= 92;
        manager.resolutionDropdown = MakeDropdown(panel.rectTransform, "Resolution", y); y -= 84;
        manager.qualityDropdown = MakeDropdown(panel.rectTransform, "Graphics Quality", y); y -= 84;
        manager.fullscreenToggle = MakeToggle(panel.rectTransform, "Fullscreen", y);
        manager.saveButton = MakeButton(panel.rectTransform, "SAVE", new Vector2(-270, -350));
        manager.resetButton = MakeButton(panel.rectTransform, "RESET", new Vector2(0, -350));
        manager.backButton = MakeButton(panel.rectTransform, "BACK", new Vector2(270, -350));

        if (!FindObjectOfType<EventSystem>()) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.Refresh();
        Selection.activeGameObject = canvasGO;
        EditorUtility.DisplayDialog("Scene generated", "Created " + path + "\nAdd this scene and MainMenu to Build Settings / Build Profiles.", "OK");
    }

    static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }
    static void SetRect(RectTransform r, Vector2 size, Vector2 position)
    {
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = size; r.anchoredPosition = position;
    }
    static Image ImageObject(string name, Transform parent, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>(); image.sprite = sprite; image.color = color;
        return image;
    }
    static TMP_Text Text(string content, Transform parent, float fontSize, Vector2 size, Vector2 position, Color color)
    {
        var go = new GameObject(content, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), size, position);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = content; t.fontSize = fontSize; t.color = color;
        t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false;
        return t;
    }
    Slider MakeSlider(Transform parent, string title, float y, out TMP_Text valueText)
    {
        Text(title, parent, 27, new Vector2(330, 50), new Vector2(-265, y), parchment);
        Image track = ImageObject(title + " Track", parent, null, new Color(0.07f, 0.09f, 0.08f));
        SetRect(track.rectTransform, new Vector2(345, 18), new Vector2(110, y));
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(track.transform, false); Stretch(fillArea.GetComponent<RectTransform>());
        Image fill = ImageObject("Fill", fillArea.transform, null, gold); Stretch(fill.rectTransform);
        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(track.transform, false); Stretch(handleArea.GetComponent<RectTransform>());
        Image handle = ImageObject("Handle", handleArea.transform, null, parchment);
        SetRect(handle.rectTransform, new Vector2(26, 35), Vector2.zero);
        Slider slider = track.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform; slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle; slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0; slider.maxValue = 1; slider.value = 1;
        valueText = Text("100%", parent, 23, new Vector2(95, 50), new Vector2(350, y), gold);
        return slider;
    }
    TMP_Dropdown MakeDropdown(Transform parent, string title, float y)
    {
        Text(title, parent, 27, new Vector2(330, 50), new Vector2(-265, y), parchment);
        Image box = ImageObject(title + " Dropdown", parent, null, new Color(0.25f, 0.29f, 0.22f));
        SetRect(box.rectTransform, new Vector2(400, 52), new Vector2(160, y));
        var dropdown = box.gameObject.AddComponent<TMP_Dropdown>();
        TMP_Text label = Text("Select", box.transform, 23, new Vector2(340, 45), Vector2.zero, parchment);
        dropdown.captionText = label;
        Image template = ImageObject("Template", box.transform, null, new Color(0.19f, 0.22f, 0.19f));
        RectTransform tr = template.rectTransform;
        tr.anchorMin = new Vector2(0, 1); tr.anchorMax = new Vector2(1, 1);
        tr.pivot = new Vector2(0.5f, 1); tr.anchoredPosition = new Vector2(0, -52);
        tr.sizeDelta = new Vector2(0, 190);
        template.gameObject.AddComponent<ScrollRect>();
        Image viewport = ImageObject("Viewport", template.transform, null, new Color(1, 1, 1, 0.01f));
        Stretch(viewport.rectTransform);
        viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        RectTransform cr = content.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0, 1); cr.anchorMax = new Vector2(1, 1);
        cr.pivot = new Vector2(0.5f, 1); cr.sizeDelta = new Vector2(0, 40); cr.anchoredPosition = Vector2.zero;
        Image item = ImageObject("Item", content.transform, null, new Color(0.2f, 0.25f, 0.2f));
        RectTransform ir = item.rectTransform;
        ir.anchorMin = new Vector2(0, 0.5f); ir.anchorMax = new Vector2(1, 0.5f);
        ir.sizeDelta = new Vector2(0, 40); ir.anchoredPosition = Vector2.zero;
        Toggle toggle = item.gameObject.AddComponent<Toggle>();
        Image check = ImageObject("Item Checkmark", item.transform, null, gold);
        SetRect(check.rectTransform, new Vector2(16, 16), new Vector2(-175, 0));
        toggle.graphic = check;
        TMP_Text itemLabel = Text("Option", item.transform, 20, new Vector2(315, 38), new Vector2(20, 0), parchment);
        ScrollRect scroll = template.GetComponent<ScrollRect>();
        scroll.content = cr; scroll.viewport = viewport.rectTransform;
        scroll.horizontal = false; scroll.vertical = true;
        dropdown.template = tr; dropdown.itemText = itemLabel;
        template.gameObject.SetActive(false);
        return dropdown;
    }
    Toggle MakeToggle(Transform parent, string title, float y)
    {
        Text(title, parent, 27, new Vector2(330, 50), new Vector2(-265, y), parchment);
        Image bg = ImageObject("Fullscreen Toggle", parent, null, new Color(0.18f, 0.23f, 0.18f));
        SetRect(bg.rectTransform, new Vector2(45, 45), new Vector2(-10, y));
        Toggle toggle = bg.gameObject.AddComponent<Toggle>();
        Image mark = ImageObject("Checkmark", bg.transform, null, gold);
        SetRect(mark.rectTransform, new Vector2(27, 27), Vector2.zero);
        toggle.graphic = mark; toggle.targetGraphic = bg;
        return toggle;
    }
    Button MakeButton(Transform parent, string title, Vector2 position)
    {
        Image img = ImageObject(title + " Button", parent, buttonSprite, new Color(0.36f, 0.27f, 0.14f));
        SetRect(img.rectTransform, new Vector2(220, 68), position);
        if (buttonSprite) img.type = Image.Type.Sliced;
        Button b = img.gameObject.AddComponent<Button>();
        var colors = b.colors;
        colors.normalColor = Color.white; colors.highlightedColor = new Color(1f, 0.92f, 0.7f);
        colors.pressedColor = new Color(0.7f, 0.65f, 0.5f); b.colors = colors;
        Text(title, img.transform, 27, new Vector2(200, 60), Vector2.zero, parchment);
        return b;
    }
}
