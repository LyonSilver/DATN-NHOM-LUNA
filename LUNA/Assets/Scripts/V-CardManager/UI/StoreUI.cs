using TMPro;
using UnityEngine;

public class StoreUI : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private CardDatabase cardDatabase;

    [Header("Managers")]
    [SerializeField] private StoreManager storeManager;
    [SerializeField] private CurrencyManager currencyManager;

    [Header("Store")]
    [SerializeField] private Transform storePanel;
    [SerializeField] private GameObject storeCardPrefab;

    [Header("Currency UI")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text energyText;

    [Header("Inventory")]
    [SerializeField] private CardInventory cardInventory;

    [Header("Popup")]
    [SerializeField] private PurchasePopup purchasePopup;

    private void Start()
    {
        RefreshStore();
        UpdateCurrencyUI();
    }

    public void RefreshStore()
    {
        ClearStore();

        if (cardDatabase == null)
        {
            Debug.LogError("StoreUI: CardDatabase chưa được gán.");
            return;
        }

        for (int i = 0; i < cardDatabase.GetStoreCards().Count; i++)
        {
            CardData card =
                cardDatabase.GetStoreCards()[i];

            GameObject cardObject =
                Instantiate(storeCardPrefab, storePanel);

            StoreCardUI cardUI =
                cardObject.GetComponent<StoreCardUI>();

            cardUI.Setup(card, this);

            cardUI.UpdateQuantity(
                card,
                cardInventory
            );
        }
    }

    private void ClearStore()
    {
        if (storePanel == null)
        {
            return;
        }

        for (int i = storePanel.childCount - 1; i >= 0; i--)
        {
            Destroy(storePanel.GetChild(i).gameObject);
        }
    }

    public void BuyCard(CardData card)
    {
        PurchaseResult result =
            storeManager.BuyCard(card);

        switch (result)
        {
            case PurchaseResult.Success:

                int currentQuantity =
                    cardInventory.GetCardQuantity(card);

                purchasePopup.Show(
                    "MUA THÀNH CÔNG",
                    "Đã mua: " + card.cardName +
                    "\nSố lượng: " +
                    currentQuantity +
                    " / " +
                    card.maxQuantity
                );

                UpdateCurrencyUI();

                RefreshStore();

                break;


            case PurchaseResult.InvalidCard:

                purchasePopup.Show(
                    "LỖI",
                    "Card không hợp lệ."
                );

                break;


            case PurchaseResult.InventoryFull:

                currentQuantity =
                    cardInventory.GetCardQuantity(card);

                purchasePopup.Show(
                    "KHÔNG THỂ MUA",
                    card.cardName +
                    " đã đạt giới hạn." +
                    "\nSố lượng: " +
                    currentQuantity +
                    " / " +
                    card.maxQuantity
                );

                break;


            case PurchaseResult.NotEnoughMoney:

                purchasePopup.Show(
                    "KHÔNG ĐỦ MONEY",
                    "Bạn không đủ Money để mua " +
                    card.cardName
                );

                break;


            case PurchaseResult.NotEnoughEnergy:

                purchasePopup.Show(
                    "KHÔNG ĐỦ GEM",
                    "Bạn không đủ Gem để mua " +
                    card.cardName
                );

                break;


            case PurchaseResult.NotEnoughMoneyAndEnergy:

                purchasePopup.Show(
                    "KHÔNG ĐỦ TÀI NGUYÊN",
                    "Bạn không đủ Money và Gem để mua " +
                    card.cardName
                );

                break;
        }
    }
    public void UpdateCurrencyUI()
    {
        if (currencyManager == null)
        {
            return;
        }

        moneyText.text =
            "Money: " + currencyManager.GetMoney();

        energyText.text =
            "Gem: " + currencyManager.GetEnergy();
    }
}