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

public enum StatusEffectType
{
    None,
    Poison,
    Break
}

[CreateAssetMenu(fileName = "CardData_New", menuName = "Chaos Card/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string cardID;
    public string cardName;
    public CardType cardType;
    public int cost;

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

    [Header("Giá trị phụ (Ý nghĩa thay đổi theo effectType)")]
    [Tooltip("FocusTiming: % Mở rộng vùng Timing. ChaosDraw: Số card rút thêm. DoubleHit: Số lần đánh.")]
    public int extraValue;
}