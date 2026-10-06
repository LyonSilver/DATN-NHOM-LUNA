using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreCardUI : MonoBehaviour
{
    [Header("Card UI")]
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardTypeText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    private CardData cardData;
    private StoreUI storeUI;

    public void Setup(CardData card, StoreUI store)
    {
        cardData = card;
        storeUI = store;

        cardNameText.text = card.cardName;
        cardTypeText.text = card.cardType.ToString();
        rarityText.text = card.rarity.ToString();

        UpdatePrice();

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(BuyCard);
    }

    private void UpdatePrice()
    {
        switch (cardData.purchaseType)
        {
            case PurchaseType.Money:

                priceText.text =
                    "Money: " + cardData.moneyCost;

                break;

            case PurchaseType.Energy:

                priceText.text =
                    "Gem: " + cardData.energyCost;

                break;

            case PurchaseType.MoneyAndEnergy:

                priceText.text =
                    "Money: " + cardData.moneyCost +
                    "\nGem: " + cardData.energyCost;

                break;
        }
    }

    private void BuyCard()
    {
        storeUI.BuyCard(cardData);
    }
}