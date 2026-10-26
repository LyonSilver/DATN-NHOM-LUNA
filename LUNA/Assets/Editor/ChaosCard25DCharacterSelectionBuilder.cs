using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ChaosCard25DCharacterSelectionBuilder : EditorWindow
{
    string sceneName = "ChaosCardSelectHero";
    string backSceneName = "MENU_GAME";
    string confirmSceneName = "LienketLobby";
    bool loadSceneOnConfirm = true;
    Sprite background, farLayer, midLayer, foregroundLayer;
    Sprite[] portraits = new Sprite[6];
    Sprite[] artworks = new Sprite[6];
    string[] heroNames = { "Aether", "Frost Warden", "Crimson Mage", "Moon Knight", "Sylvan Oracle", "Void Reaper" };
    string[] heroTitles = { "Keeper of the Astral Gate", "Guardian of the Frozen Throne", "Master of Ember", "Blade of the Eclipse", "Voice of the Ancient Grove", "Harbinger of the Void" };
    string[] rarities = { "LEGENDARY", "EPIC", "EPIC", "RARE", "RARE", "MYTHIC" };
    Vector2 scroll;

    [MenuItem("Chaos Card/2.5D Hero Selection Builder")]
    static void Open() => GetWindow<ChaosCard25DCharacterSelectionBuilder>("Chaos Card 2.5D");

    void OnGUI()
    {
        EditorGUILayout.LabelField("CHAOS CARD  •  2.5D HERO SELECTION", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Phong cách minh họa fantasy 2D có chiều sâu: layered background, parallax, ánh sáng UI, particle giả lập. Không pixel art, không yêu cầu model 3D. Tool tạo scene MỚI và không sửa scene gameplay hiện tại.", MessageType.Info);
        sceneName = EditorGUILayout.TextField("New scene name", sceneName);
        backSceneName = EditorGUILayout.TextField("Back scene", backSceneName);
        confirmSceneName = EditorGUILayout.TextField("After confirm scene", confirmSceneName);
        loadSceneOnConfirm = EditorGUILayout.Toggle("Load scene on confirm", loadSceneOnConfirm);
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("BACKGROUND LAYERS", EditorStyles.boldLabel);
        background = (Sprite)EditorGUILayout.ObjectField("Main background", background, typeof(Sprite), false);
        farLayer = (Sprite)EditorGUILayout.ObjectField("Far layer (optional)", farLayer, typeof(Sprite), false);
        midLayer = (Sprite)EditorGUILayout.ObjectField("Mid layer (optional)", midLayer, typeof(Sprite), false);
        foregroundLayer = (Sprite)EditorGUILayout.ObjectField("Foreground (optional)", foregroundLayer, typeof(Sprite), false);
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("HEROES • portrait + full-body illustration", EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(330));
        for (int i = 0; i < heroNames.Length; i++)
        {
            EditorGUILayout.BeginVertical("box");
            heroNames[i] = EditorGUILayout.TextField("Name", heroNames[i]);
            heroTitles[i] = EditorGUILayout.TextField("Title", heroTitles[i]);
            rarities[i] = EditorGUILayout.TextField("Rarity", rarities[i]);
            portraits[i] = (Sprite)EditorGUILayout.ObjectField("Portrait", portraits[i], typeof(Sprite), false);
            artworks[i] = (Sprite)EditorGUILayout.ObjectField("Full-body artwork", artworks[i], typeof(Sprite), false);
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
        GUILayout.Space(8);
        GUI.backgroundColor = new Color(0.52f, 0.32f, 0.92f);
        if (GUILayout.Button("CREATE 2.5D CHARACTER SELECTION SCENE", GUILayout.Height(42))) BuildScene();
        GUI.backgroundColor = Color.white;
    }

    void BuildScene()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Stop Play Mode", "Dừng Play Mode trước khi tạo scene.", "OK");
            return;
        }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = sceneName;

        var canvasGO = new GameObject("CHAOS_CARD_25D_UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920,1080);
        scaler.matchWidthOrHeight = 0.5f;

        var controllerGO = new GameObject("HeroSelectionSystem");
        var controller = controllerGO.AddComponent<ChaosCard25DCharacterSelection>();
        controller.backSceneName = backSceneName;
        controller.confirmSceneName = confirmSceneName;
        controller.loadSceneOnConfirm = loadSceneOnConfirm;
        controller.heroes = new List<ChaosHeroData>();
        for (int i=0;i<heroNames.Length;i++)
        {
            controller.heroes.Add(new ChaosHeroData {
                heroName=heroNames[i], title=heroTitles[i], rarity=rarities[i],
                portrait=portraits[i], fullArtwork=artworks[i],
                description="A champion drawn into the eternal conflict over the Chaos Cards.",
                level=1, attack=68+i*5, defense=58+i*4, magic=75+i*3,
                skillName=new[]{"Astral Nova","Glacial Oath","Inferno Veil","Eclipse Slash","Verdant Echo","Void Collapse"}[i],
                skillDescription="Unleashes a signature power shaped by this hero's ancient origin."
            });
        }

        // Background stack
        var bg = CreateImage(canvasGO.transform,"Background_Base",Vector2.zero,Vector2.one,background,new Color(0.075f,0.055f,0.19f,1),true);
        if (background) bg.color = Color.white;
        var tint = CreateImage(bg.transform,"Cinematic_Tint",Vector2.zero,Vector2.one,null,new Color(0.025f,0.025f,0.12f,0.32f),false);
        tint.raycastTarget=false;
        var far = CreateLayer(canvasGO.transform,"Background_FarLayer",farLayer,new Vector2(-0.02f,-0.02f),new Vector2(1.02f,1.02f),-7f);
        var mid = CreateLayer(canvasGO.transform,"Background_MidLayer",midLayer,new Vector2(-0.01f,-0.01f),new Vector2(1.01f,1.01f),-12f);
        var fore = CreateLayer(canvasGO.transform,"Background_ForegroundLayer",foregroundLayer,new Vector2(-0.02f,-0.02f),new Vector2(1.02f,1.02f),-18f);
        var particleGO = new GameObject("Arcane_Particle_Field",typeof(RectTransform),typeof(ChaosCard25DParticleField));
        particleGO.transform.SetParent(canvasGO.transform,false);
        Stretch(particleGO.GetComponent<RectTransform>(),Vector2.zero,Vector2.one);
        particleGO.GetComponent<ChaosCard25DParticleField>().particleCount=38;

        // Header
        CreateText(canvasGO.transform,"GameLogo","CHAOS  CARD",30,new Vector2(0.045f,0.92f),new Vector2(0.24f,0.985f),new Color(1f,0.78f,0.37f),FontStyles.Bold,TextAlignmentOptions.Left);
        CreateText(canvasGO.transform,"HeaderTitle","CHOOSE YOUR CHAMPION",35,new Vector2(0.30f,0.89f),new Vector2(0.70f,0.96f),new Color(0.96f,0.91f,1f),FontStyles.Bold,TextAlignmentOptions.Center);
        CreateText(canvasGO.transform,"HeaderSubtitle","EVERY LEGEND BEGINS WITH A CHOICE",14,new Vector2(0.32f,0.855f),new Vector2(0.68f,0.89f),new Color(0.67f,0.73f,0.94f),FontStyles.Normal,TextAlignmentOptions.Center);
        CreatePanel(canvasGO.transform,"HeaderRule",new Vector2(0.35f,0.845f),new Vector2(0.65f,0.848f),new Color(0.72f,0.48f,1f,0.8f));

        // Left hero roster
        CreatePanel(canvasGO.transform,"RosterPanel",new Vector2(0.025f,0.145f),new Vector2(0.265f,0.82f),new Color(0.025f,0.035f,0.105f,0.86f));
        CreatePanel(canvasGO.transform,"RosterAccent",new Vector2(0.025f,0.145f),new Vector2(0.029f,0.82f),new Color(0.55f,0.31f,1f,0.95f));
        CreateText(canvasGO.transform,"RosterHeading","HERO ARCHIVE",19,new Vector2(0.045f,0.755f),new Vector2(0.245f,0.805f),new Color(0.76f,0.67f,1f),FontStyles.Bold,TextAlignmentOptions.Left);
        CreateText(canvasGO.transform,"RosterHint","Choose a legend",12,new Vector2(0.045f,0.72f),new Vector2(0.245f,0.75f),new Color(0.57f,0.64f,0.82f),FontStyles.Normal,TextAlignmentOptions.Left);
        var listGO = new GameObject("HeroRoster",typeof(RectTransform),typeof(VerticalLayoutGroup),typeof(ContentSizeFitter));
        listGO.transform.SetParent(canvasGO.transform,false);
        var listRT=listGO.GetComponent<RectTransform>(); listRT.anchorMin=new Vector2(0.04f,0.19f);listRT.anchorMax=new Vector2(0.25f,0.71f);listRT.offsetMin=Vector2.zero;listRT.offsetMax=Vector2.zero;
        var vlg=listGO.GetComponent<VerticalLayoutGroup>();vlg.spacing=9;vlg.padding=new RectOffset(0,4,4,4);vlg.childControlWidth=true;vlg.childControlHeight=false;vlg.childForceExpandWidth=true;vlg.childForceExpandHeight=false;
        var fitter=listGO.GetComponent<ContentSizeFitter>();fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;fitter.horizontalFit=ContentSizeFitter.FitMode.Unconstrained;

        // Central hero stage
        CreatePanel(canvasGO.transform,"HeroStageShadow",new Vector2(0.275f,0.135f),new Vector2(0.735f,0.82f),new Color(0.025f,0.03f,0.10f,0.35f));
        CreatePanel(canvasGO.transform,"HeroStageFrame",new Vector2(0.285f,0.145f),new Vector2(0.725f,0.81f),new Color(0.08f,0.065f,0.18f,0.32f));
        CreatePanel(canvasGO.transform,"HeroStageTopLine",new Vector2(0.33f,0.80f),new Vector2(0.68f,0.803f),new Color(0.77f,0.56f,1f,0.8f));
        var halo=CreateImage(canvasGO.transform,"HeroAura",new Vector2(0.37f,0.20f),new Vector2(0.65f,0.72f),null,new Color(0.34f,0.19f,0.72f,0.22f),false);
        halo.raycastTarget=false;
        var heroArt=CreateImage(canvasGO.transform,"SelectedHero_Artwork",new Vector2(0.335f,0.16f),new Vector2(0.685f,0.79f),null,Color.white,true);
        heroArt.preserveAspect=true;
        heroArt.gameObject.AddComponent<ChaosCard25DParallax>().strength=7f;
        CreateText(canvasGO.transform,"ArtworkHint","ASSIGN FULL-BODY 2D HERO ARTWORK",12,new Vector2(0.36f,0.15f),new Vector2(0.66f,0.19f),new Color(0.70f,0.68f,0.88f),FontStyles.Italic,TextAlignmentOptions.Center);
        CreateText(canvasGO.transform,"HeroFloorLabel","ASTRAL SUMMONING CIRCLE",11,new Vector2(0.38f,0.17f),new Vector2(0.62f,0.20f),new Color(0.64f,0.74f,1f),FontStyles.Bold,TextAlignmentOptions.Center);

        // Right information card
        CreatePanel(canvasGO.transform,"HeroInfoPanel",new Vector2(0.75f,0.145f),new Vector2(0.975f,0.82f),new Color(0.022f,0.032f,0.095f,0.91f));
        CreatePanel(canvasGO.transform,"HeroInfoTopAccent",new Vector2(0.75f,0.812f),new Vector2(0.975f,0.817f),new Color(0.92f,0.66f,0.29f,0.95f));
        CreateText(canvasGO.transform,"HeroRarity","LEGENDARY",13,new Vector2(0.775f,0.75f),new Vector2(0.95f,0.79f),new Color(0.87f,0.63f,1f),FontStyles.Bold,TextAlignmentOptions.Left);
        CreateText(canvasGO.transform,"HeroName","AETHER",30,new Vector2(0.775f,0.695f),new Vector2(0.955f,0.755f),new Color(1f,0.82f,0.47f),FontStyles.Bold,TextAlignmentOptions.Left);
        CreateText(canvasGO.transform,"HeroTitle","Keeper of the Astral Gate",14,new Vector2(0.775f,0.66f),new Vector2(0.955f,0.70f),new Color(0.76f,0.79f,0.95f),FontStyles.Italic,TextAlignmentOptions.Left);
        CreateText(canvasGO.transform,"HeroLevel","LEVEL 01",12,new Vector2(0.775f,0.625f),new Vector2(0.95f,0.655f),new Color(0.61f,0.83f,1f),FontStyles.Bold,TextAlignmentOptions.Left);
        CreateText(canvasGO.transform,"HeroDescription","A champion drawn into the eternal conflict over the Chaos Cards.",13,new Vector2(0.775f,0.535f),new Vector2(0.955f,0.625f),new Color(0.75f,0.79f,0.90f),FontStyles.Normal,TextAlignmentOptions.TopLeft);
        CreatePanel(canvasGO.transform,"InfoDivider",new Vector2(0.775f,0.515f),new Vector2(0.95f,0.518f),new Color(0.38f,0.34f,0.63f,0.85f));
        CreateText(canvasGO.transform,"StatsHeading","COMBAT ATTRIBUTES",13,new Vector2(0.775f,0.47f),new Vector2(0.95f,0.51f),new Color(0.72f,0.75f,1f),FontStyles.Bold,TextAlignmentOptions.Left);
        CreateStat(canvasGO.transform,"AttackStat","ATK","78",0.425f,new Color(1f,0.43f,0.39f));
        CreateStat(canvasGO.transform,"DefenseStat","DEF","62",0.375f,new Color(0.42f,0.76f,1f));
        CreateStat(canvasGO.transform,"MagicStat","MAG","95",0.325f,new Color(0.78f,0.52f,1f));
        CreateText(canvasGO.transform,"SkillHeading","SIGNATURE SKILL",13,new Vector2(0.775f,0.265f),new Vector2(0.95f,0.30f),new Color(0.73f,0.74f,1f),FontStyles.Bold,TextAlignmentOptions.Left);
        CreateText(canvasGO.transform,"SkillName","ASTRAL NOVA",16,new Vector2(0.775f,0.225f),new Vector2(0.95f,0.265f),new Color(1f,0.82f,0.47f),FontStyles.Bold,TextAlignmentOptions.Left);
        CreateText(canvasGO.transform,"SkillDescription","Unleashes a burst of astral energy.",12,new Vector2(0.775f,0.17f),new Vector2(0.95f,0.225f),new Color(0.70f,0.76f,0.91f),FontStyles.Normal,TextAlignmentOptions.TopLeft);

        var status=CreateText(canvasGO.transform,"SelectionStatus","HERO READY FOR SELECTION",12,new Vector2(0.35f,0.065f),new Vector2(0.65f,0.105f),new Color(0.72f,0.82f,1f),FontStyles.Bold,TextAlignmentOptions.Center);
        var prev=CreateButton(canvasGO.transform,"PreviousHeroButton","‹ PREVIOUS",new Vector2(0.39f,0.075f),new Vector2(150,48),new Color(0.075f,0.09f,0.19f,0.98f));
        var confirm=CreateButton(canvasGO.transform,"ConfirmHeroButton","CONFIRM HERO  ✦",new Vector2(0.50f,0.075f),new Vector2(235,54),new Color(0.34f,0.16f,0.58f,1f));
        var next=CreateButton(canvasGO.transform,"NextHeroButton","NEXT ›",new Vector2(0.61f,0.075f),new Vector2(150,48),new Color(0.075f,0.09f,0.19f,0.98f));
        var back=CreateButton(canvasGO.transform,"BackButton","← BACK",new Vector2(0.075f,0.075f),new Vector2(145,48),new Color(0.075f,0.09f,0.19f,0.98f));
        var template=CreateButton(canvasGO.transform,"HeroButtonTemplate","HERO",new Vector2(1.2f,1.2f),new Vector2(300,74),new Color(0.055f,0.075f,0.16f,0.96f));
        template.gameObject.SetActive(false);
        BuildHeroButtonTemplate(template.transform);

        controller.heroArtwork=heroArt;
        controller.heroListRoot=listGO.transform;
        controller.heroButtonPrefab=template;
        controller.confirmButton=confirm;controller.backButton=back;controller.previousButton=prev;controller.nextButton=next;
        controller.heroNameText=FindText(canvasGO.transform,"HeroName");
        controller.heroTitleText=FindText(canvasGO.transform,"HeroTitle");
        controller.heroDescriptionText=FindText(canvasGO.transform,"HeroDescription");
        controller.rarityText=FindText(canvasGO.transform,"HeroRarity");
        controller.levelText=FindText(canvasGO.transform,"HeroLevel");
        controller.attackText=FindText(canvasGO.transform,"AttackStat/Value");
        controller.defenseText=FindText(canvasGO.transform,"DefenseStat/Value");
        controller.magicText=FindText(canvasGO.transform,"MagicStat/Value");
        controller.skillNameText=FindText(canvasGO.transform,"SkillName");
        controller.skillDescriptionText=FindText(canvasGO.transform,"SkillDescription");
        controller.statusText=status;

        if (!Object.FindFirstObjectByType<EventSystem>())
            new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));

        string path = "Assets/Scenes/" + sceneName + ".unity";
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets","Scenes");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,path);
        AddToBuildSettings(path);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject=controllerGO;
        EditorUtility.DisplayDialog("Chaos Card 2.5D", "Scene created:\n"+path+"\n\nScene cũ không bị chỉnh sửa.\n\nTiếp theo: gán artwork/portrait thật và kiểm tra tên scene chuyển tiếp trong Build Settings.", "OK");
    }

    static void BuildHeroButtonTemplate(Transform root)
    {
        var img=root.GetComponent<Image>();img.color=new Color(0.055f,0.075f,0.16f,0.96f);
        var rt=root.GetComponent<RectTransform>();rt.sizeDelta=new Vector2(300,74);
        var portraitGO=new GameObject("Portrait",typeof(RectTransform),typeof(Image));portraitGO.transform.SetParent(root,false);
        var prt=portraitGO.GetComponent<RectTransform>();prt.anchorMin=new Vector2(0,0.08f);prt.anchorMax=new Vector2(0,0.92f);prt.pivot=new Vector2(0,0.5f);prt.anchoredPosition=new Vector2(8,0);prt.sizeDelta=new Vector2(54,0);
        portraitGO.GetComponent<Image>().color=new Color(0.34f,0.25f,0.54f,1);
        var name=CreateText(root,"HeroLabel","HERO",13,new Vector2(0.22f,0.48f),new Vector2(0.96f,0.90f),Color.white,FontStyles.Bold,TextAlignmentOptions.Left);
        var rarity=CreateText(root,"RarityLabel","RARITY",10,new Vector2(0.22f,0.10f),new Vector2(0.96f,0.48f),new Color(0.72f,0.61f,1f),FontStyles.Bold,TextAlignmentOptions.Left);
        var marker=CreatePanel(root,"SelectedMarker",new Vector2(0,0),new Vector2(0.018f,1),new Color(1f,0.77f,0.34f,1));
        marker.SetActive(false);
    }
    static void CreateStat(Transform root,string name,string label,string value,float y,Color color)
    {
        var group = new GameObject(name, typeof(RectTransform));
        group.transform.SetParent(root, false);
        var grt = group.GetComponent<RectTransform>(); grt.anchorMin = new Vector2(0, y); grt.anchorMax = new Vector2(1, y + 0.045f); grt.offsetMin = Vector2.zero; grt.offsetMax = Vector2.zero;
        CreateText(group.transform, "Label", label, 13, new Vector2(0,0), new Vector2(0.36f,1), color, FontStyles.Bold, TextAlignmentOptions.Left);
        CreateText(group.transform, "Value", value, 15, new Vector2(0.72f,0), new Vector2(1,1), Color.white, FontStyles.Bold, TextAlignmentOptions.Right);
        CreatePanel(group.transform, "Track", new Vector2(0,0.02f), new Vector2(0.68f,0.12f), new Color(0.25f,0.24f,0.4f,0.7f));
        CreatePanel(group.transform, "Fill", new Vector2(0,0.02f), new Vector2(0.48f,0.12f), new Color(color.r,color.g,color.b,0.75f));
    }
    static void AddToBuildSettings(string path)
    {
        var list=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach(var s in list) if(s.path==path) return;
        list.Add(new EditorBuildSettingsScene(path,true));EditorBuildSettings.scenes=list.ToArray();
    }
    static Image CreateImage(Transform parent,string name,Vector2 amin,Vector2 amax,Sprite sprite,Color color,bool preserve)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
        var rt=go.GetComponent<RectTransform>();rt.anchorMin=amin;rt.anchorMax=amax;rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;
        var image=go.GetComponent<Image>();image.sprite=sprite;image.color=color;image.preserveAspect=preserve;image.raycastTarget=false;
        return image;
    }
    static GameObject CreateLayer(Transform parent,string name,Sprite sprite,Vector2 amin,Vector2 amax,float strength)
    {
        if (!sprite) return new GameObject(name+"_EMPTY");
        var im=CreateImage(parent,name,amin,amax,sprite,Color.white,false);
        im.gameObject.AddComponent<ChaosCard25DParallax>().strength=strength;
        return im.gameObject;
    }
    static GameObject CreatePanel(Transform parent,string name,Vector2 amin,Vector2 amax,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
        var rt=go.GetComponent<RectTransform>();rt.anchorMin=amin;rt.anchorMax=amax;rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;
        var im=go.GetComponent<Image>();im.color=color;im.raycastTarget=false;return go;
    }
    static TMP_Text CreateText(Transform parent,string name,string value,int size,Vector2 amin,Vector2 amax,Color color,FontStyles style,TextAlignmentOptions align)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);
        var rt=go.GetComponent<RectTransform>();rt.anchorMin=amin;rt.anchorMax=amax;rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;
        var t=go.GetComponent<TextMeshProUGUI>();t.text=value;t.fontSize=size;t.color=color;t.fontStyle=style;t.alignment=align;t.enableWordWrapping=true;t.raycastTarget=false;
        return t;
    }
    static Button CreateButton(Transform parent,string name,string label,Vector2 anchor,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);
        var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=anchor;rt.anchoredPosition=Vector2.zero;rt.sizeDelta=size;
        var image=go.GetComponent<Image>();image.color=color;
        var button=go.GetComponent<Button>();var cb=button.colors;cb.normalColor=color;cb.highlightedColor=Color.Lerp(color,new Color(0.72f,0.56f,1f),0.35f);cb.pressedColor=Color.Lerp(color,Color.black,0.25f);cb.selectedColor=cb.highlightedColor;button.colors=cb;
        CreateText(go.transform,"Label",label,16,new Vector2(0,0),new Vector2(1,1),new Color(1f,0.86f,0.59f),FontStyles.Bold,TextAlignmentOptions.Center);
        return button;
    }
    static TMP_Text FindText(Transform root,string path)=>root.Find(path)?.GetComponent<TMP_Text>();
    static void Stretch(RectTransform rt,Vector2 min,Vector2 max){rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;}
}
