#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Westervale.ArcanaTrial;

namespace Westervale.ArcanaTrial.Editor
{
    public sealed class ArcanaTrialBuilder : EditorWindow
    {
        const string Root = "Assets/ArcanaTrialGenerated";
        [SerializeField] Sprite background;
        [SerializeField] Font uiFont;
        [SerializeField] bool openAfterBuild = true;

        [MenuItem("Tools/Westervale/Arcana Trial Auto Builder")]
        public static void Open() => GetWindow<ArcanaTrialBuilder>("Arcana Trial Builder");

        void OnGUI()
        {
            EditorGUILayout.LabelField("ARCANA TRIAL • MINI GAME", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Tạo scene mới, không sửa scene hiện tại. Luật Kim/Mộc/Thủy/Hỏa/Thổ theo vòng tương khắc truyền thống (có thể chỉnh trong script).", MessageType.Info);
            background = (Sprite)EditorGUILayout.ObjectField("Background (optional)", background, typeof(Sprite), false);
            uiFont = (Font)EditorGUILayout.ObjectField("Font (optional)", uiFont, typeof(Font), false);
            openAfterBuild = EditorGUILayout.Toggle("Open new scene after build", openAfterBuild);
            EditorGUILayout.Space();
            if (GUILayout.Button("BUILD NEW ARCANA TRIAL SCENE", GUILayout.Height(38))) Build();
            EditorGUILayout.LabelField("Output: " + Root + "/ArcanaTrial.unity", EditorStyles.miniLabel);
        }

        void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureFolder("Assets", "ArcanaTrialGenerated");
            string scenePath = AssetDatabase.GenerateUniqueAssetPath(Root + "/ArcanaTrial.unity");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraGo = new GameObject("ArcanaTrialCamera", typeof(Camera));
            var cam = cameraGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.065f,.052f,.105f);
            cam.orthographic = true;
            cameraGo.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(0,0,-10);

            var canvasGo = new GameObject("ArcanaTrialCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920,1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            var root = canvasGo.GetComponent<RectTransform>();
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;
            Image panel = MakeImage("Background", root, new Color(.09f,.07f,.14f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            if (background != null) { panel.sprite = background; panel.color = Color.white; panel.type = Image.Type.Simple; panel.preserveAspect = false; }
            MakeImage("DarkOverlay", root, new Color(.045f,.035f,.09f,.73f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var game = new GameObject("ArcanaTrialController", typeof(ArcanaTrialGame)).GetComponent<ArcanaTrialGame>();
            game.titleText = MakeText("Title", root, "ARCANA TRIAL", 54, new Vector2(.1f,.85f), new Vector2(.9f,.97f));
            game.roundText = MakeText("Round", root, "LƯỢT 1/5", 30, new Vector2(.1f,.77f), new Vector2(.9f,.84f));
            game.healthText = MakeText("Health", root, "EREN 100/100 HP", 28, new Vector2(.1f,.69f), new Vector2(.9f,.77f));
            game.enemyText = MakeText("EnemyCard", root, "? CARD BÍ ẨN ?", 46, new Vector2(.28f,.49f), new Vector2(.72f,.68f));
            game.hintText = MakeText("Hint", root, "Gợi ý", 26, new Vector2(.15f,.38f), new Vector2(.85f,.49f));
            game.revealButton = MakeButton("RevealButton", root, "LẬT BÀI", new Color(.34f,.26f,.48f), new Vector2(.39f,.31f), new Vector2(.61f,.38f));
            for (int i = 0; i < 5; i++)
            {
                float left = .11f + i*.16f;
                string[] labels = {"KIM","MỘC","THỦY","HỎA","THỔ"};
                Color[] colors = {new Color(.64f,.68f,.75f),new Color(.24f,.53f,.34f),new Color(.22f,.48f,.7f),new Color(.72f,.27f,.23f),new Color(.57f,.44f,.26f)};
                var b = MakeButton("Card_"+labels[i],root,labels[i],colors[i],new Vector2(left,.17f),new Vector2(left+.14f,.28f));
                game.elementButtons[i] = b;
                game.elementLabels[i] = b.GetComponentInChildren<Text>();
                game.elementImages[i] = b.GetComponent<Image>();
            }
            game.resultText = MakeText("Result", root, "Chọn một lá bài", 25, new Vector2(.12f,.085f), new Vector2(.88f,.16f));
            game.nextButton = MakeButton("NextButton",root,"TIẾP TỤC",new Color(.37f,.31f,.56f),new Vector2(.39f,.015f),new Vector2(.61f,.085f));
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                es.AddComponent<StandaloneInputModule>();
#endif
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene,scenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[ArcanaTrial] Created: " + scenePath);
            if (!openAfterBuild) EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
            EditorUtility.DisplayDialog("Arcana Trial", "Scene created:\n" + scenePath + "\nPress Play to test.", "OK");
        }

        static void EnsureFolder(string parent,string name)
        {
            if (!AssetDatabase.IsValidFolder(parent+"/"+name)) AssetDatabase.CreateFolder(parent,name);
        }
        static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 offsetMin,Vector2 offsetMax)
        {
            var go = new GameObject(name,typeof(RectTransform));
            go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>();
            r.anchorMin=min; r.anchorMax=max; r.offsetMin=offsetMin; r.offsetMax=offsetMax;
            return r;
        }
        static Image MakeImage(string name,Transform parent,Color color,Vector2 min,Vector2 max,Vector2 offMin,Vector2 offMax)
        {
            var r=Rect(name,parent,min,max,offMin,offMax);
            var im=r.gameObject.AddComponent<Image>(); im.color=color; return im;
        }
        Text MakeText(string name,Transform parent,string content,int size,Vector2 min,Vector2 max)
        {
            var r=Rect(name,parent,min,max,Vector2.zero,Vector2.zero);
            var t=r.gameObject.AddComponent<Text>();
            t.text=content; t.font=uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize=size; t.color=new Color(.95f,.92f,.98f); t.alignment=TextAnchor.MiddleCenter;
            t.resizeTextForBestFit=true; t.resizeTextMinSize=14; t.resizeTextMaxSize=size;
            return t;
        }
        Button MakeButton(string name,Transform parent,string label,Color color,Vector2 min,Vector2 max)
        {
            var image=MakeImage(name,parent,color,min,max,Vector2.zero,Vector2.zero);
            var b=image.gameObject.AddComponent<Button>();
            var colors=b.colors; colors.highlightedColor=Color.Lerp(color,Color.white,.2f); colors.pressedColor=Color.Lerp(color,Color.black,.3f); b.colors=colors;
            var text=MakeText("Label",image.transform,label,25,Vector2.zero,Vector2.one);
            text.raycastTarget=false;
            return b;
        }
    }
}
#endif
