
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
    private bool reportedMissingCardUI;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Lấy CardUI của Card hiện tại
        TryGetCardUI();

        // Pivot nằm ở đáy Card
        rectTransform.pivot = new Vector2(0.5f, 0f);
    }

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        fullHeight = rectTransform.rect.height;

        if (collapsedHeight <= 0f)
        {
            collapsedHeight = rectTransform.rect.width;
        }

        SetCardHeight(collapsedHeight);

        if (TryGetCardUI())
        {
            cardUI.SetHover(false);
        }
    }

    // =====================================================
    // GET CARD UI
    // =====================================================

    private bool TryGetCardUI()
    {
        if (cardUI != null)
            return true;

        cardUI = GetComponent<CardUI>();

        if (cardUI == null)
        {
            if (!reportedMissingCardUI)
            {
                Debug.LogError(
                    $"CardInputHandler: {gameObject.name} " +
                    "không tìm thấy Component CardUI!",
                    gameObject
                );

                reportedMissingCardUI = true;
            }

            return false;
        }

        reportedMissingCardUI = false;
        return true;
    }

    // =====================================================
    // INIT
    // =====================================================

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
        if (!TryGetCardUI())
            return;

        if (resizeCoroutine != null)
        {
            StopCoroutine(resizeCoroutine);
            resizeCoroutine = null;
        }

        float targetHeight;

        if (hover)
        {
            // Hover: mở Card đầy đủ
            targetHeight = fullHeight;

            cardUI.SetHover(true);
        }
        else
        {
            // Không Hover: thu Card thành hình vuông
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

        if (animationDuration <= 0f)
        {
            SetCardHeight(targetHeight);
            resizeCoroutine = null;
            yield break;
        }

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
                "CardInputHandler: Chưa được gán CardManager! " +
                "HandManager cần gọi Init khi Instantiate.",
                gameObject
            );

            return;
        }

        if (!TryGetCardUI())
            return;

        cardManager.OnCardClicked(this);
    }

    // =====================================================
    // SELECTED
    // =====================================================

    public void SetSelectedVisual(bool selected)
    {
        isSelected = selected;

        // Kiểm tra CardUI trước khi sử dụng
        if (!TryGetCardUI())
        {
            Debug.LogWarning(
                $"CardInputHandler: Không thể cập nhật " +
                $"Selected cho {gameObject.name}.",
                gameObject
            );

            return;
        }

        cardUI.SetSelected(selected);

        // Không Scale
        // Không di chuyển
        // Không thay đổi kích thước khi Click
    }

    // =====================================================
    // GET CARD UI
    // =====================================================

    public CardUI GetCardUI()
    {
        if (!TryGetCardUI())
            return null;

        return cardUI;
    }
}
