#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Westervale.ArcanaFlip;

namespace Westervale.ArcanaFlip.Editor
{
    public class ArcanaFlipFullUIBuilder : EditorWindow
    {
        private string scenePath = "Assets/ArcanaFlip/Generated/ArcanaFlip.unity";
        private Sprite background, back, legendary, uncommon, common;
        private bool overwrite = true;
        private Font font;

        [MenuItem("Tools/Westervale/Arcana Flip/Full UI Builder v5")]
        public static void Open() => GetWindow<ArcanaFlipFullUIBuilder>("Arcana Flip v5");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("ARCANA FLIP - FULL UI", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Creates a complete 1920x1080 UI. Overwrite existing scene is optional and always makes a timestamped backup first.", MessageType.Info);
            scenePath = EditorGUILayout.TextField("Scene Path", scenePath);
            overwrite = EditorGUILayout.Toggle("Allow Scene Overwrite", overwrite);
            background = (Sprite)EditorGUILayout.ObjectField("Background", background, typeof(Sprite), false);
            back = (Sprite)EditorGUILayout.ObjectField("Card Back", back, typeof(Sprite), false);
            legendary = (Sprite)EditorGUILayout.ObjectField("Legendary Face", legendary, typeof(Sprite), false);
            uncommon = (Sprite)EditorGUILayout.ObjectField("Uncommon Face", uncommon, typeof(Sprite), false);
            common = (Sprite)EditorGUILayout.ObjectField("Common Face", common, typeof(Sprite), false);
            EditorGUILayout.Space();
            if (GUILayout.Button("BUILD FULL UI SCENE", GUILayout.Height(42))) Build();
        }

