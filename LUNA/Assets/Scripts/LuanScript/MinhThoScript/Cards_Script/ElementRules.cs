using UnityEngine;

public enum ElementRelation
{
    Neutral,
    Advantage,
    Disadvantage
}

public static class ElementRules
{
    public static readonly CardElement[] AllElements =
    {
        CardElement.Fire,
        CardElement.Water,
        CardElement.Metal,
        CardElement.Wood,
        CardElement.Earth
    };

    public static Color GetColor(CardElement element)
    {
        switch (element)
        {
            case CardElement.Fire:  return new Color32(220, 70, 50, 255);
            case CardElement.Water: return new Color32(50, 130, 220, 255);
            case CardElement.Metal: return new Color32(235, 195, 60, 255);
            case CardElement.Wood:  return new Color32(80, 175, 80, 255);
            case CardElement.Earth: return new Color32(150, 105, 65, 255);
            default:                return new Color32(200, 200, 200, 255);
        }
    }

    // Tên dùng trên UI (tiếng Anh, tránh lỗi font thiếu dấu tiếng Việt)
    public static string GetName(CardElement element)
    {
        switch (element)
        {
            case CardElement.Fire:  return "Fire";
            case CardElement.Water: return "Water";
            case CardElement.Metal: return "Metal";
            case CardElement.Wood:  return "Wood";
            case CardElement.Earth: return "Earth";
            default:                return "None";
        }
    }

    // Tên tiếng Việt, chỉ dùng cho Debug.Log
    public static string GetNameVN(CardElement element)
    {
        switch (element)
        {
            case CardElement.Fire:  return "Hỏa";
            case CardElement.Water: return "Thủy";
            case CardElement.Metal: return "Kim";
            case CardElement.Wood:  return "Mộc";
            case CardElement.Earth: return "Thổ";
            default:                return "Không hệ";
        }
    }

    // Ngũ hành tương khắc: Kim > Mộc > Thổ > Thủy > Hỏa > Kim
    public static CardElement GetOvercomes(CardElement element)
    {
        switch (element)
        {
            case CardElement.Metal: return CardElement.Wood;
            case CardElement.Wood:  return CardElement.Earth;
            case CardElement.Earth: return CardElement.Water;
            case CardElement.Water: return CardElement.Fire;
            case CardElement.Fire:  return CardElement.Metal;
            default:                return CardElement.None;
        }
    }

    // attacker = hệ của thẻ, defender = hệ của Enemy
    public static ElementRelation GetRelation(CardElement attacker, CardElement defender)
    {
        if (attacker == CardElement.None || defender == CardElement.None)
        {
            return ElementRelation.Neutral;
        }

        if (GetOvercomes(attacker) == defender)
        {
            return ElementRelation.Advantage;
        }

        if (GetOvercomes(defender) == attacker)
        {
            return ElementRelation.Disadvantage;
        }

        return ElementRelation.Neutral;
    }
}