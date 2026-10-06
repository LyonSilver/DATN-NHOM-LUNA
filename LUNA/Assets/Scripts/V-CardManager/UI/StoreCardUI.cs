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

        // Xóa listener cũ để tránh trùng lặp
        buyButton.onClick.RemoveAllListeners();

        // Bổ sung UnlockableItemComponent tự động nếu chưa có
        UnlockableItemComponent unlockComp = GetComponent<UnlockableItemComponent>();
        if (unlockComp == null)
        {
            unlockComp = gameObject.AddComponent<UnlockableItemComponent>();
        }

        // Truyền dữ liệu sang script của bạn
        unlockComp.itemId = card.cardName;
        unlockComp.actionButton = buyButton;

        // KIỂM TRA ĐIỀU KIỆN MUA
        if (IsLegendaryCard())
        {
            buyButton.interactable = false;
            SetButtonText("Locked");
        }
        else if (UnlockManager.Instance != null && UnlockManager.Instance.IsUnlocked(unlockComp.unlockType, card.cardName))
        {
            // Nếu thẻ đã được mua/mở khóa rồi
            buyButton.interactable = false;
            SetButtonText("Owned");
        }
        else
        {
            // Thẻ bình thường và chưa mua
            buyButton.interactable = true;
            buyButton.onClick.AddListener(BuyCard);
        }
    }

    private bool IsLegendaryCard()
    {
        if (cardData == null) return false;

        // Kiểm tra theo Rarity hoặc tên thẻ
        string rarityStr = cardData.rarity.ToString().ToLower();
        string nameStr = string.IsNullOrEmpty(cardData.cardName) ? "" : cardData.cardName.ToLower();

        return rarityStr.Contains("legendary") || nameStr.Contains("chaos") || nameStr.Contains("legendary");
    }

    private void SetButtonText(string text)
    {
        TMP_Text tmpText = buyButton.GetComponentInChildren<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = text;
            return;
        }

        Text legacyText = buyButton.GetComponentInChildren<Text>();
        if (legacyText != null)
        {
            legacyText.text = text;
        }
    }

    private void UpdatePrice()
    {
        if (cardData == null) return;

        switch (cardData.purchaseType)
        {
            case PurchaseType.Money:
                priceText.text = "Money: " + cardData.moneyCost;
                break;

            case PurchaseType.Energy:
                priceText.text = "Energy: " + cardData.energyCost;
                break;

            case PurchaseType.MoneyAndEnergy:
                priceText.text = "Money: " + cardData.moneyCost + "\nEnergy: " + cardData.energyCost;
                break;
        }
    }

    private void BuyCard()
    {
        if (IsLegendaryCard()) return;

        // 1. Gọi hàm Mua của Cửa hàng gốc
        if (storeUI != null)
        {
            storeUI.BuyCard(cardData);
        }

        // 2. Cập nhật trạng thái mở khóa sang UnlockManager của bạn
        if (UnlockManager.Instance != null && cardData != null)
        {
            UnlockableItemComponent unlockComp = GetComponent<UnlockableItemComponent>();
            UnlockType type = unlockComp != null ? unlockComp.unlockType : default;
            
            UnlockManager.Instance.Unlock(type, cardData.cardName);
        }

        // 3. Cập nhật lại giao diện Nút ngay sau khi mua
        buyButton.interactable = false;
        SetButtonText("Owned");
    }
}