using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace ChaosCard.EditorTools
{
    public static class ChaosCardLobbyUIFactory
    {
        public static Canvas CreateRootCanvas(string name)
        {
            GameObject canvasGO = new GameObject(name);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;

            Image img = panel.GetComponent<Image>();
            img.color = color;
            return panel;
        }

        public static Button CreateFantasyButton(Transform parent, string name, string labelText, Color buttonColor, Color textColor, Vector2 size, UnityAction onClickAction = null)
        {
            GameObject btnGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(parent, false);

            RectTransform rect = btnGO.GetComponent<RectTransform>();
            rect.sizeDelta = size;

            Image img = btnGO.GetComponent<Image>();
            img.color = buttonColor;

            Button btn = btnGO.GetComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = buttonColor * 1.2f;
            colors.pressedColor = buttonColor * 0.8f;
            btn.colors = colors;

            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(btnGO.transform, false);

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.text = labelText;
            tmp.fontSize = 20;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            if (onClickAction != null)
            {
                btn.onClick.AddListener(onClickAction);
            }

            return btn;
        }

        public static TextMeshProUGUI CreateText(Transform parent, string name, string content, int fontSize, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
        {
            GameObject textGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;

            return tmp;
        }
    }
}