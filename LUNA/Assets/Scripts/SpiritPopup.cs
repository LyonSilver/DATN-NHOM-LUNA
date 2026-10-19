using UnityEngine;
using System.Collections;

public class SpiritPopup : MonoBehaviour
{
    public GameObject spirit;
    public float showTime = 3f;
    public float fadeSpeed = 6f;

    private CanvasGroup cg;
    private Coroutine routine;

    void Awake()
    {
        if (spirit == null) return;
        cg = spirit.GetComponent<CanvasGroup>() ?? spirit.AddComponent<CanvasGroup>();
        spirit.SetActive(false);
    }

    public void Show()
    {
        if (spirit == null) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        spirit.SetActive(true);
        cg.alpha = 0f;

        while (cg.alpha < 1f)
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, 1f, Time.unscaledDeltaTime * fadeSpeed);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(showTime);

        while (cg.alpha > 0f)
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, 0f, Time.unscaledDeltaTime * fadeSpeed);
            yield return null;
        }

        spirit.SetActive(false);
    }
}