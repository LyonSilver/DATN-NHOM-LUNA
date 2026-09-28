using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class CardInputHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Hover Settings")]
    public float hoverScale = 1.08f;
    public float hoverMoveY = 40f;

    [Header("Selected Settings")]
    public float selectedScale = 1.10f;

    private RectTransform rectTransform;
    private CardUI cardUI;
    private CardManager cardManager;

    private Vector2 basePosition;
    private Vector3 baseScale;
    private bool isSelected;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cardUI = GetComponent<CardUI>();
    }

    private void Start()
    {
        basePosition = rectTransform.anchoredPosition;
        baseScale = rectTransform.localScale;
    }

    public void Init(CardManager manager)
    {
        cardManager = manager;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelected) return;

        rectTransform.SetAsLastSibling();
        rectTransform.anchoredPosition = basePosition + new Vector2(0f, hoverMoveY);
        rectTransform.localScale = baseScale * hoverScale;
        cardUI.SetHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelected) return;

        rectTransform.anchoredPosition = basePosition;
        rectTransform.localScale = baseScale;
        cardUI.SetHover(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (cardManager == null)
        {
            Debug.LogWarning("CardInputHandler: Chưa được gán CardManager (HandManager cần gọi Init khi Instantiate).");
            return;
        }

        cardManager.OnCardClicked(this);
    }

    public void SetSelectedVisual(bool selected)
    {
        isSelected = selected;
        cardUI.SetSelected(selected);

        if (selected)
        {
            rectTransform.SetAsLastSibling();
            rectTransform.anchoredPosition = basePosition + new Vector2(0f, hoverMoveY);
            rectTransform.localScale = baseScale * selectedScale;
        }
        else
        {
            cardUI.SetHover(false);
            rectTransform.anchoredPosition = basePosition;
            rectTransform.localScale = baseScale;
        }
    }

    public CardUI GetCardUI()
    {
        return cardUI;
    }
}