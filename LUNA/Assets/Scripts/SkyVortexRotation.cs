using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class SkyVortexRotation : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField, Min(0f)] private float degreesPerSecond = 2f;
    [SerializeField] private bool clockwise = true;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Breathing")]
    [SerializeField] private bool breathing = true;
    [SerializeField, Range(0f, 0.1f)] private float breathingAmount = 0.015f;
    [SerializeField, Min(0f)] private float breathingCyclesPerSecond = 0.12f;

    [Header("Opacity")]
    [SerializeField] private bool pulseOpacity = false;
    [SerializeField, Range(0f, 1f)] private float baseOpacity = 0.85f;
    [SerializeField, Range(0f, 0.5f)] private float opacityAmplitude = 0.05f;

    private SpriteRenderer spriteRenderer;
    private Vector3 initialScale;
    private float elapsed;
    private float initialAlpha;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;
        initialAlpha = spriteRenderer.color.a;
    }

    private void OnEnable()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;
        initialAlpha = spriteRenderer.color.a;
        elapsed = 0f;
    }

    private void Update()
    {
        float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        elapsed += delta;
        float sign = clockwise ? -1f : 1f;
        transform.Rotate(0f, 0f, sign * degreesPerSecond * delta, Space.Self);

        float wave = Mathf.Sin(elapsed * breathingCyclesPerSecond * Mathf.PI * 2f);
        transform.localScale = breathing ? initialScale * (1f + breathingAmount * wave) : initialScale;

        Color color = spriteRenderer.color;
        color.a = pulseOpacity ? Mathf.Clamp01(baseOpacity + opacityAmplitude * wave) : baseOpacity;
        spriteRenderer.color = color;
    }

    private void OnDisable()
    {
        transform.localScale = initialScale;
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = initialAlpha;
            spriteRenderer.color = color;
        }
    }

    private void OnValidate()
    {
        degreesPerSecond = Mathf.Max(0f, degreesPerSecond);
        breathingCyclesPerSecond = Mathf.Max(0f, breathingCyclesPerSecond);
    }
}
