using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class CardOutlineGlow : MonoBehaviour
{
    [Header("References")]
    public Image targetImage;

    [Header("Glow Settings")]
    public Color glowColor = new Color(1f, 0.7f, 0.15f, 1f); // màu vàng cam
    public float minIntensity = 0.4f;
    public float maxIntensity = 3.2f;
    public float pulseSpeed = 2.8f;
    public float thickness = 2.8f;

    [Header("State")]
    public bool playOnStart = true;

    private Material mat;
    private bool isPlaying;
    private Coroutine pulseRoutine;

    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");
    private static readonly int GlowIntensityID = Shader.PropertyToID("_GlowIntensity");

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        // Tạo bản sao Material riêng (tránh ảnh hưởng card khác)
        if (targetImage != null && targetImage.material != null)
        {
            mat = Instantiate(targetImage.material);
            targetImage.material = mat;
        }
    }

    private void Start()
    {
        if (playOnStart)
            Play();
        else
            Stop();
    }

    private void OnDestroy()
    {
        if (mat != null)
            Destroy(mat);
    }

    // ===================== PUBLIC =====================

    public void Play()
    {
        if (mat == null) return;

        isPlaying = true;
        mat.SetColor(OutlineColorID, glowColor);
        mat.SetFloat(OutlineThicknessID, thickness);

        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(Pulse());
    }

    public void Stop()
    {
        isPlaying = false;
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }

        if (mat != null)
            mat.SetFloat(GlowIntensityID, 0f);
    }

    public void SetColor(Color color)
    {
        glowColor = color;
        if (mat != null)
            mat.SetColor(OutlineColorID, color);
    }

    public void PlayOnce(float duration = 1.2f)
    {
        StopAllCoroutines();
        StartCoroutine(PlayOnceRoutine(duration));
    }

    // ===================== PULSE =====================

    private IEnumerator Pulse()
    {
        float t = 0f;
        while (isPlaying)
        {
            t += Time.deltaTime * pulseSpeed;
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, (Mathf.Sin(t) + 1f) * 0.5f);
            mat.SetFloat(GlowIntensityID, intensity);
            yield return null;
        }
    }

    private IEnumerator PlayOnceRoutine(float duration)
    {
        if (mat == null) yield break;

        isPlaying = true;
        mat.SetColor(OutlineColorID, glowColor);
        mat.SetFloat(OutlineThicknessID, thickness);

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float intensity = Mathf.Sin(t * Mathf.PI) * maxIntensity;
            mat.SetFloat(GlowIntensityID, intensity);
            yield return null;
        }

        Stop();
    }
}