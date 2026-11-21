
using UnityEngine;
using UnityEngine.UI;

public enum MemoryCardKind
{
    Element,
    LoseTurnTrap,
    BombTrap
}

public class MemoryCardView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button cardButton;
    [SerializeField] private Image frontImage;
    [SerializeField] private Image backImage;

    [Header("Selection Effect")]
    [SerializeField] private Sprite selectedBackSprite;

    private LostMemoriesManager gameManager;

    // Lưu sprite mặt sau ban đầu để khôi phục khi bỏ chọn.
    private Sprite originalBackSprite;

    public MemoryCardKind Kind { get; private set; }
    public int PairId { get; private set; }

    public bool IsFaceUp { get; private set; }
    public bool IsMatched { get; private set; }
    public bool IsSelected { get; private set; }

    public bool CanSelect =>
        gameObject.activeInHierarchy &&
        !IsFaceUp &&
        !IsMatched;

    // ==========================================
    // UNITY LIFECYCLE
    // ==========================================

    private void Awake()
    {
        if (cardButton == null)
            cardButton = GetComponent<Button>();

        if (cardButton == null)
        {
            Debug.LogError(
                $"[{name}] Không tìm thấy Button Component."
            );
            return;
        }

        // Lưu sprite mặt sau ban đầu.
        if (backImage != null)
            originalBackSprite = backImage.sprite;

        // Kiểm tra các Image cần thiết.
        if (frontImage == null)
        {
            Debug.LogError(
                $"[{name}] Chưa gán Front Image."
            );
        }

        if (backImage == null)
        {
            Debug.LogError(
                $"[{name}] Chưa gán Back Image."
            );
        }

        if (selectedBackSprite == null)
        {
            Debug.LogWarning(
                $"[{name}] Chưa gán Selected Back Sprite."
            );
        }
    }

    // ==========================================
    // INITIALIZATION
    // ==========================================

    public void Initialize(
        LostMemoriesManager manager,
        MemoryCardKind kind,
        int pairId,
        Sprite faceSprite)
    {
        gameManager = manager;

        gameObject.SetActive(true);

        Kind = kind;
        PairId = pairId;

        IsFaceUp = false;
        IsMatched = false;
        IsSelected = false;

        // Gán hình mặt trước.
        if (frontImage != null)
        {
            frontImage.sprite = faceSprite;
            frontImage.gameObject.SetActive(false);
        }

        // Khôi phục mặt sau và ẩn hiệu ứng chọn.
        ResetSelectionVisual();

        if (backImage != null)
            backImage.gameObject.SetActive(true);

        if (cardButton != null)
        {
            cardButton.onClick.RemoveListener(HandleClick);
            cardButton.onClick.AddListener(HandleClick);

            cardButton.interactable = false;
        }
    }

    public void SetCardData(
        MemoryCardKind kind,
        int pairId,
        Sprite faceSprite)
    {
        Kind = kind;
        PairId = pairId;

        if (frontImage != null)
            frontImage.sprite = faceSprite;
    }

    // ==========================================
    // CLICK AND SELECTION
    // ==========================================

    private void HandleClick()
    {
        if (gameManager == null)
            return;

        gameManager.OnCardSelected(this);
    }

    public void SetSelected(bool selected)
    {
        if (IsMatched || IsFaceUp)
            return;

        IsSelected = selected;

        if (backImage == null)
            return;

        if (selected && selectedBackSprite != null)
        {
            // Hiển thị khung phát sáng khi chọn thẻ.
            backImage.sprite = selectedBackSprite;
        }
        else
        {
            // Khôi phục hình mặt sau ban đầu.
            backImage.sprite = originalBackSprite;
        }
    }

    private void ResetSelectionVisual()
    {
        IsSelected = false;

        if (backImage != null)
            backImage.sprite = originalBackSprite;
    }

    // ==========================================
    // REVEAL AND HIDE
    // ==========================================

    public void RevealCard()
    {
        if (IsFaceUp || IsMatched)
            return;

        IsFaceUp = true;

        // Xóa hiệu ứng chọn trước khi lật.
        ResetSelectionVisual();

        if (frontImage != null)
            frontImage.gameObject.SetActive(true);

        if (backImage != null)
            backImage.gameObject.SetActive(false);
    }

    public void HideCard()
    {
        if (IsMatched)
            return;

        IsFaceUp = false;

        // Khôi phục mặt sau bình thường.
        ResetSelectionVisual();

        if (frontImage != null)
            frontImage.gameObject.SetActive(false);

        if (backImage != null)
            backImage.gameObject.SetActive(true);
    }

    // ==========================================
    // MATCH STATE
    // ==========================================

    public void SetMatched()
    {
        IsMatched = true;
        IsFaceUp = true;

        ResetSelectionVisual();

        if (cardButton != null)
            cardButton.interactable = false;

        if (frontImage != null)
            frontImage.gameObject.SetActive(true);

        if (backImage != null)
            backImage.gameObject.SetActive(false);
    }

    // Được gọi khi bom phá hủy một cặp đã hoàn thành.
    public void ResetMatchedByBomb()
    {
        IsMatched = false;
        IsFaceUp = false;
        IsSelected = false;

        ResetSelectionVisual();

        if (frontImage != null)
            frontImage.gameObject.SetActive(false);

        if (backImage != null)
            backImage.gameObject.SetActive(true);

        if (cardButton != null)
            cardButton.interactable = false;
    }

    // ==========================================
    // INTERACTION
    // ==========================================

    public void SetSelectable(bool selectable)
    {
        if (cardButton == null)
            return;

        cardButton.interactable =
            selectable &&
            !IsMatched &&
            !IsFaceUp &&
            gameObject.activeInHierarchy;
    }

    // ==========================================
    // CLEANUP
    // ==========================================

    private void OnDestroy()
    {
        if (cardButton != null)
            cardButton.onClick.RemoveListener(HandleClick);
    }
}
