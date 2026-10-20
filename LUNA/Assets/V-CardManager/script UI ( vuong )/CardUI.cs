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

    [Header("TEST ONLY")]
    public CardData testDataOnStart;

    private Image rootImage;
    private CanvasGroup canvasGroup;
    private StoreUI storeUI;

    private void Awake()
    {
        rootImage = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (testDataOnStart != null)
            Setup(testDataOnStart);
    }

    public void Setup(CardData data)
    {
        if (data == null)
        {
            Debug.LogWarning("CardUI.Setup: CardData null.");
            return;
        }

        cardData = data;

        if (costText != null) costText.text = data.cost.ToString();
        if (cardNameText != null) cardNameText.text = data.cardName;
        if (typeLabelText != null) typeLabelText.text = data.cardType.ToString().ToUpper();
        if (descriptionText != null) descriptionText.text = data.description;

        if (artworkImage != null)
        {
            artworkImage.sprite = data.artwork != null ? data.artwork : data.cardSprite;
            artworkImage.enabled = artworkImage.sprite != null;
        }

        if (typeIconImage != null && data.typeIcon != null)
            typeIconImage.sprite = data.typeIcon;

        ApplyTypeColor(data.cardType);

        SetHover(false);
        SetSelected(false);
        SetDisabled(false);
        SetInteractable(true);
        SetDimmed(false);
    }

    public void SetupForStore(CardData card, StoreUI storeUI, StoreManager storeManager, CardInventory inventory)
    {
        this.storeUI = storeUI;
        Setup(card);

        if (card == null) return;

        Button buyBtn = GetComponentInChildren<Button>(true);
        if (buyBtn != null)
        {
            buyBtn.onClick.RemoveAllListeners();
            buyBtn.onClick.AddListener(() => storeUI.BuyCard(card));
        }
    }

    public void UpdateCardState()
    {
        if (cardData == null) return;

        bool canBuy = true;

        var inventory = FindObjectOfType<CardInventory>();
        if (inventory != null && !inventory.CanAddCard(cardData))
            canBuy = false;

        SetDisabled(!canBuy);
        SetInteractable(canBuy);
    }

    private void ApplyTypeColor(CardType type)
    {
        Color accent = GetColorForType(type);

        if (typeLabelImage != null) typeLabelImage.color = accent;
        if (costBadgeImage != null) costBadgeImage.color = accent;
        if (typeIconImage != null) typeIconImage.color = accent;
    }

    public static Color GetColorForType(CardType type)
    {
        switch (type)
        {
            case CardType.Attack:
                return new Color32(196, 90, 40, 255);
            case CardType.Guard:
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
        if (hoverHighlight != null) hoverHighlight.SetActive(isOn);
    }

    public void SetSelected(bool isOn)
    {
        if (selectionHighlight != null) selectionHighlight.SetActive(isOn);
    }

    public void SetDisabled(bool isOn)
    {
        if (disabledOverlay != null) disabledOverlay.SetActive(isOn);
    }

    public void SetInteractable(bool isOn)
    {
        if (rootImage != null) rootImage.raycastTarget = isOn;
    }

    public void SetDimmed(bool isOn)
    {
        if (canvasGroup != null) canvasGroup.alpha = isOn ? 0.55f : 1f;
    }
}