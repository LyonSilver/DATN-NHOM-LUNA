
using UnityEngine;

// =========================
// LOẠI HIỆU ỨNG CARD
// =========================

public enum CardEffectType
{
    Damage,
    Shield,
    Heal,
    Poison,
    FocusTiming,
    DoubleHit,
    ChaosDraw
}

// =========================
// HIỆU ỨNG TRẠNG THÁI
// =========================

public enum StatusEffectType
{
    None,
    Poison,
    Break
}

// =========================
// ĐỘ HIẾM CARD
// =========================

public enum CardRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

// =========================
// LOẠI THANH TOÁN
// =========================

public enum PurchaseType
{
    Money,
    Energy,
    MoneyAndEnergy
}

[CreateAssetMenu(
    fileName = "CardData_New",
    menuName = "Chaos Card/Card Data"
)]
public class CardData : ScriptableObject
{
    // =========================
    // THÔNG TIN CƠ BẢN
    // =========================

    [Header("Thông tin cơ bản")]

    public string cardID;
    public string cardName;

    public CardType cardType;
    public CardRarity rarity;

    [Min(0)]
    public int cost;

    // =========================
    // HIỂN THỊ CARD
    // =========================

    [Header("Hiển thị")]

    [TextArea(2, 4)]
    public string description;

    public Sprite artwork;
    public Sprite typeIcon;

    // Tương thích với GachaUIManager.
    // Sử dụng chung Sprite với artwork.
    public Sprite cardSprite
    {
        get => artwork;
        set => artwork = value;
    }

    // =========================
    // HIỆU ỨNG CHÍNH
    // =========================

    [Header("Hiệu ứng chính")]

    public CardEffectType effectType;

    [Min(0)]
    public int baseDamage;

    [Min(0)]
    public int shieldValue;

    [Min(0)]
    public int healValue;

    // =========================
    // HIỆU ỨNG TRẠNG THÁI
    // =========================

    [Header("Trạng thái áp lên đối tượng")]

    public StatusEffectType statusEffect;

    [Min(0)]
    public int statusDuration;

    // =========================
    // GIÁ TRỊ PHỤ
    // =========================

    [Header("Giá trị phụ")]

    [Tooltip(
        "FocusTiming: % mở rộng vùng Timing. " +
        "ChaosDraw: Số Card rút thêm. " +
        "DoubleHit: Số lần đánh."
    )]
    public int extraValue;

    // =========================
    // HỆ THỐNG MUA CARD
    // =========================

    [Header("Thông tin mua Card")]

    public PurchaseType purchaseType;

    [Min(0)]
    public int moneyCost;

    [Min(0)]
    public int energyCost;

    // =========================
    // GIỚI HẠN MUA CARD
    // =========================

    [Header("Giới hạn mua Card")]

    [Tooltip(
        "Số lần tối đa có thể mua Card trong một trận"
    )]
    [Min(1)]
    public int maxQuantity = 1;
}
