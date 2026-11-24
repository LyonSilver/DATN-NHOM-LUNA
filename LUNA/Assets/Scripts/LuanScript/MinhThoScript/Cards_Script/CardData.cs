using UnityEngine;
using UnityEngine.Serialization;

// LƯU Ý: Chỉ THÊM giá trị mới ở CUỐI mỗi enum. Không chèn giữa, không đổi thứ tự,
// vì Unity lưu enum theo số thứ tự trong asset.

public enum CardEffectType
{
    Damage,          // 0
    Shield,          // 1
    Heal,            // 2
    Poison,          // 3
    FocusTiming,     // 4
    DoubleHit,       // 5
    ChaosDraw,       // 6 
    ApplyStatus,     // 7
    DrawCard,        // 8
    Cleanse,         // 9

    ChaosRandomCard  // 10 - Chaos random thẻ cùng hệ, cùng cấp
}

public enum StatusEffectType
{
    None,    // 0
    Poison,  // 1
    Break,   // 2
    Burn,    // 3  
    Weak,    // 4  
    Stun     // 5  
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
    NotSet = 0,   // Chưa đặt: không mua được thẻ này (Shop sẽ báo đạt giới hạn)
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

[CreateAssetMenu(fileName = "CardData_New", menuName = "Chaos Card/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string cardID;
    public string cardName;
    public CardType cardType;
    public CardRarity rarity;
    public int cost;

    [Header("Phương hệ & Mức độ (Ngũ hành)")]
    [Tooltip("None = thẻ không thuộc hệ nào (Chaos, thẻ Starter cũ).")]
    public CardElement element = CardElement.None;
    [Tooltip("Normal = 1, High = 2, Extreme = 3 Energy (Cost tự đồng bộ). None = tự nhập Cost.")]
    public CardTier tier = CardTier.None;

    [Header("Hiển thị")]
    [TextArea(2, 4)]
    public string description;
    public Sprite artwork;
    public Sprite typeIcon;

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

    [Header("Giá trị phụ (Ý nghĩa thay đổi theo effectType)")]
    [Tooltip("FocusTiming: % Mở rộng vùng Timing. ChaosDraw/DrawCard: Số card rút thêm. DoubleHit: Số lần đánh.")]
    public int extraValue;

    [Header("Purchase")]
    public PurchaseType purchaseType;
    public int moneyCost;
    public int gemCost;

    [Header("Inventory - Giới hạn sở hữu")]
    [Tooltip("Số bản tối đa người chơi được sở hữu. NotSet = không mua được.")]
    [FormerlySerializedAs("maxQuantity")]
    public Quantity quantityLimit = Quantity.Max3;

    // Giữ tên cũ để CardInventory, StoreManager, StoreCardUI, InventoryCardUI
    // vẫn đọc card.maxQuantity như trước (chỉ đọc, không gán được).
    public int maxQuantity => (int)quantityLimit;

    [Header("Visual")]
    public Sprite cardSprite;

    private void OnValidate()
    {
        if (tier != CardTier.None)
        {
            cost = (int)tier;
        }
    }

    [ContextMenu("TEST - Log Element Info")]
    private void TestLogElementInfo()
    {
        Debug.Log(cardName + " | Type: " + cardType +
                  " | Hệ: " + ElementRules.GetNameVN(element) +
                  " | Tier: " + tier +
                  " | Cost: " + cost +
                  " | Giới hạn sở hữu: " + maxQuantity +
                  " | Màu: #" + ColorUtility.ToHtmlStringRGB(ElementRules.GetColor(element)));

        foreach (CardElement target in ElementRules.AllElements)
        {
            ElementRelation relation = ElementRules.GetRelation(element, target);
            Debug.Log("  Đánh Enemy hệ " + ElementRules.GetNameVN(target) + " -> " + relation);
        }
    }
}