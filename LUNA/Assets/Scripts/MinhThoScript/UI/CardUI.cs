using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("Dữ liệu Card hiện tại")]
    public CardData cardData;

    [Header("Frame / Background")]
    public Image cardBackgroundImage;
    public Image cardBorderImage;
    public Image costBadgeImage;
    public Image typeIconImage;
    public Image typeLabelImage;

    [Header("Text")]
    public TMP_Text costText;
    public TMP_Text cardNameText;
    public TMP_Text typeLabelText;
    public TMP_Text descriptionText;

    [Header("Artwork")]
    public Image artworkImage;

    [Header("State Overlays")]
    public GameObject hoverHighlight;
    public GameObject selectionHighlight;
    public GameObject disabledOverlay;

    [Header("Shop UI")]
    [SerializeField] private Button buyButton;
    [SerializeField] private CanvasGroup cardCanvasGroup;

    [Header("TEST ONLY")]
    public CardData testDataOnStart;

    private Image rootImage;

    private StoreUI storeUI;
    private StoreManager storeManager;
    private CardInventory cardInventory;

    private bool isStoreCard = false;

    private void Awake()
    {
        rootImage = GetComponent<Image>();
        cardCanvasGroup = GetComponent<CanvasGroup>();

        if (cardCanvasGroup == null)
        {
            cardCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        // Không ghi đè dữ liệu do StoreUI truyền vào.
        if (!isStoreCard && testDataOnStart != null)
        {
            Setup(testDataOnStart);
        }
    }

    // Hiển thị card thông thường.
    public void Setup(CardData data)
    {
        if (data == null)
        {
            Debug.LogWarning(
                "CardUI.Setup: CardData đang null."
            );
            return;
        }

        cardData = data;

        if (costText != null)
            costText.text = data.cost.ToString();

        if (cardNameText != null)
            cardNameText.text = data.cardName;

        if (typeLabelText != null)
            typeLabelText.text = data.cardType.ToString().ToUpper();

        if (descriptionText != null)
            descriptionText.text = data.description;

        if (artworkImage != null)
        {
            artworkImage.sprite = data.artwork;
            artworkImage.enabled = data.artwork != null;
        }

        if (typeIconImage != null)
        {
            typeIconImage.sprite = data.typeIcon;
            typeIconImage.enabled = data.typeIcon != null;
        }

        ApplyTypeColor(data.cardType);

        SetHover(false);
        SetSelected(false);
        SetDisabled(false);
        SetInteractable(true);
        SetDimmed(false);
    }

    // Khởi tạo card dành riêng cho Shop.
    public void SetupForStore(
        CardData data,
        StoreUI store,
        StoreManager manager,
        CardInventory inventory)
    {
        isStoreCard = true;

        storeUI = store;
        storeManager = manager;
        cardInventory = inventory;

        if (data == null)
        {
            Debug.LogWarning(
                "CardUI.SetupForStore: CardData đang null."
            );
            return;
        }

        Setup(data);

        // Trong Shop, CostText hiển thị giá mua,
        // không phải chỉ số cost thông thường của card.
        UpdateStorePrice();

        if (buyButton == null)
        {
            Debug.LogError(
                "CardUI: Chưa gán BuyButton trong Inspector."
            );
            return;
        }

        buyButton.onClick.RemoveListener(BuyCard);
        buyButton.onClick.AddListener(BuyCard);

        UpdateCardState();
    }

    // Hiển thị giá theo loại thanh toán.
    private void UpdateStorePrice()
    {
        if (cardData == null || costText == null)
            return;

        switch (cardData.purchaseType)
        {
            case PurchaseType.Money:
                costText.text = "Money: " + cardData.moneyCost;
                break;

            case PurchaseType.Energy:
                costText.text = "Gem: " + cardData.energyCost;
                break;

            case PurchaseType.MoneyAndEnergy:
                costText.text =
                    "Money: " + cardData.moneyCost +
                    "\nGem: " + cardData.energyCost;
                break;
        }
    }

    // Cập nhật trạng thái mua card.
    public void UpdateCardState()
    {
        if (cardData == null ||
            storeManager == null ||
            cardInventory == null)
        {
            return;
        }

        int purchasedCount =
            storeManager.GetPurchasedCount(cardData);

        int ownedCount =
            cardInventory.GetCardQuantity(cardData);

        bool canBuy =
            storeManager.CanBuyMoreInMatch(cardData) &&
            cardInventory.CanAddCard(cardData);

        // Làm mờ card nếu hết lượt mua
        // hoặc đã đạt giới hạn sở hữu.
        SetDimmed(!canBuy);
        SetDisabled(!canBuy);

        if (buyButton != null)
        {
            buyButton.interactable = canBuy;
        }
    }

    private void BuyCard()
    {
        if (cardData == null || storeUI == null)
            return;

        storeUI.BuyCard(cardData);
    }

    private void ApplyTypeColor(CardType type)
    {
        Color accent = GetColorForType(type);

        if (typeLabelImage != null)
            typeLabelImage.color = accent;

        if (costBadgeImage != null)
            costBadgeImage.color = accent;

        if (typeIconImage != null)
            typeIconImage.color = accent;
    }

    public static Color GetColorForType(CardType type)
    {
        switch (type)
        {
            case CardType.Attack:
                return new Color32(196, 90, 40, 255);

            case CardType.Defense:
                return new Color32(45, 130, 180, 255);

            case CardType.Effect:
                return new Color32(70, 150, 70, 255);

            case CardType.Support:
                return new Color32(60, 170, 165, 255);

            case CardType.Chaos:
                return new Color32(210, 175, 60, 255);

            default:
                return Color.white;
        }
    }

    public void SetHover(bool isOn)
    {
        if (hoverHighlight != null)
            hoverHighlight.SetActive(isOn);
    }

    public void SetSelected(bool isOn)
    {
        if (selectionHighlight != null)
            selectionHighlight.SetActive(isOn);
    }

    public void SetDisabled(bool isOn)
    {
        if (disabledOverlay != null)
            disabledOverlay.SetActive(isOn);
    }

    public void SetInteractable(bool isOn)
    {
        if (rootImage != null)
            rootImage.raycastTarget = isOn;
    }

    public void SetDimmed(bool isOn)
    {
        if (cardCanvasGroup != null)
            cardCanvasGroup.alpha = isOn ? 0.55f : 1f;
    }
}





// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class CardUI : MonoBehaviour
// {
//     [Header("Dữ liệu Card hiện tại (Chỉ để xem, không chỉnh sửa trực tiếp)")]
//     public CardData cardData;

//     [Header("Frame / Background")]
//     public Image cardBackgroundImage;
//     public Image cardBorderImage;
//     public Image costBadgeImage;
//     public Image typeIconImage;
//     public Image typeLabelImage;

//     [Header("Text")]
//     public TMP_Text costText;
//     public TMP_Text cardNameText;
//     public TMP_Text typeLabelText;
//     public TMP_Text descriptionText;

//     [Header("Artwork")]
//     public Image artworkImage;

//     [Header("State Overlays")]
//     public GameObject hoverHighlight;
//     public GameObject selectionHighlight;
//     public GameObject disabledOverlay;

//     [Header("TEST ONLY")]
//     public CardData testDataOnStart;

//     private Image rootImage;
//     private CanvasGroup canvasGroup;

//     private void Awake()
//     {
//         rootImage = GetComponent<Image>();
//         canvasGroup = GetComponent<CanvasGroup>();
//     }

//     private void Start()
//     {
//         if (testDataOnStart != null)
//         {
//             Setup(testDataOnStart);
//         }
//     }

//     public void Setup(CardData data)
//     {
//         if (data == null)
//         {
//             Debug.LogWarning("CardUI.Setup: CardData đang null, không thể hiện thị Card.");
//             return;
//         }

//         cardData = data;

//         if (costText != null) costText.text = data.cost.ToString();
//         if (cardNameText != null) cardNameText.text = data.cardName;
//         if (typeLabelText != null) typeLabelText.text = data.cardType.ToString().ToUpper();
//         if (descriptionText != null) descriptionText.text = data.description;

//         if (artworkImage != null)
//         {
//             artworkImage.sprite = data.artwork;
//             artworkImage.enabled = data.artwork != null;
//         }

//         if (typeIconImage != null && data.typeIcon != null)
//         {
//             typeIconImage.sprite = data.typeIcon;
//         }

//         ApplyTypeColor(data.cardType);

//         SetHover(false);
//         SetSelected(false);
//         SetDisabled(false);
//         SetInteractable(true);
//         SetDimmed(false);
//     }

//     private void ApplyTypeColor(CardType type)
//     {
//         Color accent = GetColorForType(type);

//         if (typeLabelImage != null) typeLabelImage.color = accent;
//         if (costBadgeImage != null) costBadgeImage.color = accent;
//         if (typeIconImage != null) typeIconImage.color = accent;
//     }

//     public static Color GetColorForType(CardType type)
//     {
//         switch (type)
//         {
//             case CardType.Attack:
//                 return new Color32(196, 90, 40, 255);
//             case CardType.Defense:
//                 return new Color32(45, 130, 180, 255);
//             case CardType.Effect:
//                 return new Color32(70, 150, 70, 255);
//             case CardType.Support:
//                 return new Color32(60, 170, 165, 255);
//             case CardType.Chaos:
//                 return new Color32(210, 175, 60, 255);
//             default:
//                 return Color.white;
//         }
//     }

//     public void SetHover(bool isOn)
//     {
//         if (hoverHighlight != null) hoverHighlight.SetActive(isOn);
//     }

//     public void SetSelected(bool isOn)
//     {
//         if (selectionHighlight != null) selectionHighlight.SetActive(isOn);
//     }

//     public void SetDisabled(bool isOn)
//     {
//         if (disabledOverlay != null) disabledOverlay.SetActive(isOn);
//     }

//     public void SetInteractable(bool isOn)
//     {
//         if (rootImage != null) rootImage.raycastTarget = isOn;
//     }

//     public void SetDimmed(bool isOn)
//     {
//         if (canvasGroup != null) canvasGroup.alpha = isOn ? 0.55f : 1f;
//     }
// }