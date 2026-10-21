
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform), typeof(Image), typeof(Button))]
public class IslandHoverEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Island Information")]
    public string islandTitle = "Đảo Băng";

    [TextArea(2, 5)]
    public string description = "Một vùng đất băng giá bí ẩn.";

    public string difficulty = "3 / 5";

    [Header("Hover Visual")]
    public Color glowColor = Color.cyan;
    public float riseDistance = 18f;
    public float hoverScale = 1.08f;
    public float animationSpeed = 5f;

    [Header("References")]
    public IslandInfoPanel infoPanel;

    private RectTransform rect;
    private Outline outline;

    private Vector2 originalPosition;
    private Vector3 originalScale;

    private bool isHovering;
    private float hoverAmount;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        outline = GetComponent<Outline>();

        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        outline.effectDistance = new Vector2(5f, 5f);
        outline.useGraphicAlpha = false;

        originalPosition = rect.anchoredPosition;
        originalScale = rect.localScale;

        // Tắt Color Tint mặc định của Button.
        GetComponent<Button>().transition =
            Selectable.Transition.None;

        SetGlowAlpha(0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cinematicLocked) return;
        isHovering = true;

        if (infoPanel != null)
            infoPanel.Show(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (cinematicLocked) return;
        isHovering = false;

        if (infoPanel != null)
            infoPanel.Hide(this);
    }

    private void Update()
    {
        if (cinematicLocked) return;
        float target = isHovering ? 1f : 0f;

        hoverAmount = Mathf.MoveTowards(
            hoverAmount,
            target,
            animationSpeed * Time.unscaledDeltaTime
        );

        float smooth = Mathf.SmoothStep(0f, 1f, hoverAmount);

        // Đảo bay lên.
        rect.anchoredPosition = Vector2.Lerp(
            originalPosition,
            originalPosition + Vector2.up * riseDistance,
            smooth
        );

        // Đảo phóng to nhẹ.
        rect.localScale = Vector3.Lerp(
            originalScale,
            originalScale * hoverScale,
            smooth
        );

        // Ánh sáng dao động nhẹ khi hover.
        float pulse = 0.75f +
            0.25f * Mathf.Sin(Time.unscaledTime * 4f);

        SetGlowAlpha(smooth * pulse);
    }

    private void SetGlowAlpha(float alpha)
    {
        if (outline == null) return;

        Color color = glowColor;
        color.a = alpha;
        outline.effectColor = color;
    }

    private void OnDisable()
    {
        isHovering = false;
        hoverAmount = 0f;

        if (rect != null)
        {
            rect.anchoredPosition = originalPosition;
            rect.localScale = originalScale;
        }

        SetGlowAlpha(0f);

        if (infoPanel != null)
            infoPanel.Hide(this);
    }
    private bool cinematicLocked;

public void SetCinematicMode(bool locked)
{
    cinematicLocked = locked;

    if (locked)
    {
        isHovering = false;
        hoverAmount = 0f;

        rect.anchoredPosition = originalPosition;
        rect.localScale = originalScale;

        SetGlowAlpha(0f);
    }
}
}
