using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Các loại hiệu ứng Troll trong trò chơi
/// </summary>
public enum TrollEffectType
{
    UnoReverse,      // Phản đòn 200% sát thương + cướp bài
    HandSwap,        // Hoán đổi bài trên tay
    GreedTax,        // Phạt HP mỗi khi dùng bài
    ImpostorBomb,    // Cài bài bom thối kẹt hand & nổ HP cuối lượt
    ForcedDilemma    // Ép đối thủ bỏ bài hoặc bị skip turn
}

/// <summary>
/// Thời điểm kích hoạt hiệu ứng bài Troll
/// </summary>
public enum TriggerTiming
{
    Instant,           // Kích hoạt ngay khi đánh ra
    OnOpponentAttack,  // Bẫy kích hoạt khi đối thủ tấn công
    EndTurn            // Kích hoạt khi kết thúc lượt
}

/// <summary>
/// Dữ liệu nguyên mẫu cho 1 lá bài
/// </summary>
[System.Serializable]
public class CardData
{
    public string id;
    public string cardName;
    public string description;
    public int manaCost;
    public bool isTrollBomb;
    public TriggerTiming timing;
    public TrollEffectType effectType;

    public CardData(string id, string cardName, string description, int manaCost, TriggerTiming timing, TrollEffectType effectType)
    {
        this.id = id;
        this.cardName = cardName;
        this.description = description;
        this.manaCost = manaCost;
        this.timing = timing;
        this.effectType = effectType;
        this.isTrollBomb = false;
    }
}

/// <summary>
/// Quản lý trạng thái người chơi
/// </summary>
public class CardGamePlayer
{
    public string PlayerName { get; private set; }
    public int Health { get; set; }
    public int Mana { get; set; }
    public List<CardData> Hand { get; private set; } = new List<CardData>();
    public List<CardData> ActiveTraps { get; private set; } = new List<CardData>();
    public bool IsSkipTurnNext { get; set; } = false;
    public bool IsTaxed { get; set; } = false;

    public CardGamePlayer(string name, int health, int mana)
    {
        PlayerName = name;
        Health = health;
        Mana = mana;
    }

    public void TakeDamage(int damage)
    {
        Health = Mathf.Max(0, Health - damage);
    }
}

public class TrollCardSystem : MonoBehaviour
{
    public CardGamePlayer p1; // Yugi (Bạn)
    public CardGamePlayer p2; // Kaiba (Đối thủ)
    public int currentTurn = 1; // 1 = P1, 2 = P2

    private Vector2 scrollLogPos;
    private List<string> battleLogs = new List<string>();

    private void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        p1 = new CardGamePlayer("Yugi (Bạn)", 30, 10);
        p2 = new CardGamePlayer("Kaiba (Đối thủ)", 30, 10);
        currentTurn = 1;
        battleLogs.Clear();

        // Nạp bộ bài Troll cho Yugi
        p1.Hand.Add(new CardData("T01", "Uno Reverse", "Bẫy phản 200% sát thương & cướp 1 bài", 2, TriggerTiming.OnOpponentAttack, TrollEffectType.UnoReverse));
        p1.Hand.Add(new CardData("T02", "Mind Swap", "Tráo đổi toàn bộ bài trên tay với đối thủ", 4, TriggerTiming.Instant, TrollEffectType.HandSwap));
        p1.Hand.Add(new CardData("T03", "Thuế Lòng Tham", "Phạt 5 LP khi đánh bài ở lượt sau", 3, TriggerTiming.Instant, TrollEffectType.GreedTax));
        p1.Hand.Add(new CardData("T04", "Cài Bom Thối", "Nhét 2 Bom Thối vào tay đối thủ", 3, TriggerTiming.Instant, TrollEffectType.ImpostorBomb));
        p1.Hand.Add(new CardData("T05", "Bẫy Nghiệt Ngã", "Ép bỏ 3 lá bài hoặc bị MẤT LƯỢT", 5, TriggerTiming.Instant, TrollEffectType.ForcedDilemma));

        // Nạp bài thường cho Kaiba
        for (int i = 1; i <= 3; i++)
        {
            p2.Hand.Add(new CardData($"NOR_{i}", $"Rồng Trắng #{i}", "Quái thú bình thường", 3, TriggerTiming.Instant, TrollEffectType.GreedTax));
        }

