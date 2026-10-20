using UnityEngine;
using System.Collections;

public class RadialMenuController : MonoBehaviour
{
    [Header("References")]
    public GameObject radialMenu;

    [Header("Animation")]
    public float openDuration = 0.32f;
    public float closeDuration = 0.22f;
    public float openScaleFrom = 0.6f;
    public float closeScaleTo = 0.75f;
    public AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve closeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CanvasGroup canvasGroup;
    private RectTransform rect;
    private bool isOpen;
    private Coroutine currentAnim;

    void Awake()
    {
        if (radialMenu == null) return;

        canvasGroup = radialMenu.GetComponent<CanvasGroup>() ?? radialMenu.AddComponent<CanvasGroup>();
        rect = radialMenu.GetComponent<RectTransform>();
        radialMenu.SetActive(false);
        isOpen = false;
    }

    public void ToggleMenu()
    {
        if (isOpen) CloseMenu();
        else OpenMenu();
    }

    public void OpenMenu()
    {
        if (isOpen || radialMenu == null) return;
        if (currentAnim != null) StopCoroutine(currentAnim);

        isOpen = true;
        radialMenu.SetActive(true);
        currentAnim = StartCoroutine(AnimateOpen());
    }

    public void CloseMenu()
    {
        if (!isOpen || radialMenu == null) return;
        if (currentAnim != null) StopCoroutine(currentAnim);

        isOpen = false;
        currentAnim = StartCoroutine(AnimateClose());
    }

    IEnumerator AnimateOpen()
    {
        float time = 0f;
        Vector3 startScale = Vector3.one * openScaleFrom;
        Vector3 endScale = Vector3.one;

        while (time < openDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / openDuration);
            float curve = openCurve.Evaluate(t);

            rect.localScale = Vector3.LerpUnclamped(startScale, endScale, curve);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, curve);

            yield return null;
        }

        rect.localScale = endScale;
        canvasGroup.alpha = 1f;
        currentAnim = null;
    }

    IEnumerator AnimateClose()
    {
        float time = 0f;
        Vector3 startScale = rect.localScale;
        Vector3 endScale = Vector3.one * closeScaleTo;

        while (time < closeDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / closeDuration);
            float curve = closeCurve.Evaluate(t);

            rect.localScale = Vector3.LerpUnclamped(startScale, endScale, curve);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, curve);

            yield return null;
        }

        rect.localScale = endScale;
        canvasGroup.alpha = 0f;
        radialMenu.SetActive(false);
        currentAnim = null;
    }
}