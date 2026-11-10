using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("Dữ liệu Card hiện tại (Chỉ để xem, không chỉnh sửa trực tiếp)")]
    public CardData cardData;

    [Header("Thư viện hiển thị (18 art, icon loại thẻ, Material 5 phương hệ)")]
    public CardVisualLibrary visualLibrary;

    [Header("Frame / Background")]
    public Image cardBackgroundImage;
    public Image cardBorderImage;
    public Image costBadgeImage;
    public Image typeIconImage;
    public Image typeLabelImage;

    [Header("Khung đổi màu bằng Shader theo phương hệ")]
    [Tooltip("Từng Image riêng lẻ: BannerType, TypeLabel...")]
    public Image[] frameTintImageTargets;

    [Tooltip("Object chứa nhiều Image con: Mọi Image bên trong đều được nhuộm.")]
    public Transform[] frameTintGroupTargets;

    [Header("Text")]
    public TMP_Text costText;
    public TMP_Text cardNameText;
    public TMP_Text typeLabelText;
    public TMP_Text descriptionText;

    [Header("Artwork")]
    public Image artworkImage;
    [Tooltip("Tắt = Giữ nguyên màu artwork. Bật = nhuộm artwork theo phương hệ bằng shader.")]
    public bool tintArtworkByElement = false;

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

    // Các Image của khung đổi màu bằng Shader, kèm Material và màu gốc để khôi phục
    private readonly List<Image> frameImages = new List<Image>();
    private readonly Dictionary<Image, Material> frameDefaultMaterials = new Dictionary<Image, Material>();
    private readonly Dictionary<Image, Color> frameDefaultColors = new Dictionary<Image, Color>();

    // Phương hệ và mức độ đang HIỂN THỊ (mặc định lấy từ CardData, nút TEST có thể đổi tạm)
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

        // Cache từng Image riêng lẻ
        if (frameTintImageTargets != null)
        {
            foreach (Image image in frameTintImageTargets)
            {
                RegisterFrameImage(image);
            }
        }

        // Cache toàn bộ Image bên trong các Group
        if (frameTintGroupTargets != null)
        {
            foreach (Transform group in frameTintGroupTargets)
            {
                if (group == null) continue;

                foreach (Image image in group.GetComponentsInChildren<Image>(true))
                {
                    RegisterFrameImage(image);
                }
            }
        }

        defaultsCached = true;
    }

    private void RegisterFrameImage(Image image)
    {
        if (image == null || frameDefaultMaterials.ContainsKey(image)) return;

        frameImages.Add(image);
        frameDefaultMaterials[image] = image.material;
        frameDefaultColors[image] = image.color;
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
        ApplyFrameTint(); // chạy SAU ApplyIconAndAccent: Image nằm trong danh sách Shader sẽ bị đặt màu trắng
    }

    private void ApplyArtwork()
    {
        if (artworkImage == null) return;

        // Thẻ có Tier: lấy art theo (yếu tố, mức độ) từ thư viện.
        // Thẻ Tier = None, hoặc ô thư viện còn trống: dùng artwork riêng trong CardData.
        Sprite art = cardData.artwork;

        if (shownTier != CardTier.None && visualLibrary != null)
        {
            Sprite libraryArt = visualLibrary.GetArt(cardData.cardType, shownElement, shownTier);            if (libraryArt != null) art = libraryArt;
        }

        artworkImage.sprite = art;
        artworkImage.enabled = art != null;

        // Mặc định KHÔNG nhuộm Artwork: luôn dùng Material gốc của prefab.
        // Chỉ nhuộm khi bật tintArtworkByElement.
        Material elementMaterial = null;

        if (tintArtworkByElement && shownElement != CardElement.None && visualLibrary != null)
        {
            elementMaterial = visualLibrary.GetElementMaterial(shownElement);
        }

        artworkImage.material = elementMaterial != null ? elementMaterial : defaultArtworkMaterial;
    }

    private void ApplyIconAndAccent()
    {
        // Thẻ có phương hệ: màu theo phương hệ. Thẻ không hệ (Chaos, Starter cũ): màu theo loại thẻ như trước.
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

    // Khung (BannerType, TypeLabel, OutlineDecor...): dùng Material đổi màu của phương hệ và Color trắng.
    // Thẻ không phương hệ: trả về Material và màu gốc của prefab.
    private void ApplyFrameTint()
    {
        if (frameImages.Count == 0) return;

        Material elementMaterial = null;

        if (shownElement != CardElement.None && visualLibrary != null)
        {
            elementMaterial = visualLibrary.GetElementMaterial(shownElement);
        }

        foreach (Image image in frameImages)
        {
            if (image == null) continue;

            if (elementMaterial != null)
            {
                image.material = elementMaterial;
                image.color = Color.white;
            }
            else
            {
                image.material = frameDefaultMaterials[image];
                image.color = frameDefaultColors[image];
            }
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
            case CardType.Heal:
                return new Color32(60, 170, 80, 255);
            case CardType.Poison:
                return new Color32(130, 60, 170, 255);
            case CardType.Focus:
                return new Color32(210, 175, 60, 255);
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