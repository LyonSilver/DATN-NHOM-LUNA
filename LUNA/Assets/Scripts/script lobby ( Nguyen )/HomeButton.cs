using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class HomeButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Menu")]
    public RadialMenuController menuController;

    [Header("Events")]
    public UnityEvent onClick;

    [Header("Press Feel")]
    public float pressScale = 0.9f;
    public float pressSpeed = 18f;

    [Header("Protection")]
    public float clickCooldown = 0.18f;

    private bool canClick = true;
    private Vector3 originalScale;
    private Coroutine scaleRoutine;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        AnimateScale(originalScale * pressScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        AnimateScale(originalScale);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!canClick) return;

        canClick = false;
        onClick?.Invoke();

        if (menuController != null)
            menuController.ToggleMenu();

        Invoke(nameof(Unlock), clickCooldown);
    }

    void Unlock() => canClick = true;

    void AnimateScale(Vector3 target)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleRoutine(target));
    }

    IEnumerator ScaleRoutine(Vector3 target)
    {
        while (Vector3.Distance(transform.localScale, target) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime * pressSpeed);
            yield return null;
        }
        transform.localScale = target;
    }

    void OnDisable()
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        transform.localScale = originalScale;
    }
}