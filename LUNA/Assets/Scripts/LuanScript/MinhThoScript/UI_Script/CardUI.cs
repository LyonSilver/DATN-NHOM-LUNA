using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("Dữ liệu Card hiện tại (Chỉ để xem, không chỉnh sửa trực tiếp)")]
    public CardData cardData;

    [Header("Thư viện hiển thị (12 art, icon loại thẻ, Material 5 phương hệ)")]
    public CardVisualLibrary visualLibrary;

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
    private Material defaultArtworkMaterial;
    private bool defaultsCached;

    // Phương hệ và mức độ đang HIỂN THỊ (Mặc định lấy từ CardData, nút TEST có thể đổi tạm)
    private CardElement shownElement;
    private CardTier shownTier;

    private void Awake()
    {
        rootImage = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        CacheDefaults();
    }

    private void CacheDefaults()
    {
        if (defaultsCached) return;

        if (artworkImage != null)
        {
            defaultArtworkMaterial = artworkImage.material;
        }

        defaultsCached = true;
    }

    private void Start()
    {
        if (testDataOnStart != null)
        {
            Setup(testDataOnStart);
        }
    }

    public void Setup(CardData data)
    {
        if (data == null)
        {
            Debug.LogWarning("CardUI.Setup: CardData đang null, không thể hiện thị Card.");
            return;
        }

        CacheDefaults();

        cardData = data;
        shownElement = data.element;
        shownTier = data.tier;

        if (costText != null) costText.text = data.cost.ToString();
        if (cardNameText != null) cardNameText.text = data.cardName;
        if (typeLabelText != null) typeLabelText.text = data.cardType.ToString().ToUpper();
        if (descriptionText != null) descriptionText.text = data.description;

        RefreshVisuals();

        SetHover(false);
        SetSelected(false);
        SetDisabled(false);
        SetInteractable(true);
        SetDimmed(false);
    }

    // =====================================================
    // VISUALS: ART + MÀU THEO PHƯƠNG HỆ
    // =====================================================

    private void RefreshVisuals()
    {
        if (cardData == null) return;

        ApplyArtwork();
        ApplyIconAndAccent();
    }

    private void ApplyArtwork()
    {
        if (artworkImage == null) return;

        // Thẻ có Tier: Lấy art theo (yếu tố, mức độ) từ thư viện.
        // Thẻ Tier = None, hoặc ô thư viện còn trống: Dùng artwork riêng trong CardData.
        Sprite art = cardData.artwork;

        if (shownTier != CardTier.None && visualLibrary != null)
        {
            Sprite libraryArt = visualLibrary.GetArt(cardData.cardType, shownTier);
            if (libraryArt != null) art = libraryArt;
        }

        artworkImage.sprite = art;
        artworkImage.enabled = art != null;

        // Thẻ có phương hệ: Dùng Material đổi màu. Thẻ không hệ: Trả về Material gốc của prefab.
        Material elementMaterial = null;

        if (shownElement != CardElement.None && visualLibrary != null)
        {
            elementMaterial = visualLibrary.GetElementMaterial(shownElement);
        }

        artworkImage.material = elementMaterial != null ? elementMaterial : defaultArtworkMaterial;
    }

    private void ApplyIconAndAccent()
    {
        // Thẻ có phương hệ: Màu theo phương hệ. Thẻ không hệ (Chaos): Màu theo loại thẻ như trước.
        Color accent = shownElement != CardElement.None
            ? ElementRules.GetColor(shownElement)
            : GetColorForType(cardData.cardType);

        if (typeLabelImage != null) typeLabelImage.color = accent;
        if (costBadgeImage != null) costBadgeImage.color = accent;

        if (typeIconImage != null)
        {
            Sprite icon = cardData.typeIcon;

            if (icon == null && visualLibrary != null)
            {
                icon = visualLibrary.GetTypeIcon(cardData.cardType);
            }

            if (icon != null)
            {
                typeIconImage.sprite = icon;
            }

            typeIconImage.color = accent;
        }
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

    // =====================================================
    // STATES
    // =====================================================

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

    // =====================================================
    // TEST (chỉ đổi cách HIỂN THỊ trong lúc Play, không sửa asset)
    // =====================================================

    [ContextMenu("TEST - Phương hệ kế tiếp")]
    private void TestNextElement()
    {
        if (cardData == null) return;

        int count = System.Enum.GetValues(typeof(CardElement)).Length;
        shownElement = (CardElement)(((int)shownElement + 1) % count);
        RefreshVisuals();

        Debug.Log("CardUI TEST: Phương hệ hiển thị = " + ElementRules.GetNameVN(shownElement));
    }

    [ContextMenu("TEST - Mức độ kế tiếp")]
    private void TestNextTier()
    {
        if (cardData == null) return;

        int count = System.Enum.GetValues(typeof(CardTier)).Length;
        shownTier = (CardTier)(((int)shownTier + 1) % count);
        RefreshVisuals();

        Debug.Log("CardUI TEST: Mức độ hiển thị = " + shownTier);
    }
}