        AddLog("🎮 Game đã khởi tạo! Hãy nhấn các nút bên dưới để test thủ công.");
    }

    public void AddLog(string log)
    {
        battleLogs.Add($"[{DateTime.Now:HH:mm:ss}] {log}");
        Debug.Log(log);
    }

    public void PlayCard(CardGamePlayer caster, CardGamePlayer target, int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= caster.Hand.Count) return;

        CardData card = caster.Hand[cardIndex];

        if (card.isTrollBomb)
        {
            AddLog($"❌ {caster.PlayerName} không thể đánh lá [{card.cardName}]! Đây là bài bom thối!");
            return;
        }

        if (caster.IsTaxed)
        {
            caster.TakeDamage(5);
            target.Health = Mathf.Min(30, target.Health + 5);
            AddLog($"💸 [THUẾ LÒNG THAM] {caster.PlayerName} mất 5 LP khi dùng [{card.cardName}] (Hồi 5 LP cho {target.PlayerName})!");
        }
        else
        {
            if (caster.Mana < card.manaCost)
            {
                AddLog($"❌ Không đủ Mana ({caster.Mana}/{card.manaCost}) để đánh [{card.cardName}]!");
                return;
            }
            caster.Mana -= card.manaCost;
        }

        caster.Hand.RemoveAt(cardIndex);

        if (card.timing == TriggerTiming.OnOpponentAttack)
        {
            caster.ActiveTraps.Add(card);
            AddLog($"🃏 {caster.PlayerName} đã ÚP BẪY BÍ MẬT [{card.cardName}]!");
        }
        else
        {
            ExecuteEffect(card, caster, target);
        }
    }

    private void ExecuteEffect(CardData card, CardGamePlayer caster, CardGamePlayer target)
    {
        AddLog($"✨ {caster.PlayerName} kích hoạt lá [{card.cardName}]!");

        switch (card.effectType)
        {
            case TrollEffectType.HandSwap:
                List<CardData> temp = new List<CardData>(caster.Hand);
                caster.Hand.Clear();
                caster.Hand.AddRange(target.Hand);
                target.Hand.Clear();
                target.Hand.AddRange(temp);
                AddLog($"🔄 [TROLL] Hoán đổi toàn bộ bài trên tay giữa 2 người chơi!");
                break;

            case TrollEffectType.GreedTax:
                target.IsTaxed = true;
                AddLog($"💸 [TROLL] Yểm nguyền {target.PlayerName}! Lượt tới đánh bài mất 5 LP!");
                break;

            case TrollEffectType.ImpostorBomb:
                CardData bomb = new CardData("BOMB", "💣 Bom Thối", "Trừ 3 LP cuối lượt", 99, TriggerTiming.EndTurn, TrollEffectType.ImpostorBomb)
                {
                    isTrollBomb = true
                };
                target.Hand.Add(bomb);
                target.Hand.Add(bomb);
                AddLog($"💩 [TROLL] Nhét 2 lá [Bom Thối] vào tay {target.PlayerName}!");
                break;

            case TrollEffectType.ForcedDilemma:
                if (target.Hand.Count >= 3)
                {
                    for (int i = 0; i < 3 && target.Hand.Count > 0; i++)
                    {
                        target.Hand.RemoveAt(UnityEngine.Random.Range(0, target.Hand.Count));
                    }
                    AddLog($"🔥 {target.PlayerName} bị hủy 3 lá bài trên tay để né Skip Turn!");
                }
                else
                {
                    target.IsSkipTurnNext = true;
                    AddLog($"💤 {target.PlayerName} không đủ bài -> BỊ MẤT LƯỢT KẾ TIẾP!");
                }
                break;
        }
    }

    public void TriggerKaibaAttack()
    {
        AddLog($"⚔️ Kaiba tấn công Yugi với 10 Sát thương!");

        int trapIdx = p1.ActiveTraps.FindIndex(t => t.effectType == TrollEffectType.UnoReverse);
        if (trapIdx != -1)
        {
            p1.ActiveTraps.RemoveAt(trapIdx);
            int reflected = 20;
            p2.TakeDamage(reflected);
            AddLog($"🤡 [UNO REVERSE KÍCH HOẠT!] Yugi phản {reflected} LP vào mặt Kaiba!");

            if (p2.Hand.Count > 0)
            {
                int stolenIdx = UnityEngine.Random.Range(0, p2.Hand.Count);
                CardData stolen = p2.Hand[stolenIdx];
                p2.Hand.RemoveAt(stolenIdx);
                p1.Hand.Add(stolen);
                AddLog($"😈 Yugi cướp luôn lá [{stolen.cardName}] từ tay Kaiba!");
            }
        }
        else
        {
            p1.TakeDamage(10);
            AddLog($"💥 Yugi nhận 10 sát thương. Máu còn: {p1.Health}");
        }
    }

    public void EndTurn()
    {
        CardGamePlayer active = currentTurn == 1 ? p1 : p2;

        // Kiểm tra Bom Thối
        int bombCount = active.Hand.FindAll(c => c.isTrollBomb).Count;
        if (bombCount > 0)
        {
            int dmg = bombCount * 3;
            active.TakeDamage(dmg);
            AddLog($"💣 [CUỐI LƯỢT] {active.PlayerName} bị nổ {bombCount} lá Bom Thối, mất {dmg} LP!");
        }

        active.IsTaxed = false;
        p1.Mana = 10;
        p2.Mana = 10;

        currentTurn = currentTurn == 1 ? 2 : 1;
        AddLog($"▶ Đã chuyển sang lượt: {(currentTurn == 1 ? p1.PlayerName : p2.PlayerName)}");
    }

    private void OnGUI()
    {
        // Khung hiển thị trực quan OnGUI
        GUI.Box(new Rect(10, 10, 480, 580), "🤡 TROLL CARD TESTER PANEL");

        // Trạng thái Player 1 & 2
        GUI.Label(new Rect(25, 35, 450, 25), $"🧙‍♂️ <b>Yugi (Bạn):</b> HP: {p1.Health}/30 | MP: {p1.Mana}/10 | Bẫy úp: {p1.ActiveTraps.Count}");
        GUI.Label(new Rect(25, 55, 450, 25), $"🤖 <b>Kaiba (Đ.Thủ):</b> HP: {p2.Health}/30 | MP: {p2.Mana}/10 | Bài trên tay: {p2.Hand.Count}");

        GUI.Label(new Rect(25, 80, 450, 20), $"👉 <b>ĐANG TRONG LƯỢT CỦA:</b> {(currentTurn == 1 ? "YUGI" : "KAIBA")}");

        // Nút điều khiển chung
        if (GUI.Button(new Rect(25, 105, 140, 30), "⚔️ Kaiba Tấn Công"))
        {
            TriggerKaibaAttack();
        }

        if (GUI.Button(new Rect(175, 105, 130, 30), "⌛ Kết Thúc Lượt"))
        {
            EndTurn();
        }

        if (GUI.Button(new Rect(315, 105, 130, 30), "🔄 Reset Game"))
        {
            InitializeGame();
        }

        // Danh sách bài trên tay Yugi để BẤM ĐÁNH THỦ CÔNG
        GUI.Label(new Rect(25, 145, 450, 20), "<b>🃏 BÀI TRÊN TAY YUGI (Bấm nút bên dưới để ĐÁNH / ÚP BẪY):</b>");

        for (int i = 0; i < p1.Hand.Count; i++)
        {
            CardData card = p1.Hand[i];
            string btnText = $"[{card.manaCost} MP] {card.cardName} - {card.description}";
            
            if (GUI.Button(new Rect(25, 170 + (i * 32), 440, 28), btnText))
            {
                PlayCard(p1, p2, i);
            }
        }

        // Khung Log chi tiết
        int logY = 170 + (Mathf.Max(p1.Hand.Count, 1) * 32) + 10;
        GUI.Label(new Rect(25, logY, 400, 20), "<b>📜 NHẬT KÝ HÀNH ĐỘNG (BATTLE LOG):</b>");

        scrollLogPos = GUI.BeginScrollView(
            new Rect(25, logY + 25, 440, 560 - (logY + 30)),
            scrollLogPos,
            new Rect(0, 0, 420, battleLogs.Count * 22)
        );

        for (int i = 0; i < battleLogs.Count; i++)
        {
            GUI.Label(new Rect(5, i * 22, 410, 20), battleLogs[i]);
        }

        GUI.EndScrollView();
    }
}