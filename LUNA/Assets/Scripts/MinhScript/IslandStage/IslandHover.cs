
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class IslandHover : MonoBehaviour
{
    [Header("Hover Settings")]
    [SerializeField] float hoverScale = 1.08f;
    [SerializeField] float animationSpeed = 5f;

    [Header("Island Color")]
    [SerializeField] Color hoverColor = Color.white;

    [SerializeField] Color lockedColor =
        new Color(0.4f, 0.4f, 0.4f, 1f);

    [Header("Island State")]
    [SerializeField] bool isUnlocked = true;

    private SpriteRenderer spriteRenderer;
    private Collider2D islandCollider;
    private Camera mainCamera;

    private Vector3 originalScale;
    private Color originalColor;

    private bool isHovering;
    private float hoverAmount;

    public bool IsUnlocked => isUnlocked;
    public bool IsHovering => isHovering;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        islandCollider = GetComponent<Collider2D>();

        mainCamera = Camera.main;

        originalScale = transform.localScale;
        originalColor = spriteRenderer.color;

        RefreshColor();
    }

    private void Update()
    {
        CheckHover();
        AnimateHover();
    }

    // =====================================
    // CHECK MOUSE
    // =====================================

    private void CheckHover()
    {
        isHovering = false;

        if (!isUnlocked)
            return;

        if (Mouse.current == null || mainCamera == null)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Vector2 worldPosition =
            mainCamera.ScreenToWorldPoint(mousePosition);

        isHovering =
            islandCollider.OverlapPoint(worldPosition);
    }

    // =====================================
    // HOVER ANIMATION
    // =====================================

    private void AnimateHover()
    {
        float target = isHovering ? 1f : 0f;

        hoverAmount = Mathf.MoveTowards(
            hoverAmount,
            target,
            animationSpeed * Time.unscaledDeltaTime
        );

        float smooth = Mathf.SmoothStep(
            0f, 1f, hoverAmount
        );

        transform.localScale = Vector3.Lerp(
            originalScale,
            originalScale * hoverScale,
            smooth
        );

        if (isUnlocked)
        {
            spriteRenderer.color = Color.Lerp(
                originalColor,
                hoverColor,
                smooth
            );
        }
        else
        {
            spriteRenderer.color = lockedColor;
        }
    }

    // =====================================
    // LOCK / UNLOCK
    // =====================================

    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
        isHovering = false;

        RefreshColor();
    }

    private void RefreshColor()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = isUnlocked
            ? originalColor
            : lockedColor;
    }

    private void OnDisable()
    {
        isHovering = false;
        hoverAmount = 0f;

        if (spriteRenderer != null)
        {
            transform.localScale = originalScale;
            RefreshColor();
        }
    }
}