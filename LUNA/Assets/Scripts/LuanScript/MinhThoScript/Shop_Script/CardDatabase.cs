using System.Collections.Generic;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Card System/Card Database")]
public class CardDatabase : ScriptableObject
{
    [Header("Cards Available In Store")]
    [SerializeField] private List<CardData> storeCards = new List<CardData>();

    public List<CardData> GetStoreCards()
    {
        return storeCards;
    }

    // Giới hạn sở hữu theo mức độ: Normal 5, High 3, Extreme 1
    private static int ExpectedLimit(CardTier tier)
    {
        switch (tier)
        {
            case CardTier.Normal:  return 5;
            case CardTier.High:    return 3;
            case CardTier.Extreme: return 1;
            default:               return 0;
        }
    }

    private static string JoinNames(List<CardData> list)
    {
        StringBuilder sb = new StringBuilder();

        foreach (CardData card in list)
        {
            if (sb.Length > 0) sb.Append(", ");
            sb.Append(card.name);
        }

        return sb.ToString();
    }

    // =====================================================
    // KIỂM TRA DỮ LIỆU (chạy được trong Edit Mode)
    // =====================================================

    [ContextMenu("TEST - Báo cáo dữ liệu Shop (kiểm tra đủ 90 thẻ)")]
    private void ReportData()
    {
        CardType[] types =
        {
            CardType.Attack, CardType.Defense, CardType.Poison,
            CardType.Heal, CardType.Focus, CardType.Chaos
        };

        CardTier[] tiers = { CardTier.Normal, CardTier.High, CardTier.Extreme };

        StringBuilder sb = new StringBuilder();
        int ok = 0;
        int missing = 0;
        int duplicated = 0;

        foreach (CardElement element in ElementRules.AllElements)
        {
            foreach (CardTier tier in tiers)
            {
                foreach (CardType type in types)
                {
                    List<CardData> matches = storeCards.FindAll(
                        c => c != null && c.element == element && c.tier == tier && c.cardType == type);

                    if (matches.Count == 1)
                    {
                        ok++;
                    }
                    else if (matches.Count == 0)
                    {
                        missing++;
                        sb.AppendLine("THIẾU: " + type + " / " + element + " / " + tier);
                    }
                    else
                    {
                        duplicated++;
                        sb.AppendLine("TRÙNG x" + matches.Count + ": " + type + " / " + element + " / " + tier +
                                      " -> " + JoinNames(matches));
                    }
                }
            }
        }

        int outside = 0;
        int wrongLimit = 0;
        int noPrice = 0;

        foreach (CardData card in storeCards)
        {
            if (card == null)
            {
                sb.AppendLine("Có ô trống (None) trong danh sách.");
                continue;
            }

            bool inSet = card.element != CardElement.None &&
                         card.tier != CardTier.None &&
                         System.Array.IndexOf(types, card.cardType) >= 0;

            if (!inSet)
            {
                outside++;
                sb.AppendLine("Ngoài 90 thẻ (sẽ không hiện trong Shop): " + card.name);
                continue;
            }

            if (card.maxQuantity != ExpectedLimit(card.tier))
            {
                wrongLimit++;
            }

            if (card.moneyCost <= 0 && card.gemCost <= 0)
            {
                noPrice++;
            }
        }

        sb.Insert(0, "CardDatabase: " + storeCards.Count + " thẻ trong danh sách | ĐÚNG " + ok + "/90 | THIẾU " + missing +
                     " | TRÙNG " + duplicated + " | NGOÀI 90: " + outside +
                     " | Giới hạn khác 5/3/1: " + wrongLimit + " | Giá = 0: " + noPrice + "\n");

        Debug.Log(sb.ToString());
    }

#if UNITY_EDITOR
    // =====================================================
    // CÔNG CỤ EDITOR (chỉ chạy trong Unity Editor)
    // =====================================================

    [ContextMenu("TOOL - Nạp mọi CardData có Element và Tier vào danh sách")]
    private void FillFromProject()
    {
        string[] guids = AssetDatabase.FindAssets("t:CardData");
        List<CardData> found = new List<CardData>();

        foreach (string guid in guids)
        {
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(AssetDatabase.GUIDToAssetPath(guid));

            if (card != null && card.element != CardElement.None && card.tier != CardTier.None)
            {
                found.Add(card);
            }
        }

        found.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        storeCards = found;

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log("CardDatabase: Đã nạp " + found.Count + " thẻ có Element và Tier.");
    }

    [ContextMenu("TOOL - Đặt giới hạn sở hữu theo mức độ (Normal 5 / High 3 / Extreme 1)")]
    private void ApplyTierLimits()
    {
        if (!EditorUtility.DisplayDialog("Giới hạn sở hữu",
                "Ghi đè Quantity Limit của mọi thẻ trong danh sách: Normal 5, High 3, Extreme 1.",
                "Áp dụng", "Hủy"))
        {
            return;
        }

        int count = 0;

        foreach (CardData card in storeCards)
        {
            if (card == null) continue;

            card.quantityLimit = (Quantity)ExpectedLimit(card.tier);
            EditorUtility.SetDirty(card);
            count++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("CardDatabase: Đã đặt giới hạn cho " + count + " thẻ.");
    }

    [ContextMenu("TOOL - Đặt giá theo bảng đề xuất (ghi đè Purchase Type, Money Cost, Gem Cost)")]
    private void ApplyProposedPrices()
    {
        if (!EditorUtility.DisplayDialog("Đặt giá",
                "Ghi đè Purchase Type, Money Cost, Gem Cost của mọi thẻ trong danh sách.\n" +
                "Normal 200 Coins | High 600 Coins + 10 Gems | Extreme 1500 Coins + 40 Gems | Chaos +25%.",
                "Áp dụng", "Hủy"))
        {
            return;
        }

        int count = 0;

        foreach (CardData card in storeCards)
        {
            if (card == null) continue;

            int coins;
            int gems;

            switch (card.tier)
            {
                case CardTier.Normal:  coins = 200;  gems = 0;  break;
                case CardTier.High:    coins = 600;  gems = 10; break;
                case CardTier.Extreme: coins = 1500; gems = 40; break;
                default: continue;
            }

            if (card.cardType == CardType.Chaos)
            {
                coins = Mathf.CeilToInt(coins * 1.25f / 50f) * 50;
                gems = Mathf.CeilToInt(gems * 1.25f);
            }

            card.moneyCost = coins;
            card.gemCost = gems;
            card.purchaseType = gems > 0 ? PurchaseType.MoneyAndGem : PurchaseType.Money;

            EditorUtility.SetDirty(card);
            count++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("CardDatabase: Đã đặt giá cho " + count + " thẻ.");
    }
#endif
}