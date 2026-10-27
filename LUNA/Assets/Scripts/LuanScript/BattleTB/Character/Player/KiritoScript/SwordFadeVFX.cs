using System.Collections;
using UnityEngine;

public class SwordFadeVFX : MonoBehaviour
{
    [Header("Sword Sprites")]
    [SerializeField] private SpriteRenderer lightSword;
    [SerializeField] private SpriteRenderer darkSword;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float transitionDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private Coroutine currentEffect;

    private void Awake()
    {
        SetAlpha(lightSword, 0f);
        SetAlpha(darkSword, 0f);
    }

    // Event 1: Bắt đầu triệu hồi kiếm
    public void StartSwordVFX()
    {
        if (lightSword == null || darkSword == null)
            return;

        if (currentEffect != null)
            StopCoroutine(currentEffect);

        currentEffect = StartCoroutine(ShowSword());
    }

    private IEnumerator ShowSword()
    {
        SetAlpha(lightSword, 0f);
        SetAlpha(darkSword, 0f);

        // Giai đoạn 1: Kiếm xanh nhạt xuất hiện
        yield return Fade(
            lightSword, 0f, 1f, fadeInDuration
        );

        // Giai đoạn 2: Chuyển từ xanh nhạt sang xanh đậm
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;

            float t = transitionDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / transitionDuration);

            SetAlpha(lightSword, 1f - t);
            SetAlpha(darkSword, t);

            yield return null;
        }

        SetAlpha(lightSword, 0f);
        SetAlpha(darkSword, 1f);

        currentEffect = null;
    }

    // Event 2: Kiếm bắt đầu biến mất
    public void EndSwordVFX()
    {
        if (lightSword == null || darkSword == null)
            return;

        if (currentEffect != null)
            StopCoroutine(currentEffect);

        currentEffect = StartCoroutine(HideSword());
    }

    private IEnumerator HideSword()
    {
        float lightStart = lightSword.color.a;
        float darkStart = darkSword.color.a;

        float elapsed = 0f;

        if (fadeOutDuration <= 0f)
        {
            SetAlpha(lightSword, 0f);
            SetAlpha(darkSword, 0f);
            currentEffect = null;
            yield break;
        }

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / fadeOutDuration);

            SetAlpha(lightSword, Mathf.Lerp(lightStart, 0f, t));
            SetAlpha(darkSword, Mathf.Lerp(darkStart, 0f, t));

            yield return null;
        }

        SetAlpha(lightSword, 0f);
        SetAlpha(darkSword, 0f);

        currentEffect = null;
    }

    private IEnumerator Fade(
        SpriteRenderer renderer,
        float from,
        float to,
        float duration)
    {
        if (duration <= 0f)
        {
            SetAlpha(renderer, to);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            SetAlpha(renderer, Mathf.Lerp(from, to, t));

            yield return null;
        }

        SetAlpha(renderer, to);
    }

    private void SetAlpha(SpriteRenderer renderer, float alpha)
    {
        if (renderer == null)
            return;

        Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }
}