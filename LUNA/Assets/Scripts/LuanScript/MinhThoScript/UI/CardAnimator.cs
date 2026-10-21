using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum CardVisualState
{
    Normal,
    Hover,
    Selected,
    Playing,
    Discarding
}

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class CardAnimator : MonoBehaviour
{
    [Header("Độ mượt (Số càng lớn Card càng bám nhanh vào vị trí đích)")]
    public float followSpeed = 14f;

    [Header("Hover")]
    public float hoverScale = 1.08f;
    public float hoverMoveY = 40f;

    [Header("Selected")]
    public float selectedScale = 1.10f;
    public float selectedMoveY = 40f;

    [Header("Play - Bay vào giữa (Toạ độ trong HandContainer)")]
    public Vector2 playTargetPosition = new Vector2(0f, 170f);
    public float playScale = 0.85f;
    public float playFadeAlpha = 0.8f;
    public float playDuration = 0.30f;
    public float playHoldTime = 0.15f;

    [Header("Discard - Bay ra xa và mờ dần")]
    public Vector2 discardTargetPosition = new Vector2(900f, 200f);
    public float discardScale = 0.5f;
    public float discardDuration = 0.35f;

    public CardVisualState State { get; private set; }

    public float PlayTotalTime
    {
        get { return playDuration + playHoldTime; }
    }

    private RectTransform rect;
    private CanvasGroup canvasGroup;

    private Vector2 restPosition;
    private int slotIndex;
    private bool placed;
    private bool locked;

    private Vector2 targetPosition;
    private Vector3 targetScale = Vector3.one;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Mở rộng vùng bắt chuột xuống phía dưới để Card không bị nhấp nháy
        // (Enter/Exit liên tục) khi chuột nằm ở mép dưới lúc Card vừa nâng lên.
        Image rootImage = GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.raycastPadding = new Vector4(0f, -(hoverMoveY + 10f), 0f, 0f);
        }
    }

    private void Update()
    {
        float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPosition, t);
        rect.localScale = Vector3.Lerp(rect.localScale, targetScale, t);
    }

    // ---------- Slot trong Hand ----------

    public void SetSlot(Vector2 position, int index)
    {
        restPosition = position;
        slotIndex = index;

        if (!placed)
        {
            placed = true;
            rect.anchoredPosition = position;
            targetPosition = position;
        }

        ApplyState();
    }

    // ---------- State: Normal / Hover / Selected ----------

    public void SetState(CardVisualState newState)
    {
        State = newState;
        ApplyState();
    }

    private void ApplyState()
    {
        if (locked) return;

        switch (State)
        {
            case CardVisualState.Hover:
                targetPosition = restPosition + new Vector2(0f, hoverMoveY);
                targetScale = Vector3.one * hoverScale;
                break;

            case CardVisualState.Selected:
                targetPosition = restPosition + new Vector2(0f, selectedMoveY);
                targetScale = Vector3.one * selectedScale;
                break;

            default:
                targetPosition = restPosition;
                targetScale = Vector3.one;
                break;
        }
    }

    public void BringToFront()
    {
        rect.SetAsLastSibling();
    }

    public void RestoreSiblingOrder()
    {
        rect.SetSiblingIndex(slotIndex);
    }

    // ---------- Draw: Scale 0 -> 1 ----------

    public void PlayDrawIn(float delay)
    {
        StartCoroutine(DrawInRoutine(delay));
    }

    private IEnumerator DrawInRoutine(float delay)
    {
        locked = true;
        rect.localScale = Vector3.zero;
        targetScale = Vector3.zero;

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        locked = false;
        ApplyState();
    }

    // ---------- Play: Bay vào giữa ----------

    public void BeginPlay()
    {
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        locked = true;
        State = CardVisualState.Playing;
        canvasGroup.blocksRaycasts = false;
        rect.SetAsLastSibling();

        targetPosition = playTargetPosition;
        targetScale = Vector3.one * playScale;

        yield return StartCoroutine(FadeTo(playFadeAlpha, playDuration));
    }

    // ---------- Discard: Bay ra xa, mờ dần, xoá ----------

    public void BeginDiscard()
    {
        StartCoroutine(DiscardRoutine());
    }

    private IEnumerator DiscardRoutine()
    {
        locked = true;
        State = CardVisualState.Discarding;
        canvasGroup.blocksRaycasts = false;

        targetPosition = discardTargetPosition;
        targetScale = Vector3.one * discardScale;

        yield return StartCoroutine(FadeTo(0f, discardDuration));

        Destroy(gameObject);
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;

        if (duration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}