using System.Text;
using UnityEngine;

// Art cho 1 hệ: 3 mức độ
[System.Serializable]
public class CardArtSet
{
    public Sprite normal;
    public Sprite high;
    public Sprite extreme;

    public Sprite Get(CardTier tier)
    {
        switch (tier)
        {
            case CardTier.Normal:  return normal;
            case CardTier.High:    return high;
            case CardTier.Extreme: return extreme;
            default:               return null;
        }
    }
}

// Art cho 1 nhóm thẻ: art chung (3) + art riêng từng hệ (5 x 3)
// Tên normal/high/extreme của art chung GIỐNG bản cũ để giữ dữ liệu đã gán.
[System.Serializable]
public class CardGroupArt
{
    [Header("Art chung của nhóm (Dùng khi hệ chưa có art riêng)")]
    public Sprite normal;
    public Sprite high;
    public Sprite extreme;

    [Header("Art riêng từng hệ (Không bắt buộc, ô trống sẽ dùng art chung)")]
    public CardArtSet earth;
    public CardArtSet fire;
    public CardArtSet metal;
    public CardArtSet water;
    public CardArtSet wood;

    public Sprite GetShared(CardTier tier)
    {
        switch (tier)
        {
            case CardTier.Normal:  return normal;
            case CardTier.High:    return high;
            case CardTier.Extreme: return extreme;
            default:               return null;
        }
    }

    public Sprite GetOwn(CardElement element, CardTier tier)
    {
        CardArtSet set = GetElementSet(element);
        return set != null ? set.Get(tier) : null;
    }

    // Ưu tiên art riêng của hệ
    public Sprite Get(CardElement element, CardTier tier)
    {
        Sprite own = GetOwn(element, tier);
        return own != null ? own : GetShared(tier);
    }

    private CardArtSet GetElementSet(CardElement element)
    {
        switch (element)
        {
            case CardElement.Earth: return earth;
            case CardElement.Fire:  return fire;
            case CardElement.Metal: return metal;
            case CardElement.Water: return water;
            case CardElement.Wood:  return wood;
            default:                return null;
        }
    }
}

[CreateAssetMenu(fileName = "CardVisualLibrary_Main", menuName = "Chaos Card/Card Visual Library")]
public class CardVisualLibrary : ScriptableObject
{
    [Header("6 nhóm thẻ: Mỗi nhóm 5 hệ x 3 mức độ")]
    public CardGroupArt attackArt;
    public CardGroupArt defenseArt;
    public CardGroupArt healArt;
    public CardGroupArt poisonArt;
    public CardGroupArt focusArt;
    public CardGroupArt chaosArt;

    [Header("Icon loại thẻ")]
    public Sprite attackIcon;
    public Sprite defenseIcon;
    public Sprite healIcon;
    public Sprite poisonIcon;
    public Sprite focusIcon;
    public Sprite chaosIcon;

    [Header("Material đổi màu theo phương hệ (shader UI/CardElementTint)")]
    public Material fireMaterial;
    public Material waterMaterial;
    public Material metalMaterial;
    public Material woodMaterial;
    public Material earthMaterial;

    private static readonly CardType[] NewGroups =
    {
        CardType.Attack, CardType.Defense, CardType.Heal,
        CardType.Poison, CardType.Focus, CardType.Chaos
    };

    private static readonly CardTier[] AllTiers =
    {
        CardTier.Normal, CardTier.High, CardTier.Extreme
    };

    private CardGroupArt GetGroup(CardType type)
    {
        switch (type)
        {
            case CardType.Attack:  return attackArt;
            case CardType.Defense: return defenseArt;
            case CardType.Heal:    return healArt;
            case CardType.Poison:  return poisonArt;
            case CardType.Focus:   return focusArt;
            case CardType.Chaos:   return chaosArt;
            default:               return null;
        }
    }

    // Art theo (Nhóm, hệ, mức độ). Ưu tiên art riêng của hệ.
    public Sprite GetArt(CardType type, CardElement element, CardTier tier)
    {
        CardGroupArt group = GetGroup(type);
        return group != null ? group.Get(element, tier) : null;
    }

    // Giữ chữ ký cũ để script khác (nếu có) vẫn biên dịch được. Chỉ trả art chung.
    public Sprite GetArt(CardType type, CardTier tier)
    {
        return GetArt(type, CardElement.None, tier);
    }

    public Sprite GetTypeIcon(CardType type)
    {
        switch (type)
        {
            case CardType.Attack:  return attackIcon;
            case CardType.Defense: return defenseIcon;
            case CardType.Heal:    return healIcon;
            case CardType.Poison:  return poisonIcon;
            case CardType.Focus:   return focusIcon;
            case CardType.Chaos:   return chaosIcon;
            default:               return null;
        }
    }

    public Material GetElementMaterial(CardElement element)
    {
        switch (element)
        {
            case CardElement.Fire:  return fireMaterial;
            case CardElement.Water: return waterMaterial;
            case CardElement.Metal: return metalMaterial;
            case CardElement.Wood:  return woodMaterial;
            case CardElement.Earth: return earthMaterial;
            default:                return null;
        }
    }

    [ContextMenu("TEST - Báo cáo thư viện (6 nhóm x 5 hệ x 3 mức độ)")]
    private void TestReport()
    {
        StringBuilder sb = new StringBuilder("CardVisualLibrary: Báo cáo art\n");

        int total = 0;
        int ownTotal = 0;
        int sharedTotal = 0;
        int missingTotal = 0;

        foreach (CardType type in NewGroups)
        {
            CardGroupArt group = GetGroup(type);

            int own = 0;
            int shared = 0;
            int missing = 0;

            foreach (CardElement element in ElementRules.AllElements)
            {
                foreach (CardTier tier in AllTiers)
                {
                    total++;

                    if (group != null && group.GetOwn(element, tier) != null)
                    {
                        own++;
                    }
                    else if (group != null && group.GetShared(tier) != null)
                    {
                        shared++;
                    }
                    else
                    {
                        missing++;
                    }
                }
            }

            ownTotal += own;
            sharedTotal += shared;
            missingTotal += missing;

            sb.AppendLine(type + ": art riêng " + own + "/15 | dùng art chung " + shared +
                          "/15 | THIẾU " + missing + "/15");
        }

        sb.AppendLine("TỔNG " + total + " ô: riêng " + ownTotal + " | chung " + sharedTotal +
                      " | THIẾU " + missingTotal);

        foreach (CardElement element in ElementRules.AllElements)
        {
            if (GetElementMaterial(element) == null)
            {
                sb.AppendLine("THIẾU Material " + element);
            }
        }

        foreach (CardType type in NewGroups)
        {
            if (GetTypeIcon(type) == null)
            {
                sb.AppendLine("(Tùy chọn) Chưa có icon cho " + type);
            }
        }

        Debug.Log(sb.ToString());
    }
}