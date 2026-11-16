using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Westervale.ArcanaTrial
{
    public enum Element { Metal, Wood, Water, Fire, Earth }

    public sealed class ArcanaTrialGame : MonoBehaviour
    {
        [Header("UI - assigned by builder")]
        public Text titleText, roundText, enemyText, hintText, resultText, healthText;
        public Button revealButton, nextButton;
        public Button[] elementButtons = new Button[5];
        public Text[] elementLabels = new Text[5];
        public Image[] elementImages = new Image[5];

        [Header("Gameplay")]
        [Range(1, 20)] public int totalRounds = 5;
        [Range(1, 100)] public int startingHP = 100;
        [Range(0, 100)] public int damageOnFailure = 10;
        [Range(0, 100)] public int damageOnSuccess = 30;
        public bool allowReveal = true;
        public bool deterministicDemo = false;
        public int randomSeed = 12345;

        static readonly string[] Names = { "KIM", "MỘC", "THỦY", "HỎA", "THỔ" };
        static readonly Color[] Colors = {
            new Color(.72f,.77f,.84f), new Color(.35f,.69f,.47f),
            new Color(.34f,.63f,.85f), new Color(.87f,.40f,.31f),
            new Color(.74f,.61f,.39f)
        };
        // Metal beats Wood; Wood beats Earth; Earth beats Water; Water beats Fire; Fire beats Metal.
        static readonly Element[] CounterTo = { Element.Fire, Element.Metal, Element.Earth, Element.Water, Element.Wood };
        readonly List<Element> history = new List<Element>();
        System.Random rng;
        int round, hp, score;
        Element enemy;
        bool revealed, answered, finished;

        void Awake()
        {
            for (int i = 0; i < elementButtons.Length && i < 5; i++)
            {
                int index = i;
                if (elementButtons[i] != null) elementButtons[i].onClick.AddListener(() => Choose((Element)index));
            }
            if (revealButton != null) revealButton.onClick.AddListener(Reveal);
            if (nextButton != null) nextButton.onClick.AddListener(Next);
        }

        void Start() { Restart(); }

        public void Restart()
        {
            rng = deterministicDemo ? new System.Random(randomSeed) : new System.Random();
            history.Clear(); round = 1; hp = startingHP; score = 0; finished = false;
            BeginRound();
        }

        void BeginRound()
        {
            enemy = (Element)rng.Next(0, 5);
            revealed = false; answered = false;
            history.Add(enemy);
            Render();
        }

        public void Reveal()
        {
            if (answered || finished || !allowReveal) return;
            revealed = true;
            Render();
        }

        public void Choose(Element player)
        {
            if (answered || finished || (int)player < 0 || (int)player > 4) return;
            answered = true;
            bool success = CounterTo[(int)enemy] == player;
            if (success) score += damageOnSuccess;
            else hp = Mathf.Max(0, hp - damageOnFailure);
            if (hp <= 0 || round >= totalRounds) finished = true;
            Render();
            if (resultText != null)
            {
                resultText.text = success
                    ? "KHẮC CHẾ THÀNH CÔNG! +" + damageOnSuccess + " điểm."
                    : "CHƯA ĐÚNG! -" + damageOnFailure + " HP.";
                if (finished) resultText.text += "\nKẾT THÚC! Điểm: " + score + " | HP: " + hp;
            }
        }

        public void Next()
        {
            if (finished) { Restart(); return; }
            if (!answered) return;
            round++;
            BeginRound();
        }

        void Render()
        {
            if (titleText != null) titleText.text = "ARCANA TRIAL  •  WESTERVALE";
            if (roundText != null) roundText.text = "LƯỢT " + round + "/" + totalRounds + "   |   ĐIỂM " + score;
            if (healthText != null) healthText.text = "EREN  " + hp + "/" + startingHP + " HP";
            if (enemyText != null) enemyText.text = (revealed || answered) ? Names[(int)enemy] : "?  CARD BÍ ẨN  ?";
            if (hintText != null)
            {
                // Hint is always truthful; only the enemy's identity remains hidden.
                hintText.text = (revealed || answered)
                    ? "Hệ đối thủ: " + Names[(int)enemy] + "\nHãy chọn hệ khắc chế!"
                    : "GỢI Ý: " + Hint(enemy) + "\nBạn có thể lật bài để xem rõ.";
            }
            if (resultText != null) resultText.text = finished ? "Đã hoàn thành thử thách!" : answered ? "Chọn TIẾP TỤC để sang lượt mới." : "Chọn một lá bài để phản đòn.";
            if (revealButton != null) revealButton.interactable = allowReveal && !revealed && !answered && !finished;
            if (nextButton != null)
            {
                nextButton.interactable = answered;
                var t = nextButton.GetComponentInChildren<Text>();
                if (t != null) t.text = finished ? "CHƠI LẠI" : "TIẾP TỤC";
            }
            for (int i = 0; i < 5; i++)
            {
                if (i < elementButtons.Length && elementButtons[i] != null) elementButtons[i].interactable = !answered && !finished;
                if (i < elementLabels.Length && elementLabels[i] != null) elementLabels[i].text = Names[i];
                if (i < elementImages.Length && elementImages[i] != null) elementImages[i].color = Colors[i];
            }
        }

        static string Hint(Element e)
        {
            switch (e)
            {
                case Element.Metal: return "Âm thanh kim loại vang lên.";
                case Element.Wood: return "Dây leo đang chuyển động.";
                case Element.Water: return "Có tiếng nước chảy.";
                case Element.Fire: return "Không khí trở nên nóng rực.";
                default: return "Mặt đất khẽ rung chuyển.";
            }
        }
    }
}
