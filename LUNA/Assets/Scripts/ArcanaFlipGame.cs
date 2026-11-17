using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Westervale.ArcanaFlip
{
    public class ArcanaFlipGame : MonoBehaviour
    {
        private const int CardCount = 5;

        [Header("Five cards (required)")]
        public Button[] cardButtons = new Button[CardCount];
        public RectTransform[] cardFaces = new RectTransform[CardCount];
        public Image[] cardImages = new Image[CardCount];
        public Image[] cardBorders = new Image[CardCount];

        [Header("Optional card UI")]
        public Text[] cardLabels = new Text[CardCount];
        public GameObject[] glowObjects = new GameObject[CardCount];

        [Header("Game UI")]
        public Text energyText, statusText, resultText;
        public Button flipButton, restartButton;

        [Header("Card sprites")]
        public Sprite cardBack, legendaryFace, uncommonFace, commonFace;
        public Color legendaryColor = new Color(1f, .73f, .18f);
        public Color uncommonColor = new Color(.7f, .43f, 1f);
        public Color commonColor = new Color(.75f, .78f, .84f);

        private readonly int[] rarities = { 0, 1, 1, 1, 2 };
        private readonly List<int> selected = new List<int>(2);
        private readonly Coroutine[] glows = new Coroutine[CardCount];
        private readonly Vector3[] originalScales = new Vector3[CardCount];
        private readonly Color[] originalGlowColors = new Color[CardCount];
        private readonly Vector3[] originalGlowScales = new Vector3[CardCount];
        private readonly Color[] originalGlowOutlineColors = new Color[CardCount];
        private bool revealing;
        private bool initialized;

        private void Start()
        {
            if (!ValidateRequiredReferences())
            {
                enabled = false;
                return;
            }

            for (int i = 0; i < CardCount; i++)
            {
                int index = i;
                originalScales[i] = cardFaces[i].localScale;
                cardButtons[i].onClick.AddListener(() => Select(index));

                GameObject glow = GetGlow(i);
                if (glow != null)
                {
                    originalGlowScales[i] = glow.transform.localScale;
                    Image glowImage = glow.GetComponent<Image>();
                    Outline glowOutline = glow.GetComponent<Outline>();
                    if (glowImage != null) originalGlowColors[i] = glowImage.color;
                    if (glowOutline != null) originalGlowOutlineColors[i] = glowOutline.effectColor;
                }
            }

            if (flipButton != null) flipButton.onClick.AddListener(OnFlipClicked);
            if (restartButton != null) restartButton.onClick.AddListener(NewRound);
            initialized = true;
            NewRound();
        }

        private bool ValidateRequiredReferences()
        {
            if (cardButtons == null || cardFaces == null || cardImages == null || cardBorders == null ||
                cardButtons.Length < CardCount || cardFaces.Length < CardCount ||
                cardImages.Length < CardCount || cardBorders.Length < CardCount)
            {
                Debug.LogError("[ArcanaFlip] Assign five Buttons, Faces, Images and Borders in Inspector.", this);
                return false;
            }

            for (int i = 0; i < CardCount; i++)
            {
                if (cardButtons[i] == null || cardFaces[i] == null || cardImages[i] == null || cardBorders[i] == null)
                {
                    Debug.LogError($"[ArcanaFlip] Missing required reference at card index {i}.", this);
                    return false;
                }
            }

            if (flipButton == null || restartButton == null)
            {
                Debug.LogError("[ArcanaFlip] Assign Flip Button and Restart Button.", this);
                return false;
            }
            return true;
        }

        private Text GetLabel(int i) => cardLabels != null && i < cardLabels.Length ? cardLabels[i] : null;
        private GameObject GetGlow(int i) => glowObjects != null && i < glowObjects.Length ? glowObjects[i] : null;

        private static void SetText(Text label, string value)
        {
            if (label != null) label.text = value;
        }

        private static void SetOutline(Image image, Color color)
        {
            if (image == null) return;
            Outline outline = image.GetComponent<Outline>();
            if (outline != null) outline.effectColor = color;
        }

        public void NewRound()
        {
            if (!initialized) return;
            StopAllCoroutines();
            for (int i = 0; i < CardCount; i++) glows[i] = null;
            revealing = false;
            selected.Clear();

            for (int i = rarities.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = rarities[i];
                rarities[i] = rarities[j];
                rarities[j] = temp;
            }

            for (int i = 0; i < CardCount; i++)
            {
                cardButtons[i].interactable = true;
                cardFaces[i].localScale = originalScales[i];
                cardImages[i].sprite = cardBack;
                cardImages[i].color = cardBack != null ? Color.white : new Color(.20f, .15f, .36f);
                SetOutline(cardBorders[i], new Color(.65f, .53f, .85f));
                Text label = GetLabel(i);
                SetText(label, "CHỌN BÀI\n" + (i + 1));
                if (label != null) label.color = Color.white;

                GameObject glow = GetGlow(i);
                if (glow != null)
                {
                    glow.transform.localScale = originalGlowScales[i];
                    Image image = glow.GetComponent<Image>();
                    Outline outline = glow.GetComponent<Outline>();
                    if (image != null) image.color = originalGlowColors[i];
                    if (outline != null) outline.effectColor = originalGlowOutlineColors[i];
                    glow.SetActive(false);
                }
            }

            SetText(resultText, "");
            SetText(statusText, "CHỌN 2 LÁ BÀI BÍ ẨN");
            restartButton.gameObject.SetActive(false);
            Refresh();
        }

        private void Select(int i)
        {
            if (revealing || i < 0 || i >= CardCount) return;
            if (selected.Contains(i)) selected.Remove(i);
            else if (selected.Count < 2) selected.Add(i);
            Refresh();
        }

        private void Refresh()
        {
            SetText(energyText, "NĂNG LƯỢNG  " + (2 - selected.Count) + "/2");
            if (flipButton != null) flipButton.interactable = !revealing && selected.Count == 2;

            for (int i = 0; i < CardCount; i++)
            {
                if (revealing) continue;
                bool chosen = selected.Contains(i);
                SetOutline(cardBorders[i], chosen ? new Color(.35f, .9f, 1f) : new Color(.65f, .53f, .85f));
                SetText(GetLabel(i), chosen ? "ĐÃ CHỌN\n" + (i + 1) : "CHỌN BÀI\n" + (i + 1));
                cardButtons[i].interactable = chosen || selected.Count < 2;
            }
        }

        private void OnFlipClicked()
        {
            if (!revealing && selected.Count == 2) StartCoroutine(FlipSelected());
        }

        private IEnumerator FlipSelected()
        {
            if (revealing || selected.Count != 2) yield break;
            revealing = true;
            flipButton.interactable = false;
            SetText(statusText, "ĐANG LẬT BÀI...");
            for (int i = 0; i < CardCount; i++) cardButtons[i].interactable = false;

            bool foundLegendary = false;
            foreach (int i in selected)
            {
                yield return FlipOne(i);
                if (rarities[i] == 0) foundLegendary = true;
                yield return new WaitForSeconds(.3f);
            }

            SetText(statusText, "ĐÃ CÔNG BỐ KẾT QUẢ");
            SetText(resultText, foundLegendary ? "LEGENDARY! BẠN ĐÃ TRÚNG HUYỀN THOẠI!" : "HOÀN THÀNH! HẸN GẶP LẠI Ở VÁN SAU");
            if (resultText != null) resultText.color = foundLegendary ? legendaryColor : Color.white;
            restartButton.gameObject.SetActive(true);
        }

        private IEnumerator FlipOne(int i)
        {
            RectTransform face = cardFaces[i];
            Vector3 baseScale = originalScales[i];
            const float duration = .22f;

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                face.localScale = new Vector3(baseScale.x * Mathf.Max(.01f, 1f - t / duration), baseScale.y, baseScale.z);
                yield return null;
            }
            face.localScale = new Vector3(baseScale.x * .01f, baseScale.y, baseScale.z);

            int rarity = rarities[i];
            Color color = rarity == 0 ? legendaryColor : rarity == 1 ? uncommonColor : commonColor;
            SetOutline(cardBorders[i], color);
            cardImages[i].sprite = rarity == 0 ? legendaryFace : rarity == 1 ? uncommonFace : commonFace;
            cardImages[i].color = cardImages[i].sprite != null ? Color.white : Color.Lerp(color, Color.black, .67f);
            Text label = GetLabel(i);
            SetText(label, rarity == 0 ? "LEGENDARY" : rarity == 1 ? "UNCOMMON" : "COMMON");
            if (label != null) label.color = color;

            GameObject glow = GetGlow(i);
            if (rarity == 0 && glow != null)
            {
                glow.SetActive(true);
                if (glow.GetComponent<Image>() != null || glow.GetComponent<Outline>() != null)
                    glows[i] = StartCoroutine(PulseGlow(i));
            }

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                face.localScale = new Vector3(baseScale.x * Mathf.Min(1f, t / duration), baseScale.y, baseScale.z);
                yield return null;
            }
            face.localScale = baseScale;
        }

        private IEnumerator PulseGlow(int i)
        {
            GameObject glowObject = GetGlow(i);
            if (glowObject == null) yield break;
            Image glow = glowObject.GetComponent<Image>();
            Outline outline = glowObject.GetComponent<Outline>();
            Vector3 baseScale = originalGlowScales[i];
            while (glowObject != null && glowObject.activeInHierarchy)
            {
                float pulse = .5f + .5f * Mathf.Sin(Time.unscaledTime * 5f);
                if (glow != null) glow.color = new Color(1f, .75f, .18f, .08f + pulse * .26f);
                if (outline != null) outline.effectColor = new Color(1f, .8f, .2f, .3f + pulse * .7f);
                glowObject.transform.localScale = baseScale * (1.02f + pulse * .055f);
                yield return null;
            }
        }
    }
}
