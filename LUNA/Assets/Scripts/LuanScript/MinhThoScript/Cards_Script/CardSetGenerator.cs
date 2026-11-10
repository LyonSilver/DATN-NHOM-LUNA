
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CardSetGenerator
{
    private const string OutputFolder =
        "Assets/ScriptableObjects/Card_Generated";

    // =====================================================
    // TEMPLATE
    // =====================================================

    private class CardTemplate
    {
        public CardType type;
        public CardTier tier;
        public string name;
        public CardEffectType effect;

        public int damage;
        public int shield;
        public int heal;
        public int extra;
        public StatusEffectType status;
        public int statusDuration;

        public CardTemplate(
            CardType type,
            CardTier tier,
            string name,
            CardEffectType effect,
            int damage = 0,
            int shield = 0,
            int heal = 0,
            int extra = 0,
            StatusEffectType status = StatusEffectType.None,
            int statusDuration = 0)
        {
            this.type = type;
            this.tier = tier;
            this.name = name;
            this.effect = effect;
            this.damage = damage;
            this.shield = shield;
            this.heal = heal;
            this.extra = extra;
            this.status = status;
            this.statusDuration = statusDuration;
        }
    }

    // =====================================================
    // 18 TEMPLATE: 6 NHÓM X 3 CẤP
    // =====================================================

    private static readonly CardTemplate[] Templates =
    {
        // ATTACK
        new CardTemplate(
            CardType.Attack, CardTier.Normal,
            "Strike", CardEffectType.Damage, damage: 8),

        new CardTemplate(
            CardType.Attack, CardTier.High,
            "Heavy Impact", CardEffectType.Damage, damage: 15),

        new CardTemplate(
            CardType.Attack, CardTier.Extreme,
            "Cataclysm", CardEffectType.Damage, damage: 25),

        // DEFENSE
        new CardTemplate(
            CardType.Defense, CardTier.Normal,
            "Guard", CardEffectType.Shield, shield: 10),

        new CardTemplate(
            CardType.Defense, CardTier.High,
            "Iron Fortress", CardEffectType.Shield, shield: 18),

        new CardTemplate(
            CardType.Defense, CardTier.Extreme,
            "Aegis Dominion", CardEffectType.Shield, shield: 30),

        // HEAL
        new CardTemplate(
            CardType.Heal, CardTier.Normal,
            "Gentle Recovery", CardEffectType.Heal, heal: 8),

        new CardTemplate(
            CardType.Heal, CardTier.High,
            "Vital Renewal", CardEffectType.Heal, heal: 15),

        new CardTemplate(
            CardType.Heal, CardTier.Extreme,
            "Divine Restoration", CardEffectType.Heal, heal: 24),

        // POISON
        new CardTemplate(
            CardType.Poison, CardTier.Normal,
            "Venom Dart", CardEffectType.Poison,
            damage: 4,
            status: StatusEffectType.Poison,
            statusDuration: 2),

        new CardTemplate(
            CardType.Poison, CardTier.High,
            "Toxic Burst", CardEffectType.Poison,
            damage: 8,
            status: StatusEffectType.Poison,
            statusDuration: 3),

        new CardTemplate(
            CardType.Poison, CardTier.Extreme,
            "Plague Nova", CardEffectType.Poison,
            damage: 12,
            status: StatusEffectType.Poison,
            statusDuration: 4),

        // FOCUS
        new CardTemplate(
            CardType.Focus, CardTier.Normal,
            "Steady Mind", CardEffectType.FocusTiming,
            extra: 15),

        new CardTemplate(
            CardType.Focus, CardTier.High,
            "Perfect Rhythm", CardEffectType.FocusTiming,
            extra: 30),

        new CardTemplate(
            CardType.Focus, CardTier.Extreme,
            "Absolute Focus", CardEffectType.FocusTiming,
            extra: 50),

        // CHAOS
        new CardTemplate(
            CardType.Chaos, CardTier.Normal,
            "Chaos Spark", CardEffectType.ChaosRandomCard),

        new CardTemplate(
            CardType.Chaos, CardTier.High,
            "Chaos Surge", CardEffectType.ChaosRandomCard),

        new CardTemplate(
            CardType.Chaos, CardTier.Extreme,
            "Chaos Ascension", CardEffectType.ChaosRandomCard)
    };

    // =====================================================
    // ELEMENT PREFIX
    // =====================================================

    private static string GetElementPrefix(CardElement element)
    {
        switch (element)
        {
            case CardElement.Fire:  return "Ember";
            case CardElement.Water: return "Tidal";
            case CardElement.Metal: return "Iron";
            case CardElement.Wood:  return "Verdant";
            case CardElement.Earth: return "Terra";

            default: return "Unknown";
        }
    }

    // =====================================================
    // ELEMENTAL RARITY
    // =====================================================

    private static CardRarity GetRarity(CardTier tier)
    {
        switch (tier)
        {
            case CardTier.High:
                return CardRarity.Rare;

            case CardTier.Extreme:
                return CardRarity.Epic;

            default:
                return CardRarity.Common;
        }
    }

    // =====================================================
    // CARD SPEC
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

    // =====================================================
    // BUILD 90 CARDS
    // =====================================================

    private static List<CardSpec> BuildAllSpecs()
    {
        List<CardSpec> list = new List<CardSpec>();

        foreach (CardTemplate template in Templates)
        {
            foreach (CardElement element in ElementRules.AllElements)
            {
                CardSpec spec = new CardSpec();

                spec.type = template.type;
                spec.element = element;
                spec.tier = template.tier;

                spec.rarity = GetRarity(template.tier);

                string prefix = GetElementPrefix(element);

                spec.cardName = prefix + " " + template.name;

                spec.cardID =
                    (template.type + "_" + element + "_" +
                    template.tier).ToLower();

                spec.assetName =
                    "CardData_" + template.type + "_" +
                    element + "_" + template.tier;

                spec.effect = template.effect;

                spec.damage = template.damage;
                spec.shield = template.shield;
                spec.heal = template.heal;
                spec.extra = template.extra;

                spec.status = template.status;
                spec.statusDuration = template.statusDuration;

                spec.description = BuildDescription(spec);

                list.Add(spec);
            }
        }

        return list;
    }

    // =====================================================
    // DESCRIPTION
    // =====================================================

    private static string BuildDescription(CardSpec s)
    {
        string elementName = ElementRules.GetName(s.element);

        switch (s.type)
        {
            case CardType.Attack:
                return "Deal " + s.damage + " " +
                       elementName + " damage.";

            case CardType.Defense:
                return "Gain " + s.shield + " Shield.";

            case CardType.Heal:
                return "Restore " + s.heal + " HP.";

            case CardType.Poison:
                return "Deal " + s.damage + " initial damage.\n" +
                       "Apply Poison for " + s.statusDuration +
                       (s.statusDuration == 1 ? " turn." : " turns.");

            case CardType.Focus:
                return "Expand the Timing zone by " +
                       s.extra + "%.";

            case CardType.Chaos:
                return "Randomly transform into an Attack, Defense, " +
                       "Heal, Poison or Focus card of the same element " +
                       "and tier.";

            default:
                return "";
        }
    }

    // =====================================================
    // MENU 1: PREVIEW
    // =====================================================

    [MenuItem("Chaos Card/Card Set/Log 90 Cards")]
    public static void LogTable()
    {
        List<CardSpec> specs = BuildAllSpecs();

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("CARD SET GENERATOR - 90 CARDS");

        foreach (CardSpec s in specs)
        {
            sb.AppendLine(
                s.assetName +
                " | " + s.cardName +
                " | " + s.type +
                " | " + s.element +
                " | " + s.tier +
                " | Cost " + (int)s.tier +
                " | " + s.effect +
                " | Damage " + s.damage +
                " | Shield " + s.shield +
                " | Heal " + s.heal +
                " | Extra " + s.extra +
                " | Status " + s.status +
                " | Duration " + s.statusDuration +
                " | " + s.description.Replace("\n", " ")
            );
        }

        Debug.Log(sb.ToString());

        ValidateSpecs(specs);
    }

    // =====================================================
    // MENU 2: GENERATE OR UPDATE
    // =====================================================

    [MenuItem("Chaos Card/Card Set/Generate or Update 90 Cards")]
    public static void Generate()
    {
        List<CardSpec> specs = BuildAllSpecs();

        if (!ValidateSpecs(specs))
        {
            Debug.LogError(
                "CardSetGenerator: Invalid card data. Generation cancelled.");

            return;
        }

        bool confirmed = EditorUtility.DisplayDialog(
            "Generate 90 Cards",
            "Create or update " + specs.Count +
            " CardData assets in:\n" + OutputFolder +
            "\n\nArtwork, inventory and shop fields will be preserved.",
            "Generate",
            "Cancel"
        );

        if (!confirmed)
            return;

        EnsureFolder(OutputFolder);

        int created = 0;
        int updated = 0;

        foreach (CardSpec s in specs)
        {
            string path = OutputFolder + "/" + s.assetName + ".asset";

            CardData data =
                AssetDatabase.LoadAssetAtPath<CardData>(path);

            bool isNew = data == null;

            if (isNew)
            {
                data = ScriptableObject.CreateInstance<CardData>();

                AssetDatabase.CreateAsset(data, path);
            }

            Apply(data, s);

            EditorUtility.SetDirty(data);

            if (isNew)
                created++;
            else
                updated++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Object folder =
            AssetDatabase.LoadAssetAtPath<Object>(OutputFolder);

        if (folder != null)
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = folder;
        }

        Debug.Log(
            "CardSetGenerator COMPLETE | Created: " + created +
            " | Updated: " + updated +
            " | Total: " + specs.Count
        );
    }

    // =====================================================
    // APPLY DATA
    // =====================================================

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

        // Không ghi đè:
        // artwork
        // typeIcon
        // cardSprite
        // baseValue
        // purchaseType
        // moneyCost
        // gemCost
        // quantityLimit
    }

    // =====================================================
    // VALIDATION
    // =====================================================

    private static bool ValidateSpecs(List<CardSpec> specs)
    {
        bool ok = true;

        if (specs.Count != 90)
        {
            Debug.LogError(
                "Expected 90 cards, got " + specs.Count);

            ok = false;
        }

        HashSet<string> ids = new HashSet<string>();

        foreach (CardSpec s in specs)
        {
            if (!ids.Add(s.cardID))
            {
                Debug.LogError("Duplicate Card ID: " + s.cardID);
                ok = false;
            }

            if (s.tier == CardTier.None)
            {
                Debug.LogError("Invalid Tier: " + s.cardID);
                ok = false;
            }

            if (s.type == CardType.Chaos &&
                s.effect != CardEffectType.ChaosRandomCard)
            {
                Debug.LogError("Invalid Chaos effect: " + s.cardID);
                ok = false;
            }

            if (s.type != CardType.Chaos &&
                s.effect == CardEffectType.ChaosRandomCard)
            {
                Debug.LogError("Non-Chaos card uses Chaos effect: " + s.cardID);
                ok = false;
            }

            if (s.type == CardType.Poison &&
                s.status != StatusEffectType.Poison)
            {
                Debug.LogError("Invalid Poison status: " + s.cardID);
                ok = false;
            }
        }

        if (ok)
        {
            Debug.Log(
                "CardSetGenerator: Valid 90 cards. No duplicate IDs.");
        }

        return ok;
    }

    // =====================================================
    // ENSURE FOLDER
    // =====================================================

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
