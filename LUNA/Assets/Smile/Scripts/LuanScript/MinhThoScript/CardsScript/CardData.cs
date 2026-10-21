using UnityEngine;

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

// Hiệu ứng trạng thái
public enum StatusEffectType
{
    None,
    Poison,
    Break
}

// Độ hiếm của lá bài trong Shop
public enum CardRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

// Loại chi phí mua bài trong Shop
public enum PurchaseType
{
    Money,
    Energy,
    MoneyAndEnergy
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Card System/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Thông tin lá bài")]
    public string cardID;
    public string cardName;
    public CardType cardType;

    [TextArea(2, 5)]
    public string description;

    [Header("Hình ảnh")]
    public Sprite artwork;
    public Sprite typeIcon;
    public Sprite cardSprite;

    [Header("Chiến đấu")]
    [Min(0)]
    public int cost;

    public CardEffectType effectType;

    [Min(0)]
    public int baseDamage;

    [Min(0)]
    public int shieldValue;

    [Min(0)]
    public int healValue;

    public StatusEffectType statusEffect;

    [Min(0)]
    public int statusDuration;

    public int extraValue;

    [Header("Shop")]
    public CardRarity rarity;
    public PurchaseType purchaseType;

    [Min(0)]
    public int moneyCost;

    [Min(0)]
    public int energyCost;

    [Min(0)]
    public int baseValue;

    [Min(0)]
    public int maxQuantity;
}