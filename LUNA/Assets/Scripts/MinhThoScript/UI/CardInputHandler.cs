using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class CardInputHandler : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Card Size")]
    [SerializeField] private float collapsedHeight = 0f;
    [SerializeField] private float animationDuration = 0.15f;

    private RectTransform rectTransform;
    private CardUI cardUI;
    private CardManager cardManager;

    private float fullHeight;
    private Coroutine resizeCoroutine;

    private bool isSelected;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cardUI = GetComponent<CardUI>();

        // QUAN TRỌNG:
        // Pivot nằm ở đáy Card.
        // Khi tăng Height, Card sẽ mọc lên phía trên.
        rectTransform.pivot = new Vector2(0.5f, 0f);
    }

    private void Start()
    {
        // Chiều cao đầy đủ của Card
        fullHeight = rectTransform.rect.height;

        // Nếu không nhập thì lấy Width làm chiều cao hình vuông
        if (collapsedHeight <= 0f)
        {
            collapsedHeight = rectTransform.rect.width;
        }

        // Ban đầu: hình vuông
        SetCardHeight(collapsedHeight);

        cardUI.SetHover(false);
    }

    public void Init(CardManager manager)
    {
        cardManager = manager;
    }

    // =====================================================
    // MOUSE ENTER
    // =====================================================

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelected)
            return;

        rectTransform.SetAsLastSibling();

        SetHoverVisual(true);
    }

    // =====================================================
    // MOUSE EXIT
    // =====================================================

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelected)
            return;

        SetHoverVisual(false);
    }

    // =====================================================
    // HOVER
    // =====================================================

    public void SetHoverVisual(bool hover)
    {
        if (resizeCoroutine != null)
        {
            StopCoroutine(resizeCoroutine);
        }

        float targetHeight;

        if (hover)
        {
            // Hover → mở thành Card đầy đủ
            targetHeight = fullHeight;

            cardUI.SetHover(true);
        }
        else
        {
            // Không hover → thu thành hình vuông
            targetHeight = collapsedHeight;

            cardUI.SetHover(false);
        }

        resizeCoroutine = StartCoroutine(
            AnimateHeight(targetHeight)
        );
    }

    // =====================================================
    // ANIMATE HEIGHT
    // =====================================================

    private IEnumerator AnimateHeight(float targetHeight)
    {
        float startHeight = rectTransform.rect.height;

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / animationDuration
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            float currentHeight = Mathf.Lerp(
                startHeight,
                targetHeight,
                t
            );

            SetCardHeight(currentHeight);

            yield return null;
        }

        SetCardHeight(targetHeight);

        resizeCoroutine = null;
    }

    // =====================================================
    // SET HEIGHT
    // =====================================================

    private void SetCardHeight(float height)
    {
        Vector2 size = rectTransform.sizeDelta;

        size.y = height;

        rectTransform.sizeDelta = size;
    }

    // =====================================================
    // CLICK
    // =====================================================

    public void OnPointerClick(PointerEventData eventData)
    {
        if (cardManager == null)
        {
            Debug.LogWarning(
                "CardInputHandler: Chưa được gán CardManager " +
                "(HandManager cần gọi Init khi Instantiate)."
            );

            return;
        }

        cardManager.OnCardClicked(this);
    }

    // =====================================================
    // SELECTED
    // =====================================================

    public void SetSelectedVisual(bool selected)
    {
        isSelected = selected;

        cardUI.SetSelected(selected);

        // Không scale.
        // Không di chuyển.
        // Không đổi kích thước khi click.
    }

    // =====================================================
    // GET CARD UI
    // =====================================================

    public CardUI GetCardUI()
    {
        return cardUI;
    }
}