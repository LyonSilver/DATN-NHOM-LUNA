using UnityEngine;
using UnityEngine.UI;

namespace ChaosCard.EditorTools
{
    public class SimpleBobbingEffect : MonoBehaviour
    {
        public float amplitude = 10f;
        public float frequency = 1.5f;
        private Vector3 startPos;

        private void Start()
        {
            startPos = transform.localPosition;
        }

        private void Update()
        {
            float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
            transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
        }
    }

    public static class ChaosCardLobbyCharacterSetup
    {
        public static GameObject SetupStageAndCharacters(Transform parent, ChaosCardLobbyConfig config)
        {
            GameObject stageRoot = new GameObject("[STAGE_AND_CHARACTERS]", typeof(RectTransform));
            stageRoot.transform.SetParent(parent, false);
            RectTransform stageRect = stageRoot.GetComponent<RectTransform>();
            stageRect.anchorMin = Vector2.zero;
            stageRect.anchorMax = Vector2.one;
            stageRect.sizeDelta = Vector2.zero;

            if (config.enableBlueDragon)
            {
                SetupCharacterSlot(stageRoot.transform, "BlueDragon_Slot", new Vector2(0.2f, 0.5f), new Vector2(400, 500), 
                    config.blueDragonSprite, config.blueDragonPrefab, new Color(0.3f, 0.6f, 1f, 0.4f), "BLUE DRAGON PLACEHOLDER");
            }

            if (config.enableRedDragon)
            {
                SetupCharacterSlot(stageRoot.transform, "RedDragon_Slot", new Vector2(0.8f, 0.5f), new Vector2(400, 500), 
                    config.redDragonSprite, config.redDragonPrefab, new Color(1f, 0.4f, 0.2f, 0.4f), "RED DRAGON PLACEHOLDER");
            }

            if (config.enableMainCharacter)
            {
                SetupCharacterSlot(stageRoot.transform, "MainCharacter_Slot", new Vector2(0.5f, 0.45f), new Vector2(500, 650), 
                    config.mainCharacterSprite, config.mainCharacterPrefab, new Color(0.8f, 0.8f, 0.8f, 0.5f), "MAIN CHARACTER PLACEHOLDER");
            }

            return stageRoot;
        }

        private static void SetupCharacterSlot(Transform parent, string slotName, Vector2 anchor, Vector2 size, Sprite sprite, GameObject prefab, Color placeholderColor, string placeholderText)
        {
            GameObject slotGO = new GameObject(slotName, typeof(RectTransform));
            slotGO.transform.SetParent(parent, false);
            RectTransform rect = slotGO.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            slotGO.AddComponent<SimpleBobbingEffect>();

            if (prefab != null)
            {
                #if UNITY_EDITOR
                GameObject instance = UnityEditor.PrefabUtility.InstantiatePrefab(prefab, slotGO.transform) as GameObject;
                if (instance != null) instance.transform.localPosition = Vector3.zero;
                #endif
            }
            else if (sprite != null)
            {
                GameObject imgGO = new GameObject("SpriteDisplay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                imgGO.transform.SetParent(slotGO.transform, false);
                Image img = imgGO.GetComponent<Image>();
                img.sprite = sprite;
                img.preserveAspect = true;
                RectTransform imgRect = imgGO.GetComponent<RectTransform>();
                imgRect.anchorMin = Vector2.zero;
                imgRect.anchorMax = Vector2.one;
                imgRect.sizeDelta = Vector2.zero;
            }
            else
            {
                GameObject placeholder = ChaosCardLobbyUIFactory.CreatePanel(slotGO.transform, "PlaceholderVisual", placeholderColor, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                ChaosCardLobbyUIFactory.CreateText(placeholder.transform, "Label", placeholderText, 16, Color.white, TMPro.TextAlignmentOptions.Center);
            }
        }
    }
}