        private void Build()
        {
            if (!scenePath.StartsWith("Assets/", StringComparison.Ordinal) || !scenePath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase) || scenePath.Contains(".."))
            {
                EditorUtility.DisplayDialog("Invalid path", "Scene path must be a .unity file inside Assets.", "OK");
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Stop Play Mode", "Exit Play Mode before building.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            bool exists = File.Exists(scenePath);
            if (exists && !overwrite)
            {
                EditorUtility.DisplayDialog("Scene exists", "Enable Allow Scene Overwrite or change the Scene Path.", "OK");
                return;
            }
            if (exists && !EditorUtility.DisplayDialog("OVERWRITE SCENE?", "A backup of the existing scene and its .meta will be created before replacing:\n" + scenePath, "Backup & Overwrite", "Cancel")) return;
            if (exists)
            {
                string backupDir = "Assets/ArcanaFlip/SceneBackups";
                Directory.CreateDirectory(backupDir);
                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                string backup = backupDir + "/" + Path.GetFileNameWithoutExtension(scenePath) + "_" + stamp + ".unity";
                File.Copy(scenePath, backup, false);
                // Backup is a separate scene asset: do not copy the original .meta/GUID.
                AssetDatabase.Refresh();
            }

            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var canvasGO = new GameObject("ArcanaFlip_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(null);

            var bg = Panel("Background", canvasGO.transform, new Vector2(0, 0), new Vector2(1920, 1080), new Color(.055f, .055f, .13f), background);
            bg.transform.SetAsFirstSibling();
            // Decorative UI with visible labels: replace images/text freely in Inspector.
            Panel("TopHeader", canvasGO.transform, new Vector2(0, 480), new Vector2(1880, 105), new Color(.10f, .08f, .19f, .94f));
            Label("GameTitle_EDIT_ME", canvasGO.transform, "ARCANA FLIP  •  LEGENDARY HUNT", new Vector2(0, 495), new Vector2(950, 52), 38, new Color(1f, .85f, .56f));
            var energy = Label("EnergyText", canvasGO.transform, "NĂNG LƯỢNG  2/2", new Vector2(-715, 424), new Vector2(400, 55), 29, Color.cyan);
            var status = Label("StatusText", canvasGO.transform, "CHỌN 2 LÁ BÀI BÍ ẨN", new Vector2(0, 399), new Vector2(940, 55), 28, Color.white);
            var result = Label("ResultText", canvasGO.transform, "", new Vector2(0, -275), new Vector2(1200, 58), 32, Color.white);

            var gameGO = new GameObject("ArcanaFlip_GameLogic");
            var game = gameGO.AddComponent<ArcanaFlipGame>();
            game.cardBack = back; game.legendaryFace = legendary; game.uncommonFace = uncommon; game.commonFace = common;
            game.energyText = energy; game.statusText = status; game.resultText = result;

            var area = Panel("CardSelectionArea_EDIT_POSITIONS", canvasGO.transform, new Vector2(0, 20), new Vector2(1700, 550), new Color(0f, 0f, 0f, 0f));
            area.raycastTarget = false;
            for (int i = 0; i < 5; i++)
            {
                float x = (i - 2) * 320;
                var slot = Panel("CardSlot_" + (i + 1).ToString("00") + "_BUTTON", area.transform, new Vector2(x, 0), new Vector2(275, 425), new Color(.12f, .10f, .24f, .95f));
                var btn = slot.gameObject.AddComponent<Button>();
                btn.targetGraphic = slot;
                var face = Panel("CardFace", slot.transform, Vector2.zero, new Vector2(260, 410), back ? Color.white : new Color(.2f, .15f, .36f), back);
                face.raycastTarget = false;
                var border = Panel("RarityBorder", face.transform, Vector2.zero, new Vector2(260, 410), new Color(.65f, .53f, .85f));
                border.transform.SetAsFirstSibling();
                border.raycastTarget = false;
                // Use Outline rather than covering the card face with an opaque border.
                border.color = new Color(0f, 0f, 0f, 0f);
                var outline = border.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(.65f, .53f, .85f);
                outline.effectDistance = new Vector2(5, -5);
                var glow = Panel("LegendaryGlow_VFX", face.transform, Vector2.zero, new Vector2(274, 424), new Color(1f, .75f, .18f, .1f));
                glow.raycastTarget = false;
                var glowOutline = glow.gameObject.AddComponent<Outline>();
                glowOutline.effectColor = Color.yellow;
                glowOutline.effectDistance = new Vector2(8, -8);
                glow.gameObject.SetActive(false);
                var txt = Label("CardLabel_EDIT_OR_HIDE", face.transform, "CHỌN BÀI\n" + (i + 1), new Vector2(0, -135), new Vector2(250, 90), 25, Color.white);
                txt.raycastTarget = false;
                game.cardButtons[i] = btn;
                game.cardFaces[i] = face.rectTransform;
                game.cardImages[i] = face;
                game.cardBorders[i] = border;
                game.cardLabels[i] = txt;
                game.glowObjects[i] = glow.gameObject;
            }
            var flip = MakeButton("FlipCards_BUTTON", canvasGO.transform, "LẬT 2 LÁ BÀI", new Vector2(-210, -393), new Vector2(355, 84), new Color(.38f, .25f, .67f));
            var again = MakeButton("PlayAgain_BUTTON", canvasGO.transform, "CHƠI LẠI", new Vector2(210, -393), new Vector2(355, 84), new Color(.30f, .44f, .48f));
            game.flipButton = flip; game.restartButton = again;

            Directory.CreateDirectory(Path.GetDirectoryName(scenePath));
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();
            if (saved) EditorUtility.DisplayDialog("Scene ready", "Created scene:\n" + scenePath + "\n\nFive card buttons, two-energy selection, flip animation and Legendary glow are wired.", "OK");
            else EditorUtility.DisplayDialog("Save failed", "Could not save the scene. Check Console.", "OK");
        }

        private static Image Panel(string name, Transform parent, Vector2 position, Vector2 size, Color color, Sprite sprite = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Simple;
            img.color = color;
            return img;
        }

        private Text Label(string name, Transform parent, string value, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var txt = go.GetComponent<Text>();
            txt.font = font;
            txt.text = value;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.raycastTarget = false;
            return txt;
        }

        private Button MakeButton(string name, Transform parent, string label, Vector2 pos, Vector2 size, Color color)
        {
            var img = Panel(name, parent, pos, size, color);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            Label("ButtonLabel_EDIT_OR_HIDE", img.transform, label, Vector2.zero, size, 30, Color.white);
            return btn;
        }
    }
}
#endif
