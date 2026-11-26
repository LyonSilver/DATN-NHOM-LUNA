
using UnityEngine;
using UnityEngine.Serialization;

// Chỉ thêm giá trị mới ở cuối mỗi enum.
// Không thay đổi thứ tự để tránh lỗi dữ liệu Asset.

public enum CardEffectType
{
    Damage,       // 0
    Shield,       // 1
    Heal,         // 2
    Poison,       // 3
    FocusTiming,  // 4 - Giữ để tương thích dữ liệu cũ
    DoubleHit,    // 5
    ChaosDraw,    // 6
    ApplyStatus,  // 7
    DrawCard,     // 8
    Cleanse       // 9
}

public enum StatusEffectType
{
    None,
    Poison,
    Break,
    Burn,
    Weak,
    Stun
}

public enum CardRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public enum Quantity
{
    NotSet = 0,
    Max1 = 1,
    Max2 = 2,
    Max3 = 3,
    Max4 = 4,
    Max5 = 5
}

public enum PurchaseType
{
    Money,
    Gem,
    MoneyAndGem
}

[CreateAssetMenu(
    fileName = "CardData_New",
    menuName = "Chaos Card/Card Data"
)]
public class CardData : ScriptableObject
{
    // =====================================
    // VFX ENUM
    // =====================================

    public enum VFXTarget
    {
        Player,
        Enemy
    }

    public enum VFXDurationType
    {
        Timed,
        UntilShieldBroken
    }

    // =====================================
    // BASIC INFORMATION
    // =====================================

    [Header("Thông tin cơ bản")]
    public string cardID;
    public string cardName;
    public CardType cardType;
    public CardRarity rarity;
    public int cost;

    // =====================================
    // ELEMENT & TIER
    // =====================================

    [Header("Phương hệ & Mức độ (Ngũ hành)")]
    [Tooltip("None = thẻ không thuộc hệ nào.")]
    public CardElement element = CardElement.None;

    [Tooltip("Normal = 1, High = 2, Extreme = 3 Energy. None = tự nhập Cost.")]
    public CardTier tier = CardTier.None;

    // =====================================
    // DISPLAY
    // =====================================

    [Header("Hiển thị")]
    [TextArea(2, 4)]
    public string description;

    public Sprite artwork;
    public Sprite typeIcon;

    // =====================================
    // CARD EFFECT
    // =====================================

    [Header("Hiệu ứng chính")]
    public CardEffectType effectType;
    public int baseDamage;
    public int shieldValue;
    public int healValue;

    [Header("Trạng thái áp lên đối tượng (Nếu có)")]
    public StatusEffectType statusEffect;
    public int statusDuration;

    [Header("Giá trị thực")]
    public int baseValue;

    [Header("Giá trị phụ")]
    [Tooltip("ChaosDraw/DrawCard: Số card rút thêm. DoubleHit: Số lần đánh.")]
    public int extraValue;

    // =====================================
    // SKILL VFX
    // =====================================

    [Header("Skill VFX")]

    [Tooltip("Prefab hiệu ứng khi sử dụng thẻ.")]
    public GameObject vfxPrefab;

    [Tooltip("Đối tượng xuất hiện hiệu ứng.")]
    public VFXTarget vfxTarget = VFXTarget.Enemy;

    [Tooltip("Timed: Hủy theo thời gian. UntilShieldBroken: Hủy khi Shield bằng 0.")]
    public VFXDurationType vfxDurationType =
        VFXDurationType.Timed;

    [Tooltip("Vị trí hiệu ứng so với mục tiêu.")]
    public Vector3 vfxOffset = new Vector3(0f, 1f, 0f);

    // =====================================
    // VFX SCALE - MỚI
    // =====================================

    [Tooltip("Kích thước hiệu ứng VFX theo trục X, Y, Z.")]
    public Vector3 vfxScale = Vector3.one;

    [Tooltip("Thời gian tồn tại. Chỉ sử dụng với chế độ Timed.")]
    [Min(0.1f)]
    public float vfxLifetime = 1.5f;

    // =====================================
    // VFX REPEAT
    // =====================================

    [Header("VFX Repeat")]

    [Tooltip("Số lần xuất hiện hiệu ứng. Chỉ sử dụng với Timed.")]
    [Min(1)]
    public int vfxRepeatCount = 1;

    [Tooltip("Khoảng cách thời gian giữa các lần xuất hiện.")]
    [Min(0f)]
    public float vfxRepeatDelay = 0.2f;

    // =====================================
    // PURCHASE
    // =====================================

    [Header("Purchase")]
    public PurchaseType purchaseType;
    public int moneyCost;
    public int gemCost;

    // =====================================
    // INVENTORY
    // =====================================

    [Header("Inventory - Giới hạn sở hữu")]
    [Tooltip("Số bản tối đa người chơi được sở hữu. NotSet = không mua được.")]
    [FormerlySerializedAs("maxQuantity")]
    public Quantity quantityLimit = Quantity.Max3;

    public int maxQuantity => (int)quantityLimit;

    // =====================================
    // VISUAL
    // =====================================

    [Header("Visual")]
    public Sprite cardSprite;

    // =====================================
    // VALIDATE
    // =====================================

    private void OnValidate()
    {
        if (tier != CardTier.None)
        {
            cost = (int)tier;
        }

        vfxLifetime = Mathf.Max(0.1f, vfxLifetime);
        vfxRepeatCount = Mathf.Max(1, vfxRepeatCount);
        vfxRepeatDelay = Mathf.Max(0f, vfxRepeatDelay);

        // VFX Shield luôn xuất hiện trên Player.
        if (vfxDurationType ==
            VFXDurationType.UntilShieldBroken)
        {
            vfxTarget = VFXTarget.Player;
        }
    }

    // =====================================
    // DEBUG ELEMENT
    // =====================================

    [ContextMenu("TEST - Log Element Info")]
    private void TestLogElementInfo()
    {
        Debug.Log(
            cardName +
            " | Type: " + cardType +
            " | Hệ: " + ElementRules.GetNameVN(element) +
            " | Tier: " + tier +
            " | Cost: " + cost +
            " | Giới hạn sở hữu: " + maxQuantity +
            " | Màu: #" +
            ColorUtility.ToHtmlStringRGB(
                ElementRules.GetColor(element)
            )
        );

        foreach (CardElement target in ElementRules.AllElements)
        {
            ElementRelation relation =
                ElementRules.GetRelation(element, target);

            Debug.Log(
                "Đánh Enemy hệ " +
                ElementRules.GetNameVN(target) +
                " -> " + relation
            );
        }
    }
}
