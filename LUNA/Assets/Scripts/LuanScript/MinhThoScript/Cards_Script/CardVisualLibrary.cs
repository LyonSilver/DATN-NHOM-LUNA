using UnityEngine;

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

[CreateAssetMenu(fileName = "CardVisualLibrary_Main", menuName = "Chaos Card/Card Visual Library")]
public class CardVisualLibrary : ScriptableObject
{
    [Header("Art gốc: 4 yếu tố x 3 mức độ = 12 sprite")]
    public CardArtSet attackArt;
    public CardArtSet defenseArt;
    public CardArtSet supportArt;
    public CardArtSet effectArt;

    [Header("Icon loại thẻ (nên là hình trắng/xám để nhuộm theo màu phương hệ)")]
    public Sprite attackIcon;
    public Sprite defenseIcon;
    public Sprite supportIcon;
    public Sprite effectIcon;
    public Sprite chaosIcon;

    [Header("Material đổi màu theo phương hệ (shader UI/CardElementTint)")]
    public Material fireMaterial;
    public Material waterMaterial;
    public Material metalMaterial;
    public Material woodMaterial;
    public Material earthMaterial;

    public Sprite GetArt(CardType type, CardTier tier)
    {
        switch (type)
        {
            case CardType.Attack:  return attackArt != null ? attackArt.Get(tier) : null;
            case CardType.Defense: return defenseArt != null ? defenseArt.Get(tier) : null;
            case CardType.Support: return supportArt != null ? supportArt.Get(tier) : null;
            case CardType.Effect:  return effectArt != null ? effectArt.Get(tier) : null;
            default:               return null;
        }
    }

    public Sprite GetTypeIcon(CardType type)
    {
        switch (type)
        {
            case CardType.Attack:  return attackIcon;
            case CardType.Defense: return defenseIcon;
            case CardType.Support: return supportIcon;
            case CardType.Effect:  return effectIcon;
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

    [ContextMenu("TEST - Kiểm tra thư viện")]
    private void TestValidate()
    {
        int missing = 0;

        foreach (CardType type in new[] { CardType.Attack, CardType.Defense, CardType.Support, CardType.Effect })
        {
            foreach (CardTier tier in new[] { CardTier.Normal, CardTier.High, CardTier.Extreme })
            {
                if (GetArt(type, tier) == null)
                {
                    Debug.LogWarning("CardVisualLibrary: Thiếu art " + type + " / " + tier);
                    missing++;
                }
            }
        }

        foreach (CardElement element in ElementRules.AllElements)
        {
            if (GetElementMaterial(element) == null)
            {
                Debug.LogWarning("CardVisualLibrary: Thiếu Material " + element);
                missing++;
            }
        }

        if (missing == 0)
        {
            Debug.Log("CardVisualLibrary: Đủ 12 art và 5 Material.");
        }
        else
        {
            Debug.LogWarning("CardVisualLibrary: Còn thiếu " + missing + " mục.");
        }
    }
}