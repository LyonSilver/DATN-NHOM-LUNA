using System.Collections.Generic;
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

    [Header("Settings")]
    [SerializeField] private int cardsPerRefresh = 4;

    [Header("Currency UI")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text energyText;

    [Header("Inventory")]
    [SerializeField] private CardInventory cardInventory;

    [Header("Popup")]
    [SerializeField] private PurchasePopup purchasePopup;

    // Danh sách card đang hiển thị trong Shop.
    private List<CardData> currentStoreCards =
        new List<CardData>();


    private void OnEnable()
    {
        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged += UpdateCurrencyUI;
        }
    }

    private void OnDisable()
    {
        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged -= UpdateCurrencyUI;
        }
    }
    private void Start()
    {
        RefreshStore();
        UpdateCurrencyUI();
    }

    // Chọn ngẫu nhiên các card mới.
    // Hàm này được gọi lúc mở Shop và khi nhấn Reset.
    public void RefreshStore()
    {
        if (cardDatabase == null)
        {
            Debug.LogError("StoreUI: CardDatabase chưa được gán.");
            return;
        }

        if (storePanel == null || storeCardPrefab == null)
        {
            Debug.LogError("StoreUI: Chưa gán Store Panel hoặc Card Prefab.");
            return;
        }

        List<CardData> availableCards =
            new List<CardData>();

        List<CardData> databaseCards =
            cardDatabase.GetStoreCards();

        for (int i = 0; i < databaseCards.Count; i++)
        {
            CardData card = databaseCards[i];

            if (card != null && !availableCards.Contains(card))
            {
                availableCards.Add(card);
            }
        }

        // Xóa danh sách card đang hiển thị cũ.
        ClearStore();
        currentStoreCards.Clear();

        int targetCount =
            Mathf.Min(cardsPerRefresh, availableCards.Count);

        // Chọn ngẫu nhiên, không chọn trùng card.
        for (int i = 0; i < targetCount; i++)
        {
            int randomIndex =
                Random.Range(0, availableCards.Count);

            CardData selectedCard =
                availableCards[randomIndex];

            currentStoreCards.Add(selectedCard);
            availableCards.RemoveAt(randomIndex);
        }

        // Tạo UI cho các card được chọn.
        for (int i = 0; i < currentStoreCards.Count; i++)
        {
            CardData card = currentStoreCards[i];

            GameObject cardObject =
                Instantiate(storeCardPrefab, storePanel);

            StoreCardUI cardUI =
                cardObject.GetComponent<StoreCardUI>();

            if (cardUI == null)
            {
                Debug.LogError(
                    "StoreUI: Prefab thiếu StoreCardUI."
                );

                Destroy(cardObject);
                continue;
            }

            cardUI.Setup(
                card,
                this,
                storeManager,
                cardInventory
            );
        }

        UpdateVisibleCardStates();
    }

    // Xóa các card UI cũ nhưng không xóa dữ liệu mua trong trận.
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

    // Cập nhật trạng thái card mà không chọn lại card.
    private void UpdateVisibleCardStates()
    {
        if (storePanel == null)
        {
            return;
        }

        for (int i = 0; i < storePanel.childCount; i++)
        {
            StoreCardUI cardUI =
                storePanel.GetChild(i).GetComponent<StoreCardUI>();

            if (cardUI != null)
            {
                cardUI.UpdateCardState();
            }
        }
    }

    public void BuyCard(CardData card)
    {
        PurchaseResult result =
            storeManager.BuyCard(card);

        switch (result)
        {
            case PurchaseResult.Success:
                {
                    int currentQuantity =
                        cardInventory.GetCardQuantity(card);

                    int purchasedCount =
                        storeManager.GetPurchasedCount(card);

                    purchasePopup.ShowSuccess(
                        card.cardName,
                        card.description,     // Se gan mo ta sau khi kiem tra CardData
                        null,   // Se gan anh sau khi kiem tra CardData
                        purchasedCount,
                        card.maxQuantity,
                        currentQuantity
                    );

                    UpdateCurrencyUI();
                    UpdateVisibleCardStates();
                    break;
                }

            case PurchaseResult.InvalidCard:
                purchasePopup.ShowFailure(
                    "THẺ BÀI KHÔNG HỢP LỆ: " + card.cardName
                );
                break;

            case PurchaseResult.InventoryFull:
                purchasePopup.ShowFailure(
                    card.cardName +
                    " ĐÃ ĐẠT SỐ LƯỢNG TỐI ĐA TRONG KHO!"
                );

                UpdateVisibleCardStates();
                break;

            case PurchaseResult.CardPurchaseLimitReached:
                purchasePopup.ShowFailure(
                    "ĐÃ ĐẠT GIỚI HẠN MUA CỦA THẺ " + card.cardName
                );

                UpdateVisibleCardStates();
                break;

            case PurchaseResult.NotEnoughMoney:
                purchasePopup.ShowFailure(
                    "BẠN KHÔNG TIỀN ĐỂ MUA " +
                    card.cardName + "!"
                );
                break;

            case PurchaseResult.NotEnoughEnergy:
                purchasePopup.ShowFailure(
                    "BẠN KHÔNG CÓ ĐỦ GEM ĐỂ MUA " +
                    card.cardName + "!"
                );
                break;

            case PurchaseResult.NotEnoughMoneyAndEnergy:
                purchasePopup.ShowFailure(
                    "BẠN KHÔNG CÓ ĐỦ TIỀN VÀ GEM ĐỂ MUA " +
                    card.cardName + "!"
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

        if (moneyText != null)
        {
            moneyText.text =
                "" + currencyManager.GetMoney();
        }

        if (energyText != null)
        {
            energyText.text =
                "" + currencyManager.GetEnergy();
        }
    }
    public void ResetShop()
    {
        // Reset giới hạn mua của tất cả card
        storeManager.ResetPurchaseLimits();

        // Chọn và hiển thị lại 4 card ngẫu nhiên
        RefreshStore();
    }


}