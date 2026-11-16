using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FantasySettingsPROGenerator : EditorWindow
{
    const string Root = "Assets/FantasySettingsPRO";
    Sprite background, mainPanel, categoryPanel, contentPanel;
    Sprite save, apply, reset, graphics, audio, music, fullscreen, back, close;
    Sprite sliderTrack, sliderFill, sliderHandle, dropdownSprite, toggleSprite;
    Color parchment = new Color(0.97f, 0.87f, 0.68f);
    Vector2 scroll;
    [MenuItem("Tools/Fantasy Settings PRO/Generator")]
    public static void Open() { GetWindow<FantasySettingsPROGenerator>("Fantasy Settings PRO"); }
    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.LabelField("FANTASY SETTINGS PRO", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Assign individual Sprites. All nine button graphics can be assigned separately. Existing scenes are never overwritten.", MessageType.Info);
        background = Slot("Background", background); mainPanel = Slot("Main Panel", mainPanel);
        categoryPanel = Slot("Category Panel", categoryPanel); contentPanel = Slot("Content Panel", contentPanel);
        EditorGUILayout.Space(); EditorGUILayout.LabelField("NINE BUTTONS", EditorStyles.boldLabel);
        save = Slot("Save", save); apply = Slot("Apply", apply); reset = Slot("Reset", reset);
        graphics = Slot("Graphics", graphics); audio = Slot("Audio", audio); music = Slot("Music", music);
        fullscreen = Slot("Fullscreen", fullscreen); back = Slot("Back", back); close = Slot("Close", close);
        EditorGUILayout.Space(); EditorGUILayout.LabelField("OPTIONAL CONTROLS", EditorStyles.boldLabel);
        sliderTrack = Slot("Slider Track", sliderTrack); sliderFill = Slot("Slider Fill", sliderFill);
        sliderHandle = Slot("Slider Handle", sliderHandle); dropdownSprite = Slot("Dropdown", dropdownSprite);
        toggleSprite = Slot("Toggle", toggleSprite);
        if (GUILayout.Button("AUTO ASSIGN INCLUDED BUTTONS", GUILayout.Height(30))) AutoAssign();
        if (GUILayout.Button("GENERATE COMPLETE SETTINGS SCENE", GUILayout.Height(46))) Generate();
        EditorGUILayout.EndScrollView();
    }
    static Sprite Slot(string title, Sprite current) => (Sprite)EditorGUILayout.ObjectField(title, current, typeof(Sprite), false);
    static Sprite Load(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/Buttons/" + name + ".png");
    void AutoAssign() { save = Load("Save"); apply = Load("Apply"); reset = Load("Reset"); graphics = Load("Graphics"); audio = Load("Audio"); music = Load("Music"); fullscreen = Load("Fullscreen"); back = Load("Back"); close = Load("Close"); }
    static void Rect(RectTransform r, Vector2 size, Vector2 pos) { r.anchorMin = r.anchorMax = Vector2.one * .5f; r.sizeDelta = size; r.anchoredPosition = pos; }
    static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
    Image Img(string name, Transform parent, Sprite sprite, Color fallback, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>(); image.sprite = sprite; image.color = sprite ? Color.white : fallback;
        Rect(image.rectTransform, size, pos); return image;
    }
    TMP_Text Txt(string title, Transform parent, Vector2 pos, Vector2 size, int font = 26)
    {
        var go = new GameObject(title, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)); go.transform.SetParent(parent, false);
        Rect(go.GetComponent<RectTransform>(), size, pos); var t = go.GetComponent<TextMeshProUGUI>(); t.text = title;
        t.fontSize = font; t.color = parchment; t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false; return t;
    }
    Button Btn(string name, Transform parent, Sprite sprite, Vector2 pos, Vector2 size, bool label)
    {
        var image = Img(name, parent, sprite, new Color(.25f,.18f,.4f), size, pos);
        var b = image.gameObject.AddComponent<Button>(); b.targetGraphic = image;
        var colors = b.colors; colors.highlightedColor = new Color(1f,.85f,.56f); colors.pressedColor = new Color(.75f,.75f,.8f); b.colors = colors;
        if (label) Txt(name, image.transform, Vector2.zero, new Vector2(size.x-25, 55), 25); return b;
    }
    Slider MakeSlider(Transform parent, string label, float y, out TMP_Text value)
    {
        Txt(label, parent, new Vector2(-205,y), new Vector2(260,45));
        var track = Img(label+" Slider", parent, sliderTrack, new Color(.22f,.16f,.28f), new Vector2(310,22), new Vector2(120,y));
        var fillArea = new GameObject("Fill Area", typeof(RectTransform)); fillArea.transform.SetParent(track.transform,false); Stretch(fillArea.GetComponent<RectTransform>());
        var fill = Img("Fill", fillArea.transform, sliderFill, new Color(.86f,.67f,.25f), new Vector2(310,22), Vector2.zero); Stretch(fill.rectTransform);
        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform)); handleArea.transform.SetParent(track.transform,false); Stretch(handleArea.GetComponent<RectTransform>());
        var handle = Img("Handle",handleArea.transform,sliderHandle,Color.white,new Vector2(28,40),Vector2.zero);
        var slider = track.gameObject.AddComponent<Slider>(); slider.fillRect=fill.rectTransform; slider.handleRect=handle.rectTransform; slider.targetGraphic=handle;
        slider.minValue=0;slider.maxValue=1;slider.value=.8f;
        value=Txt("80%",parent,new Vector2(345,y),new Vector2(90,40),21);return slider;
    }
    TMP_Dropdown Dropdown(Transform parent,string name,float y)
    {
        Txt(name,parent,new Vector2(-205,y),new Vector2(260,42));
        var bg=Img(name+" Dropdown",parent,dropdownSprite,new Color(.24f,.18f,.34f),new Vector2(360,50),new Vector2(145,y));
        var dd=bg.gameObject.AddComponent<TMP_Dropdown>();dd.targetGraphic=bg;
        dd.captionText=Txt("Select",bg.transform,Vector2.zero,new Vector2(310,40),21);
        var template=Img("Template",bg.transform,null,new Color(.19f,.14f,.28f),new Vector2(360,175),new Vector2(0,-112));
        var viewport=Img("Viewport",template.transform,null,Color.white,new Vector2(360,175),Vector2.zero);Stretch(viewport.rectTransform);
        viewport.gameObject.AddComponent<Mask>().showMaskGraphic=false;
        var content=new GameObject("Content",typeof(RectTransform));content.transform.SetParent(viewport.transform,false);
        var cr=content.GetComponent<RectTransform>();cr.anchorMin=new Vector2(0,1);cr.anchorMax=Vector2.one;cr.pivot=new Vector2(.5f,1);cr.sizeDelta=new Vector2(0,0);cr.anchoredPosition=Vector2.zero;
        var item=Img("Item",content.transform,null,new Color(.3f,.23f,.4f),new Vector2(360,42),new Vector2(0,-21));
        var toggle=item.gameObject.AddComponent<Toggle>();toggle.targetGraphic=item;
        var check=Img("Item Checkmark",item.transform,null,Color.yellow,new Vector2(15,15),new Vector2(-162,0));toggle.graphic=check;
        dd.itemText=Txt("Option",item.transform,new Vector2(15,0),new Vector2(290,38),20);
        var scrollRect=template.gameObject.AddComponent<ScrollRect>();scrollRect.viewport=viewport.rectTransform;scrollRect.content=cr;scrollRect.horizontal=false;
        dd.template=template.rectTransform;template.gameObject.SetActive(false);return dd;
    }
    Toggle ToggleControl(Transform parent,string name,float y)
    {
        Txt(name,parent,new Vector2(-205,y),new Vector2(260,42));
        var bg=Img(name+" Toggle",parent,toggleSprite,new Color(.2f,.16f,.3f),new Vector2(44,44),new Vector2(0,y));
        var t=bg.gameObject.AddComponent<Toggle>();t.targetGraphic=bg;
        var mark=Img("Checkmark",bg.transform,null,Color.yellow,new Vector2(23,23),Vector2.zero);t.graphic=mark;return t;
    }
    void Generate()
    {
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        Directory.CreateDirectory(Root+"/Scenes");string path=AssetDatabase.GenerateUniqueAssetPath(Root+"/Scenes/SettingsScene.unity");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var canvasGo=new GameObject("Fantasy Settings Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasGo.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
        var bg=Img("Background",canvasGo.transform,background,Color.black,new Vector2(1920,1080),Vector2.zero);Stretch(bg.rectTransform);
        var main=Img("Main Panel",canvasGo.transform,mainPanel,new Color(.14f,.12f,.2f,.97f),new Vector2(1550,920),Vector2.zero);
        var cat=Img("Category Panel",main.transform,categoryPanel,new Color(.22f,.15f,.29f),new Vector2(330,730),new Vector2(-560,0));
        var content=Img("Content Panel",main.transform,contentPanel,new Color(.32f,.24f,.3f),new Vector2(1070,730),new Vector2(180,0));
        Txt("SETTINGS",main.transform,new Vector2(0,410),new Vector2(550,70),48);
        var manager=new GameObject("Fantasy Settings Manager").AddComponent<FantasySettingsManager>();
        var pro=manager.gameObject.AddComponent<FantasySettingsPROController>();pro.settings=manager;
        pro.audioPage=new GameObject("Audio Page",typeof(RectTransform));pro.audioPage.transform.SetParent(content.transform,false);Stretch(pro.audioPage.GetComponent<RectTransform>());
        pro.graphicsPage=new GameObject("Graphics Page",typeof(RectTransform));pro.graphicsPage.transform.SetParent(content.transform,false);Stretch(pro.graphicsPage.GetComponent<RectTransform>());
        Txt("AUDIO",pro.audioPage.transform,new Vector2(0,275),new Vector2(400,65),38);
        manager.masterSlider=MakeSlider(pro.audioPage.transform,"Master",150,out manager.masterValue);
        manager.musicSlider=MakeSlider(pro.audioPage.transform,"Music",40,out manager.musicValue);
        manager.sfxSlider=MakeSlider(pro.audioPage.transform,"SFX",-70,out manager.sfxValue);
        pro.musicState=Txt("Music ON",pro.audioPage.transform,new Vector2(0,-220),new Vector2(250,55),25);
        Txt("GRAPHICS",pro.graphicsPage.transform,new Vector2(0,275),new Vector2(400,65),38);
        manager.resolutionDropdown=Dropdown(pro.graphicsPage.transform,"Resolution",140);
        manager.qualityDropdown=Dropdown(pro.graphicsPage.transform,"Quality",35);
        manager.fullscreenToggle=ToggleControl(pro.graphicsPage.transform,"Fullscreen",-80);
        pro.graphics=Btn("Graphics",cat.transform,graphics,new Vector2(-82,240),new Vector2(115,115),false);
        pro.audio=Btn("Audio",cat.transform,audio,new Vector2(82,240),new Vector2(115,115),false);
        pro.music=Btn("Music",cat.transform,music,new Vector2(-82,75),new Vector2(115,115),false);
        pro.fullscreen=Btn("Fullscreen",cat.transform,fullscreen,new Vector2(82,75),new Vector2(115,115),false);
        pro.back=Btn("Back",cat.transform,back,new Vector2(-82,-95),new Vector2(115,115),false);
        pro.close=Btn("Close",cat.transform,close,new Vector2(82,-95),new Vector2(115,115),false);
        pro.save=Btn("Save",main.transform,save,new Vector2(-400,-410),new Vector2(300,105),true);
        pro.apply=Btn("Apply",main.transform,apply,new Vector2(0,-410),new Vector2(300,105),true);
        pro.reset=Btn("Reset",main.transform,reset,new Vector2(400,-410),new Vector2(300,105),true);
        if(!Object.FindObjectOfType<EventSystem>())new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,path);AssetDatabase.Refresh();Selection.activeGameObject=canvasGo;
        EditorUtility.DisplayDialog("Generated", "Created: "+path+"\nAdd SettingsScene and MainMenu to Build Settings. Assign an AudioMixer for separate Music/SFX volume.","OK");
    }
}
