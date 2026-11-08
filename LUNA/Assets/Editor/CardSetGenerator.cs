using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CardSetGenerator
{
    // =====================================================
    // CẤU HÌNH: nếu thư mục ScriptableObjects của bạn khác, sửa dòng này
    // =====================================================

    private const string OutputFolder = "Assets/_Project/ScriptableObjects/Cards/Generated";

    // =====================================================
    // BẢNG 12 MẪU THẺ: sửa số ở đây rồi chạy lại "Generate or Update"
    // =====================================================

    private class CardTemplate
    {
        public readonly CardType type;
        public readonly CardTier tier;
        public readonly string name;
        public readonly CardEffectType effect;
        public readonly int damage;
        public readonly int shield;
        public readonly int heal;
        public readonly int extra;

        public CardTemplate(CardType type, CardTier tier, string name, CardEffectType effect,
            int damage = 0, int shield = 0, int heal = 0, int extra = 0)
        {
            this.type = type;
            this.tier = tier;
            this.name = name;
            this.effect = effect;
            this.damage = damage;
            this.shield = shield;
            this.heal = heal;
            this.extra = extra;
        }
    }

    private static readonly CardTemplate[] Templates =
    {
        // ATTACK
        new CardTemplate(CardType.Attack, CardTier.Normal,  "Strike",      CardEffectType.Damage, damage: 8),
        new CardTemplate(CardType.Attack, CardTier.High,    "Fierce Blow", CardEffectType.Damage, damage: 16),
        new CardTemplate(CardType.Attack, CardTier.Extreme, "Annihilate",  CardEffectType.Damage, damage: 27),

        // DEFENSE
        new CardTemplate(CardType.Defense, CardTier.Normal,  "Block",    CardEffectType.Shield, shield: 10),
        new CardTemplate(CardType.Defense, CardTier.High,    "Bulwark",  CardEffectType.Shield, shield: 20),
        new CardTemplate(CardType.Defense, CardTier.Extreme, "Fortress", CardEffectType.Shield, shield: 32),

        // SUPPORT
        new CardTemplate(CardType.Support, CardTier.Normal,  "Aid",      CardEffectType.Heal,     heal: 6),
        new CardTemplate(CardType.Support, CardTier.High,    "Blessing", CardEffectType.DrawCard, extra: 1),
        new CardTemplate(CardType.Support, CardTier.Extreme, "Miracle",  CardEffectType.Heal,     heal: 25),

        // EFFECT (damage nhỏ + trạng thái theo phương hệ)
        new CardTemplate(CardType.Effect, CardTier.Normal,  "Afflict",  CardEffectType.ApplyStatus, damage: 3),
        new CardTemplate(CardType.Effect, CardTier.High,    "Torment",  CardEffectType.ApplyStatus, damage: 6),
        new CardTemplate(CardType.Effect, CardTier.Extreme, "Calamity", CardEffectType.ApplyStatus, damage: 10)
    };

    private static StatusEffectType GetStatusForElement(CardElement element)
    {
        switch (element)
        {
            case CardElement.Fire:  return StatusEffectType.Burn;
            case CardElement.Water: return StatusEffectType.Weak;
            case CardElement.Metal: return StatusEffectType.Break;
            case CardElement.Wood:  return StatusEffectType.Poison;
            case CardElement.Earth: return StatusEffectType.Stun;
            default:                return StatusEffectType.None;
        }
    }

    // Break và Stun ngắn hơn: 1/1/2 lượt. Các trạng thái còn lại: 2/3/4 lượt.
    private static int GetStatusDuration(StatusEffectType status, CardTier tier)
    {
        int t = (int)tier;

        if (status == StatusEffectType.Break || status == StatusEffectType.Stun)
        {
            return t >= 3 ? 2 : 1;
        }

        return t + 1;
    }

    private static CardRarity GetRarity(CardTier tier)
    {
        switch (tier)
        {
            case CardTier.High:    return CardRarity.Rare;
            case CardTier.Extreme: return CardRarity.Epic;
            default:               return CardRarity.Common;
        }
    }

    // =====================================================
    // DỮ LIỆU ĐÃ TÍNH TOÁN CHO TỪNG THẺ (dùng chung cho Log và Generate)
    // =====================================================

    private class CardSpec
    {
        public string assetName;
        public string cardID;
        public string cardName;
        public string description;
        public CardType type;
        public CardElement element;
        public CardTier tier;
        public CardRarity rarity;
        public CardEffectType effect;
        public int damage;
        public int shield;
        public int heal;
        public int extra;
        public StatusEffectType status;
        public int statusDuration;
    }

    private static List<CardSpec> BuildAllSpecs()
    {
        List<CardSpec> list = new List<CardSpec>();

        foreach (CardTemplate t in Templates)
        {
            foreach (CardElement element in ElementRules.AllElements)
            {
                CardSpec s = new CardSpec();

                s.type = t.type;
                s.element = element;
                s.tier = t.tier;
                s.rarity = GetRarity(t.tier);
                s.cardName = t.name;
                s.cardID = (t.type + "_" + element + "_" + t.tier).ToLower();
                s.assetName = "CardData_" + t.type + "_" + element + "_" + t.tier;

                s.effect = t.effect;
                s.damage = t.damage;
                s.shield = t.shield;
                s.heal = t.heal;
                s.extra = t.extra;

                if (t.effect == CardEffectType.ApplyStatus)
                {
                    s.status = GetStatusForElement(element);
                    s.statusDuration = GetStatusDuration(s.status, t.tier);
                }
                else
                {
                    s.status = StatusEffectType.None;
                    s.statusDuration = 0;
                }

                s.description = BuildDescription(s);

                list.Add(s);
            }
        }

        return list;
    }

    private static string BuildDescription(CardSpec s)
    {
        string elementName = ElementRules.GetName(s.element);

        switch (s.effect)
        {
            case CardEffectType.Damage:
                return "Deal " + s.damage + " " + elementName + " damage.\n" +
                       "Strong vs " + ElementRules.GetName(ElementRules.GetOvercomes(s.element)) + ".";

            case CardEffectType.Shield:
                return "Gain " + s.shield + " Shield.";

            case CardEffectType.Heal:
                return "Heal " + s.heal + " HP.";

            case CardEffectType.DrawCard:
                return "Draw " + s.extra + (s.extra == 1 ? " card." : " cards.");

            case CardEffectType.ApplyStatus:
                return "Deal " + s.damage + " damage.\n" +
                       "Apply " + s.status + " for " + s.statusDuration +
                       (s.statusDuration == 1 ? " turn." : " turns.");

            default:
                return "";
        }
    }

    // =====================================================
    // MENU 1: XEM TRƯỚC (không tạo gì)
    // =====================================================

    [MenuItem("Chaos Card/Card Set/Log 60 Cards (xem truoc, khong tao gi)")]
    public static void LogTable()
    {
        List<CardSpec> specs = BuildAllSpecs();
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("CardSetGenerator: Bảng " + specs.Count + " thẻ (chưa tạo asset nào)");

        for (int i = 0; i < specs.Count; i++)
        {
            CardSpec s = specs[i];

            sb.AppendLine(
                (i + 1).ToString("00") + ". " + s.assetName +
                " | " + s.cardName +
                " | Cost " + (int)s.tier +
                " | " + s.rarity +
                " | " + s.effect +
                " | Dmg " + s.damage +
                " Shield " + s.shield +
                " Heal " + s.heal +
                " Extra " + s.extra +
                " | " + s.status + " " + s.statusDuration +
                " | " + s.description.Replace("\n", " "));
        }

        Debug.Log(sb.ToString());
        ValidateSpecs(specs);
    }

    // =====================================================
    // MENU 2: SINH HOẶC CẬP NHẬT 60 ASSET
    // =====================================================

    [MenuItem("Chaos Card/Card Set/Generate or Update 60 Cards")]
    public static void Generate()
    {
        List<CardSpec> specs = BuildAllSpecs();

        if (!ValidateSpecs(specs))
        {
            Debug.LogError("CardSetGenerator: Bảng dữ liệu có lỗi, dừng lại. Xem các dòng lỗi phía trên.");
            return;
        }

        bool confirmed = EditorUtility.DisplayDialog(
            "Generate 60 Cards",
            "Sẽ tạo hoặc cập nhật " + specs.Count + " CardData trong:\n" + OutputFolder +
            "\n\nArtwork và các field Shop không bị đụng tới.",
            "Generate",
            "Hủy");

        if (!confirmed) return;

        EnsureFolder(OutputFolder);

        int created = 0;
        int updated = 0;

        foreach (CardSpec s in specs)
        {
            string path = OutputFolder + "/" + s.assetName + ".asset";
            CardData data = AssetDatabase.LoadAssetAtPath<CardData>(path);
            bool isNew = data == null;

            if (isNew)
            {
                data = ScriptableObject.CreateInstance<CardData>();
                AssetDatabase.CreateAsset(data, path);
            }

            Apply(data, s);
            EditorUtility.SetDirty(data);

            if (isNew) created++;
            else updated++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Object folder = AssetDatabase.LoadAssetAtPath<Object>(OutputFolder);
        if (folder != null)
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = folder;
        }

        Debug.Log("CardSetGenerator: Xong. Tạo mới " + created + ", cập nhật " + updated +
                  ", tổng " + specs.Count + " thẻ tại " + OutputFolder);
    }

    // Chỉ ghi các field thuộc "thiết kế thẻ".
    // KHÔNG đụng artwork, typeIcon, cardSprite, baseValue và các field Shop.
    private static void Apply(CardData d, CardSpec s)
    {
        d.cardID = s.cardID;
        d.cardName = s.cardName;
        d.cardType = s.type;
        d.rarity = s.rarity;

        d.element = s.element;
        d.tier = s.tier;
        d.cost = (int)s.tier;

        d.description = s.description;

        d.effectType = s.effect;
        d.baseDamage = s.damage;
        d.shieldValue = s.shield;
        d.healValue = s.heal;
        d.extraValue = s.extra;

        d.statusEffect = s.status;
        d.statusDuration = s.statusDuration;
    }

    // =====================================================
    // KIỂM TRA BẢNG
    // =====================================================

    private static bool ValidateSpecs(List<CardSpec> specs)
    {
        bool ok = true;

        if (specs.Count != 60)
        {
            Debug.LogError("CardSetGenerator: Cần đúng 60 thẻ nhưng bảng đang có " + specs.Count + ".");
            ok = false;
        }

        HashSet<string> ids = new HashSet<string>();

        foreach (CardSpec s in specs)
        {
            if (!ids.Add(s.cardID))
            {
                Debug.LogError("CardSetGenerator: Trùng cardID " + s.cardID);
                ok = false;
            }
        }

        if (ok)
        {
            Debug.Log("CardSetGenerator: Bảng hợp lệ (60 thẻ, không trùng ID).");
        }

        return ok;
    }

    private static void EnsureFolder(string folderPath)
    {
        string[] parts = folderPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}