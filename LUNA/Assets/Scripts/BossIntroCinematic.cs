
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossIntroCinematic : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator explosionAnimator;
    [SerializeField] private string explosionState = "Explosion";
    [SerializeField] private GameObject bossMapRoot;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Image fadeImage;
    [SerializeField] private SpriteRenderer shockwave;

    [Header("Timing")]
    [Min(0)] public float impactTime = 1.3f;
    [Min(0)] public float mapRevealTime = 2.4f;
    [Min(0.01f)] public float fadeDuration = 1.1f;

    [Header("Camera Shake")]
    [Min(0)] public float shakeDuration = 0.28f;
    [Min(0)] public float shakeStrength = 0.18f;

    [Header("Shockwave")]
    public float shockwaveDuration = 0.55f;
    public float shockwaveMaxScale = 5f;

    [Header("Optional")]
    public AudioSource explosionSound;
    public bool playOnStart = true;

    private Coroutine sequence;
    private Coroutine shakeRoutine;
    private Vector3 lastShakeOffset;
    private bool running;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (bossMapRoot != null)
            bossMapRoot.SetActive(false);

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            SetFade(0f);
        }

        if (shockwave != null)
            shockwave.gameObject.SetActive(false);

        if (explosionAnimator != null)
            explosionAnimator.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (playOnStart)
            PlayIntro();
    }

    public void PlayIntro()
    {
        if (running) return;
        sequence = StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        running = true;

        if (bossMapRoot != null)
            bossMapRoot.SetActive(false);

        SetFade(0f);

        if (explosionAnimator != null)
        {
            explosionAnimator.gameObject.SetActive(true);
            explosionAnimator.Play(explosionState, 0, 0f);
            explosionAnimator.Update(0f);
        }

        yield return new WaitForSecondsRealtime(impactTime);

        if (explosionSound != null)
            explosionSound.Play();

        if (targetCamera != null)
            shakeRoutine = StartCoroutine(CameraShake());

        if (shockwave != null)
            StartCoroutine(AnimateShockwave());

        float remaining = Mathf.Max(0f,
            mapRevealTime - impactTime);

        yield return new WaitForSecondsRealtime(remaining);

        // Che màn hình trước khi kích hoạt map.
        SetFade(1f);

        if (explosionAnimator != null)
            explosionAnimator.gameObject.SetActive(false);

        if (shockwave != null)
            shockwave.gameObject.SetActive(false);

        if (bossMapRoot != null)
            bossMapRoot.SetActive(true);

        // Hiện map bằng Fade In.
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            t = t * t * (3f - 2f * t);

            SetFade(1f - t);
            yield return null;
        }

        SetFade(0f);
        running = false;
        sequence = null;
    }

    private IEnumerator CameraShake()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            // Loại offset cũ để không làm trôi camera.
            targetCamera.transform.localPosition -= lastShakeOffset;

            float fade = 1f - elapsed /
                Mathf.Max(0.001f, shakeDuration);

            Vector2 random = Random.insideUnitCircle *
                shakeStrength * fade;

            lastShakeOffset = new Vector3(
                random.x, random.y, 0f);

            targetCamera.transform.localPosition += lastShakeOffset;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        RestoreCamera();
        shakeRoutine = null;
    }

    private IEnumerator AnimateShockwave()
    {
        shockwave.gameObject.SetActive(true);

        Color baseColor = shockwave.color;
        float elapsed = 0f;

        while (elapsed < shockwaveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, shockwaveDuration));

            float scale = Mathf.Lerp(0.25f,
                shockwaveMaxScale, 1f - (1f - t) * (1f - t));

            shockwave.transform.localScale =
                Vector3.one * scale;

            Color c = baseColor;
            c.a = baseColor.a * (1f - t);
            shockwave.color = c;

            yield return null;
        }

        shockwave.color = baseColor;
        shockwave.gameObject.SetActive(false);
    }

    private void SetFade(float alpha)
    {
        if (fadeImage == null) return;

        Color color = fadeImage.color;
        color.a = Mathf.Clamp01(alpha);
        fadeImage.color = color;
    }

    private void RestoreCamera()
    {
        if (targetCamera != null)
            targetCamera.transform.localPosition -= lastShakeOffset;

        lastShakeOffset = Vector3.zero;
    }

    private void OnDisable()
    {
        RestoreCamera();
        running = false;
    }
}
