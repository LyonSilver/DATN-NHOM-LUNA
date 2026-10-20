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
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Button buyButton;

    [Header("Trạng thái card")]
    [SerializeField] private CanvasGroup cardCanvasGroup;

    private CardData cardData;
    private StoreUI storeUI;
    private StoreManager storeManager;
    private CardInventory cardInventory;

    public void Setup(
        CardData card,
        StoreUI store,
        StoreManager manager,
        CardInventory inventory)
    {
        cardData = card;
        storeUI = store;
        storeManager = manager;
        cardInventory = inventory;

        cardNameText.text = card.cardName;
        cardTypeText.text = card.cardType.ToString();
    
        rarityText.text = card.rarity.ToString();

        if (cardCanvasGroup == null)
        {
            cardCanvasGroup = GetComponent<CanvasGroup>();

            if (cardCanvasGroup == null)
            {
                cardCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        UpdatePrice();

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(BuyCard);

        UpdateCardState();
    }

    private void UpdatePrice()
    {
        switch (cardData.purchaseType)
        {
            case PurchaseType.Money:

                priceText.text = "Money: " + cardData.moneyCost;
                break;

            case PurchaseType.Energy:

                priceText.text = "Gem: " + cardData.energyCost;
                break;

            case PurchaseType.MoneyAndEnergy:

                priceText.text =
                    "Money: " + cardData.moneyCost +
                    "\nGem: " + cardData.energyCost;
                break;
        }
    }

    public void UpdateCardState()
    {
        if (cardData == null ||
            storeManager == null ||
            cardInventory == null)
        {
            return;
        }

        int purchasedCount = storeManager.GetPurchasedCount(cardData);

        int ownedCount = cardInventory.GetCardQuantity(cardData);

        quantityText.text =
            "Bought: " + purchasedCount +
            " / " + cardData.maxQuantity +
            "\nOwned: " + ownedCount;

           bool canBuy = storeManager.CanBuyMoreInMatch(cardData);
        // bool canBuy =
        // storeManager.CanBuyMoreInMatch(cardData) &&
        // cardInventory.CanAddCard(cardData);

        // Card mờ khi hết giới hạn mua hoặc Inventory đã đầy.
        cardCanvasGroup.alpha = canBuy ? 1f : 0.4f;

        // Chặn người chơi nhấn nút Buy.
        buyButton.interactable = canBuy;
    }

    private void BuyCard()
    {
        if (cardData == null || storeUI == null)
        {
            return;
        }

        storeUI.BuyCard(cardData);
    }
}