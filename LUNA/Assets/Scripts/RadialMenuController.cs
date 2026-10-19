using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class RadialMenuController : MonoBehaviour
{
    public GameObject radialMenu;
    public float animDuration = 0.28f;
    public float pressScale = 0.92f;
    public float pressDuration = 0.12f;

    private CanvasGroup canvasGroup;
    private RectTransform rect;
    private bool isOpen = false;

    void Awake()
    {
        canvasGroup = radialMenu.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = radialMenu.AddComponent<CanvasGroup>();

        rect = radialMenu.GetComponent<RectTransform>();
        radialMenu.SetActive(false);

        // Tự gắn hiệu ứng nhấn cho tất cả nút con
        AddPressEffectToChildren();
    }

    void AddPressEffectToChildren()
    {
        foreach (Transform child in radialMenu.transform)
        {
            var btn = child.GetComponent<UnityEngine.UI.Button>();
            if (btn == null) continue;

            var trigger = child.gameObject.GetComponent<EventTrigger>() ?? child.gameObject.AddComponent<EventTrigger>();

            // Pointer Down
            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener((data) => StartCoroutine(PressAnim(child, true)));
            trigger.triggers.Add(down);

            // Pointer Up
            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener((data) => StartCoroutine(PressAnim(child, false)));
            trigger.triggers.Add(up);
        }
    }

    IEnumerator PressAnim(Transform target, bool pressing)
    {
        Vector3 start = target.localScale;
        Vector3 end = pressing ? Vector3.one * pressScale : Vector3.one;
        float t = 0f;

        while (t < pressDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.SmoothStep(0f, 1f, t / pressDuration);
            target.localScale = Vector3.Lerp(start, end, lerp);
            yield return null;
        }
        target.localScale = end;
    }

    public void ToggleMenu()
    {
        if (isOpen) CloseMenu();
        else OpenMenu();
    }

    public void OpenMenu()
    {
        if (isOpen) return;
        StopAllCoroutines();
        isOpen = true;
        radialMenu.SetActive(true);
        StartCoroutine(AnimateMenu(true));
    }

    public void CloseMenu()
    {
        if (!isOpen) return;
        StopAllCoroutines();
        isOpen = false;
        StartCoroutine(AnimateMenu(false));
    }

    IEnumerator AnimateMenu(bool open)
    {
        float time = 0f;
        float startScale = open ? 0.65f : 1f;
        float endScale = open ? 1f : 0.65f;
        float startAlpha = open ? 0f : 1f;
        float endAlpha = open ? 1f : 0f;

        while (time < animDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / animDuration);

            float scale = Mathf.Lerp(startScale, endScale, t);
            rect.localScale = Vector3.one * scale;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }

        rect.localScale = Vector3.one * endScale;
        canvasGroup.alpha = endAlpha;

        if (!open)
            radialMenu.SetActive(false);
    }
}