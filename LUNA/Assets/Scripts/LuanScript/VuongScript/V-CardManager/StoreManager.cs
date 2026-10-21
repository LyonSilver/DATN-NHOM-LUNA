using System.Collections.Generic;
using UnityEngine;

public enum PurchaseResult
{
    Success,
    InvalidCard,
    InventoryFull,
    NotEnoughMoney,
    NotEnoughEnergy,
    NotEnoughMoneyAndEnergy,
    CardPurchaseLimitReached
}

public class StoreManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private CurrencyManager currencyManager;
    [SerializeField] private CardInventory cardInventory;

    // Lưu số lần đã mua mỗi card trong trận hiện tại.
    private Dictionary<CardData, int> purchasedCards =
        new Dictionary<CardData, int>();

    public int GetPurchasedCount(CardData card)
    {
        if (card == null)
        {
            return 0;
        }

        if (purchasedCards.ContainsKey(card))
        {
            return purchasedCards[card];
        }

        return 0;
    }

    public bool CanBuyMoreInMatch(CardData card)
    {
        if (card == null)
        {
            return false;
        }

        return GetPurchasedCount(card) < card.maxQuantity;
    }

    public PurchaseResult BuyCard(CardData card)
    {
        if (card == null)
        {
            return PurchaseResult.InvalidCard;
        }

        // Kiểm tra giới hạn mua trong cả trận.
        if (!CanBuyMoreInMatch(card))
        {
            return PurchaseResult.CardPurchaseLimitReached;
        }

        // Kiểm tra giới hạn sở hữu trong Inventory.
        if (!cardInventory.CanAddCard(card))
        {
            return PurchaseResult.InventoryFull;
        }

        switch (card.purchaseType)
        {
            case PurchaseType.Money:

                if (!currencyManager.HasEnoughMoney(card.moneyCost))
                {
                    return PurchaseResult.NotEnoughMoney;
                }

                break;

            case PurchaseType.Energy:

                if (!currencyManager.HasEnoughEnergy(card.energyCost))
                {
                    return PurchaseResult.NotEnoughEnergy;
                }

                break;

            case PurchaseType.MoneyAndEnergy:

                bool enoughMoney =
                    currencyManager.HasEnoughMoney(card.moneyCost);

                bool enoughEnergy =
                    currencyManager.HasEnoughEnergy(card.energyCost);

                if (!enoughMoney && !enoughEnergy)
                {
                    return PurchaseResult.NotEnoughMoneyAndEnergy;
                }

                if (!enoughMoney)
                {
                    return PurchaseResult.NotEnoughMoney;
                }

                if (!enoughEnergy)
                {
                    return PurchaseResult.NotEnoughEnergy;
                }

                break;
        }

        // Thêm card vào Inventory trước khi trừ tài nguyên.
        bool added = cardInventory.AddCard(card);

        if (!added)
        {
            return PurchaseResult.InventoryFull;
        }

        // Trừ tài nguyên sau khi thêm card thành công.
        switch (card.purchaseType)
        {
            case PurchaseType.Money:
                currencyManager.SpendMoney(card.moneyCost);
                break;

            case PurchaseType.Energy:
                currencyManager.SpendEnergy(card.energyCost);
                break;

            case PurchaseType.MoneyAndEnergy:
                currencyManager.SpendMoney(card.moneyCost);
                currencyManager.SpendEnergy(card.energyCost);
                break;
        }

        // Chỉ ghi nhận lượt mua sau khi giao dịch thành công.
        int purchasedCount = GetPurchasedCount(card);
        purchasedCards[card] = purchasedCount + 1;

        return PurchaseResult.Success;
    }
    public void ResetPurchaseLimits()
    {
        purchasedCards.Clear();
    }
}