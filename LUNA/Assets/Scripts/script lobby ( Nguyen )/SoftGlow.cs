using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SoftGlow : MonoBehaviour
{
    public Image targetImage;
    public Color glowColor = new Color(0.6f, 0.85f, 1f, 1f);
    public float speed = 1.8f;
    public float minIntensity = 0.65f;
    public float maxIntensity = 1f;

    private Color originalColor;
    private Coroutine glowRoutine;

    void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (targetImage != null)
            originalColor = targetImage.color;
    }

    void OnEnable()
    {
        if (glowRoutine != null) StopCoroutine(glowRoutine);
        glowRoutine = StartCoroutine(GlowLoop());
    }

    void OnDisable()
    {
        if (glowRoutine != null) StopCoroutine(glowRoutine);
        if (targetImage != null)
            targetImage.color = originalColor;
    }

    IEnumerator GlowLoop()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f;
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

            if (targetImage != null)
                targetImage.color = Color.Lerp(originalColor, glowColor, intensity * 0.4f) * intensity;

            yield return null;
        }
    }
}