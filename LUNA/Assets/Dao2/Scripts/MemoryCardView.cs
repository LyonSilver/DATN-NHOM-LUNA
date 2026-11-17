
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

    private LostMemoriesManager gameManager;
    private Color originalColor = Color.white;

    public MemoryCardKind Kind { get; private set; }
    public int PairId { get; private set; }

    public bool IsFaceUp { get; private set; }
    public bool IsMatched { get; private set; }
    public bool IsSelected { get; private set; }

    public bool CanSelect =>
        gameObject.activeInHierarchy &&
        !IsFaceUp &&
        !IsMatched;

    private void Awake()
    {
        if (cardButton == null)
            cardButton = GetComponent<Button>();

        if (cardButton == null)
        {
            Debug.LogError($"[{name}] Missing Button component.");
            return;
        }

        if (cardButton.targetGraphic != null)
            originalColor = cardButton.targetGraphic.color;
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

        if (frontImage != null)
        {
            frontImage.sprite = faceSprite;
            frontImage.gameObject.SetActive(false);
        }

        if (backImage != null)
            backImage.gameObject.SetActive(true);

        ResetSelectionVisual();

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

        if (cardButton != null &&
            cardButton.targetGraphic != null)
        {
            cardButton.targetGraphic.color = selected
                ? new Color(0.65f, 0.9f, 1f, 1f)
                : originalColor;
        }
    }

    private void ResetSelectionVisual()
    {
        IsSelected = false;

        if (cardButton != null &&
            cardButton.targetGraphic != null)
        {
            cardButton.targetGraphic.color = originalColor;
        }
    }

    // ==========================================
    // REVEAL AND HIDE
    // ==========================================

    public void RevealCard()
    {
        if (IsFaceUp || IsMatched)
            return;

        IsFaceUp = true;

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

    // Called when a bomb destroys a completed pair.
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

    private void OnDestroy()
    {
        if (cardButton != null)
            cardButton.onClick.RemoveListener(HandleClick);
    }
}
