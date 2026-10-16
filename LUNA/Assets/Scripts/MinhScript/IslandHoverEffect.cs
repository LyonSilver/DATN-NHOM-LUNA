using UnityEngine;
using UnityEngine.EventSystems;

public class IslandHoverEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    public float hoverScale = 1.15f;
    public float hoverHeight = 15f;
    public float animationSpeed = 10f;

    private RectTransform rectTransform;

    private Vector3 originalScale;
    private Vector2 originalPosition;

    private bool isHovering = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        Vector3 targetScale;
        Vector2 targetPosition;

        if (isHovering)
        {
            targetScale = originalScale * hoverScale;

            targetPosition = originalPosition
                + Vector2.up * hoverHeight;
        }
        else
        {
            targetScale = originalScale;
            targetPosition = originalPosition;
        }

        float t = 1f - Mathf.Exp(
            -animationSpeed * Time.unscaledDeltaTime
        );

        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetScale,
            t
        );

        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            targetPosition,
            t
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        // Đưa đảo lên trên các UI cùng cấp
        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    void OnDisable()
    {
        isHovering = false;

        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}