using UnityEngine;

public enum PurchaseResult
{
    Success,
    InvalidCard,
    InventoryFull,
    NotEnoughMoney,
    NotEnoughGem,
    NotEnoughMoneyAndGem
}

public class StoreManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private CurrencyManager currencyManager;
    [SerializeField] private CardInventory cardInventory;

    public PurchaseResult BuyCard(CardData card)
    {
        if (card == null)
        {
            return PurchaseResult.InvalidCard;
        }

        if (currencyManager == null || cardInventory == null)
        {
            Debug.LogError("StoreManager: Chưa gán Currency Manager hoặc Card Inventory trong Inspector.");
            return PurchaseResult.InvalidCard;
        }

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

                currencyManager.SpendMoney(card.moneyCost);
                break;

            case PurchaseType.Gem:

                if (!currencyManager.HasEnoughGem(card.gemCost))
                {
                    return PurchaseResult.NotEnoughGem;
                }

                currencyManager.SpendGem(card.gemCost);
                break;

            case PurchaseType.MoneyAndGem:

                bool enoughMoney = currencyManager.HasEnoughMoney(card.moneyCost);
                bool enoughGem = currencyManager.HasEnoughGem(card.gemCost);

                if (!enoughMoney && !enoughGem)
                {
                    return PurchaseResult.NotEnoughMoneyAndGem;
                }

                if (!enoughMoney)
                {
                    return PurchaseResult.NotEnoughMoney;
                }

                if (!enoughGem)
                {
                    return PurchaseResult.NotEnoughGem;
                }

                currencyManager.SpendMoney(card.moneyCost);
                currencyManager.SpendGem(card.gemCost);
                break;
        }

        if (!cardInventory.AddCard(card))
        {
            return PurchaseResult.InventoryFull;
        }

        return PurchaseResult.Success;
    }
}