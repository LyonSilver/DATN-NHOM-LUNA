using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerClickHandler
{
    [Header("Drag (Chuột phải)")]
    public float smoothTime = 0.04f;

    [Header("Menu (Chuột trái)")]
    public RadialMenuController menuController;

    [Header("Press Feedback")]
    public float pressScale = 0.9f;
    public float pressSpeed = 12f;

    private RectTransform rect;
    private Canvas canvas;
    private Vector2 targetPos;
    private Vector2 velocity;
    private bool isRightDragging;
    private bool pointerDown;
    private Vector3 originalScale;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDown = true;

        // Chuột phải = bắt đầu kéo
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            isRightDragging = true;
            velocity = Vector2.zero;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDown = false;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            isRightDragging = false;
            targetPos = rect.anchoredPosition;
            velocity = Vector2.zero;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Chỉ kéo khi giữ chuột phải
        if (eventData.button != PointerEventData.InputButton.Right) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        targetPos = localPoint;
    }

    void Update()
    {
        if (isRightDragging)
        {
            rect.anchoredPosition = Vector2.SmoothDamp(
                rect.anchoredPosition,
                targetPos,
                ref velocity,
                smoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime);
        }

        // Hiệu ứng nhấn
        Vector3 targetScale = pointerDown ? originalScale * pressScale : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * pressSpeed);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Chỉ chuột trái mới mở menu
        if (eventData.button == PointerEventData.InputButton.Left && menuController != null)
        {
            menuController.ToggleMenu();
        }
    }
}