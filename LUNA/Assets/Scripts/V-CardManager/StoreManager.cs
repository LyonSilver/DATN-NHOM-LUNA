using UnityEngine;

public enum PurchaseResult
{
    Success,
    InvalidCard,
    InventoryFull,
    NotEnoughMoney,
    NotEnoughEnergy,
    NotEnoughMoneyAndEnergy
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


            case PurchaseType.Energy:

                if (!currencyManager.HasEnoughEnergy(card.energyCost))
                {
                    return PurchaseResult.NotEnoughEnergy;
                }

                currencyManager.SpendEnergy(card.energyCost);

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

                currencyManager.SpendMoney(card.moneyCost);
                currencyManager.SpendEnergy(card.energyCost);

                break;
        }

        bool added = cardInventory.AddCard(card);

        if (!added)
        {
            return PurchaseResult.InventoryFull;
        }

        return PurchaseResult.Success;
    }
}