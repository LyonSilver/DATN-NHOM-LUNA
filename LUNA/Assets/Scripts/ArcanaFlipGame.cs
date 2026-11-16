using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Westervale.ArcanaFlip
{
    public class ArcanaFlipGame : MonoBehaviour
    {
        public Button[] cardButtons = new Button[5];
        public RectTransform[] cardFaces = new RectTransform[5];
        public Image[] cardImages = new Image[5];
        public Image[] cardBorders = new Image[5];
        public Text[] cardLabels = new Text[5];
        public GameObject[] glowObjects = new GameObject[5];
        public Text energyText, statusText, resultText;
        public Button flipButton, restartButton;
        public Sprite cardBack, legendaryFace, uncommonFace, commonFace;
        public Color legendaryColor = new Color(1f, .73f, .18f);
        public Color uncommonColor = new Color(.7f, .43f, 1f);
        public Color commonColor = new Color(.75f, .78f, .84f);

        private readonly int[] rarities = { 0, 1, 1, 1, 2 };
        private readonly List<int> selected = new List<int>();
        private bool revealing;
        private Coroutine[] glows = new Coroutine[5];

        private void Start()
        {
            for (int i = 0; i < cardButtons.Length; i++)
            {
                int index = i;
                cardButtons[i].onClick.AddListener(() => Select(index));
            }
            flipButton.onClick.AddListener(() => StartCoroutine(FlipSelected()));
            restartButton.onClick.AddListener(NewRound);
            NewRound();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < glows.Length; i++)
                if (glows[i] != null) StopCoroutine(glows[i]);
        }

        private void NewRound()
        {
            StopAllCoroutines();
            for (int i = 0; i < glows.Length; i++) glows[i] = null;
            revealing = false;
            selected.Clear();
            for (int i = rarities.Length - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                int temp = rarities[i]; rarities[i] = rarities[j]; rarities[j] = temp;
            }
            for (int i = 0; i < 5; i++)
            {
                cardButtons[i].interactable = true;
                cardFaces[i].localScale = Vector3.one;
                cardImages[i].sprite = cardBack;
                cardImages[i].color = cardBack ? Color.white : new Color(.20f, .15f, .36f);
                cardBorders[i].GetComponent<Outline>().effectColor = new Color(.65f, .53f, .85f);
                cardLabels[i].text = "CHỌN BÀI\n" + (i + 1);
                cardLabels[i].color = Color.white;
                glowObjects[i].SetActive(false);
            }
            resultText.text = "";
            statusText.text = "CHỌN 2 LÁ BÀI BÍ ẨN";
            restartButton.gameObject.SetActive(false);
            Refresh();
        }

        private void Select(int i)
        {
            if (revealing) return;
            if (selected.Contains(i)) selected.Remove(i);
            else if (selected.Count < 2) selected.Add(i);
            Refresh();
        }

        private void Refresh()
        {
            energyText.text = "NĂNG LƯỢNG  " + (2 - selected.Count) + "/2";
            flipButton.interactable = !revealing && selected.Count == 2;
            for (int i = 0; i < 5; i++)
            {
                if (!revealing)
                {
                    bool chosen = selected.Contains(i);
                    cardBorders[i].GetComponent<Outline>().effectColor = chosen ? new Color(.35f, .9f, 1f) : new Color(.65f, .53f, .85f);
                    cardLabels[i].text = chosen ? "ĐÃ CHỌN\n" + (i + 1) : "CHỌN BÀI\n" + (i + 1);
                    cardButtons[i].interactable = chosen || selected.Count < 2;
                }
            }
        }

        private IEnumerator FlipSelected()
        {
            if (revealing || selected.Count != 2) yield break;
            revealing = true;
            flipButton.interactable = false;
            statusText.text = "ĐANG LẬT BÀI...";
            for (int i = 0; i < 5; i++) cardButtons[i].interactable = false;
            bool foundLegendary = false;
            foreach (int i in selected)
            {
                yield return FlipOne(i);
                if (rarities[i] == 0) foundLegendary = true;
                yield return new WaitForSeconds(.3f);
            }
            statusText.text = "ĐÃ CÔNG BỐ KẾT QUẢ";
            resultText.text = foundLegendary ? "LEGENDARY! BẠN ĐÃ TRÚNG HUYỀN THOẠI!" : "HOÀN THÀNH! HẸN GẶP LẠI Ở VÁN SAU";
            resultText.color = foundLegendary ? legendaryColor : Color.white;
            restartButton.gameObject.SetActive(true);
        }

        private IEnumerator FlipOne(int i)
        {
            RectTransform face = cardFaces[i];
            float duration = .22f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                face.localScale = new Vector3(Mathf.Max(.01f, 1f - t / duration), 1f, 1f);
                yield return null;
            }
            int rarity = rarities[i];
            Color color = rarity == 0 ? legendaryColor : rarity == 1 ? uncommonColor : commonColor;
            cardBorders[i].GetComponent<Outline>().effectColor = color;
            cardImages[i].sprite = rarity == 0 ? legendaryFace : rarity == 1 ? uncommonFace : commonFace;
            cardImages[i].color = cardImages[i].sprite ? Color.white : Color.Lerp(color, Color.black, .67f);
            cardLabels[i].text = rarity == 0 ? "LEGENDARY" : rarity == 1 ? "UNCOMMON" : "COMMON";
            cardLabels[i].color = color;
            if (rarity == 0)
            {
                glowObjects[i].SetActive(true);
                glows[i] = StartCoroutine(PulseGlow(i));
            }
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                face.localScale = new Vector3(Mathf.Min(1f, t / duration), 1f, 1f);
                yield return null;
            }
            face.localScale = Vector3.one;
        }

        private IEnumerator PulseGlow(int i)
        {
            Image glow = glowObjects[i].GetComponent<Image>();
            Outline outline = glowObjects[i].GetComponent<Outline>();
            while (true)
            {
                float pulse = .5f + .5f * Mathf.Sin(Time.unscaledTime * 5f);
                glow.color = new Color(1f, .75f, .18f, .08f + pulse * .26f);
                outline.effectColor = new Color(1f, .8f, .2f, .3f + pulse * .7f);
                glowObjects[i].transform.localScale = Vector3.one * (1.02f + pulse * .055f);
                yield return null;
            }
        }
    }
}
