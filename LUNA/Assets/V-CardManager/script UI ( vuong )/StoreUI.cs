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
        Debug.LogError(
            "StoreUI: Chưa gán Store Panel hoặc Card Prefab."
        );
        return;
    }

    if (storeManager == null || cardInventory == null)
    {
        Debug.LogError(
            "StoreUI: Chưa gán StoreManager hoặc CardInventory."
        );
        return;
    }

    // Lấy danh sách card từ database.
    List<CardData> databaseCards = cardDatabase.GetStoreCards();

    if (databaseCards == null || databaseCards.Count == 0)
    {
        Debug.LogWarning("StoreUI: Không có card trong database.");
        return;
    }

    // Lọc card null và loại bỏ card trùng.
    List<CardData> availableCards = new List<CardData>();

    for (int i = 0; i < databaseCards.Count; i++)
    {
        CardData card = databaseCards[i];

        if (card != null && !availableCards.Contains(card))
        {
            availableCards.Add(card);
        }
    }

    if (availableCards.Count == 0)
    {
        Debug.LogWarning("StoreUI: Không có card hợp lệ.");
        return;
    }

    // Xóa giao diện Shop cũ.
    ClearStore();
    currentStoreCards.Clear();

    // Chọn tối đa số card được cấu hình.
    int targetCount = Mathf.Min(
        cardsPerRefresh,
        availableCards.Count
    );

    // Chọn ngẫu nhiên, không chọn trùng.
    for (int i = 0; i < targetCount; i++)
    {
        int randomIndex = Random.Range(
            0,
            availableCards.Count
        );

        CardData selectedCard = availableCards[randomIndex];

        currentStoreCards.Add(selectedCard);
        availableCards.RemoveAt(randomIndex);
    }

    // Tạo prefab Card 3 sử dụng CardUI.
    for (int i = 0; i < currentStoreCards.Count; i++)
    {
        CardData card = currentStoreCards[i];

        GameObject cardObject = Instantiate(
            storeCardPrefab,
            storePanel
        );

        CardUI cardUI = cardObject.GetComponent<CardUI>();

        if (cardUI == null)
        {
            Debug.LogError(
                "StoreUI: Prefab Shop cần có CardUI trên GameObject gốc."
            );

            Destroy(cardObject);
            continue;
        }

        cardUI.SetupForStore(
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
        return;

    for (int i = 0; i < storePanel.childCount; i++)
    {
        CardUI cardUI =
            storePanel.GetChild(i).GetComponent<CardUI>();

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
    RefreshStore();
}


}