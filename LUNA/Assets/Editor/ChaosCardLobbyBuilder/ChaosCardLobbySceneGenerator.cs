using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace ChaosCard.EditorTools
{
    public static class ChaosCardLobbySceneGenerator
    {
        public static void GenerateScene(ChaosCardLobbyConfig config)
        {
            if (!Directory.Exists(config.sceneDirectory))
            {
                Directory.CreateDirectory(config.sceneDirectory);
            }

            string fullPath = Path.Combine(config.sceneDirectory, config.sceneName + ".unity");

            if (File.Exists(fullPath))
            {
                bool overwrite = EditorUtility.DisplayDialog("Xác nhận ghi đè Scene",
                    $"Scene tại '{fullPath}' đã tồn tại. Bạn có muốn ghi đè không?", "Ghi đè", "Hủy");
                if (!overwrite) return;
            }

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            GameObject rootGO = new GameObject("[CHAOS_CARD_LOBBY_ROOT]");
            Undo.RegisterCreatedObjectUndo(rootGO, "Create Chaos Card Lobby");

            Canvas canvas = ChaosCardLobbyUIFactory.CreateRootCanvas("LobbyCanvas");
            canvas.transform.SetParent(rootGO.transform);

            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
                es.transform.SetParent(rootGO.transform);
            }

            GameObject bgPanel = ChaosCardLobbyUIFactory.CreatePanel(canvas.transform, "BackgroundPanel", Color.black, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            if (config.backgroundSprite != null)
            {
                Image bgImg = bgPanel.GetComponent<Image>();
                bgImg.sprite = config.backgroundSprite;
                bgImg.color = Color.white;
            }

            GameObject stage = ChaosCardLobbyCharacterSetup.SetupStageAndCharacters(canvas.transform, config);
            stage.transform.SetParent(canvas.transform, false);

            BuildPlayerHeader(canvas.transform, config);
            BuildLeftMenu(canvas.transform, config);
            BuildRightMenu(canvas.transform, config);
            BuildBottomArea(canvas.transform, config);

            EditorSceneManager.SaveScene(newScene, fullPath);
            Debug.Log($"<color=green>[Chaos Card Tool]</color> Đã tạo thành công Lobby Scene tại: {fullPath}");
        }

        private static void BuildPlayerHeader(Transform parent, ChaosCardLobbyConfig config)
        {
            GameObject header = ChaosCardLobbyUIFactory.CreatePanel(parent, "UI_PlayerHeader", config.primaryThemeColor, new Vector2(0, 1), new Vector2(0, 1), new Vector2(250, -60), new Vector2(450, 90));

            GameObject avatar = ChaosCardLobbyUIFactory.CreatePanel(header.transform, "Avatar", config.accentGoldColor, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(45, 0), new Vector2(70, 70));
            ChaosCardLobbyUIFactory.CreateText(avatar.transform, "Text", "AVATAR", 12, Color.black, TextAlignmentOptions.Center);

            ChaosCardLobbyUIFactory.CreateText(header.transform, "PlayerName", "Duelist Player", 20, Color.white).rectTransform.anchoredPosition = new Vector2(100, 20);
            ChaosCardLobbyUIFactory.CreateText(header.transform, "PlayerLevel", "Lv. 50", 14, config.accentGoldColor).rectTransform.anchoredPosition = new Vector2(100, -5);

            ChaosCardLobbyUIFactory.CreateText(header.transform, "GoldText", "Gold: 25,000", 14, Color.yellow).rectTransform.anchoredPosition = new Vector2(250, 20);
            ChaosCardLobbyUIFactory.CreateText(header.transform, "GemsText", "Gems: 1,200", 14, Color.cyan).rectTransform.anchoredPosition = new Vector2(250, -5);
        }

        private static void BuildLeftMenu(Transform parent, ChaosCardLobbyConfig config)
        {
            GameObject container = new GameObject("UI_LeftMenu", typeof(RectTransform), typeof(VerticalLayoutGroup));
            container.transform.SetParent(parent, false);
            RectTransform rect = container.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(120, 0);
            rect.sizeDelta = new Vector2(200, 350);

            VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.childControlHeight = true;
            layout.childControlWidth = true;

            string[] buttons = { "Card Deck", "Collection", "Quests", "Events", "Settings" };
            foreach (var name in buttons)
            {
                ChaosCardLobbyUIFactory.CreateFantasyButton(container.transform, "Btn_" + name.Replace(" ", ""), name, config.primaryThemeColor, Color.white, new Vector2(200, 50));
            }
        }

        private static void BuildRightMenu(Transform parent, ChaosCardLobbyConfig config)
        {
            GameObject container = new GameObject("UI_RightMenu", typeof(RectTransform), typeof(VerticalLayoutGroup));
            container.transform.SetParent(parent, false);
            RectTransform rect = container.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.anchoredPosition = new Vector2(-120, 0);
            rect.sizeDelta = new Vector2(200, 280);

            VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.childControlHeight = true;
            layout.childControlWidth = true;

            string[] buttons = { "Card Shop", "Card Packs", "Daily Rewards", "Mail" };
            foreach (var name in buttons)
            {
                ChaosCardLobbyUIFactory.CreateFantasyButton(container.transform, "Btn_" + name.Replace(" ", ""), name, config.primaryThemeColor, Color.white, new Vector2(200, 50));
            }
        }

        private static void BuildBottomArea(Transform parent, ChaosCardLobbyConfig config)
        {
            GameObject bottomGO = new GameObject("UI_BottomArea", typeof(RectTransform));
            bottomGO.transform.SetParent(parent, false);
            RectTransform bRect = bottomGO.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0);
            bRect.anchorMax = new Vector2(0.5f, 0);
            bRect.anchoredPosition = new Vector2(0, 100);
            bRect.sizeDelta = new Vector2(1200, 150);

            ChaosCardLobbyUIFactory.CreateText(bottomGO.transform, "RankedLabel", "RANKED DUEL - SEASON 1", 22, config.accentGoldColor, TextAlignmentOptions.Center).rectTransform.anchoredPosition = new Vector2(0, 70);

            Button battleBtn = ChaosCardLobbyUIFactory.CreateFantasyButton(bottomGO.transform, "Btn_BATTLE", "BATTLE", new Color(0.85f, 0.2f, 0.2f, 1f), Color.white, new Vector2(260, 80));
            battleBtn.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            GameObject cardsContainer = ChaosCardLobbyUIFactory.CreatePanel(bottomGO.transform, "FeaturedCards", new Color(0, 0, 0, 0.3f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(-300, 0), new Vector2(320, 120));
            ChaosCardLobbyUIFactory.CreateText(cardsContainer.transform, "Label", "FEATURED CARDS SLOTS", 14, Color.gray, TextAlignmentOptions.Center);
        }
    }
}