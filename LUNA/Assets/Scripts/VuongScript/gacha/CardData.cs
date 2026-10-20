using UnityEngine;

public enum CardType
{
    Attack,
    Guard,
    Effect,
    Support,
    Chaos
}

public enum CardRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public enum PurchaseType
{
    Money,
    Energy,
    MoneyAndEnergy
}

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

public enum StatusEffectType
{
    None,
    Poison,
    Break
}

[CreateAssetMenu(fileName = "CardData_New", menuName = "Card System/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string cardID;
    public string cardName;
    public CardType cardType;
    public CardRarity rarity;
    public int cost;

    [TextArea(2, 4)]
    public string description;

    [Header("Hiển thị")]
    public Sprite artwork;
    public Sprite typeIcon;
    public Sprite cardSprite;

    [Header("Hiệu ứng chính")]
    public CardEffectType effectType;
    public int baseDamage;
    public int shieldValue;
    public int healValue;
    public int baseValue;

    [Header("Trạng thái")]
    public StatusEffectType statusEffect;
    public int statusDuration;

    [Tooltip("FocusTiming: % mở rộng vùng Timing. ChaosDraw: số card rút thêm. DoubleHit: số lần đánh.")]
    public int extraValue;

    [Header("Mua / Inventory")]
    public PurchaseType purchaseType;
    public int moneyCost;
    public int energyCost;
    public int maxQuantity = 1;
}