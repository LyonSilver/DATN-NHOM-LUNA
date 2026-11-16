using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ChaosBossIntro
{
    [DisallowMultipleComponent]
    public sealed class ChaosBossIntroController : MonoBehaviour
    {
        [Header("Environment")]
        public SpriteRenderer sky;
        public SpriteRenderer vortex;
        public SpriteRenderer ruins;
        public SpriteRenderer arena;
        public SpriteRenderer explosion;
        public Transform lightningOrigin;
        public Transform lightningTarget;
        public LineRenderer lightning;
        [Header("Gameplay objects (optional)")]
        public GameObject player;
        public GameObject boss;
        public GameObject cardHud;
        public UnityEvent onIntroComplete;
        [Header("Timing (seconds)")]
        [Min(0)] public float skyHold = 1.2f;
        [Min(0.01f)] public float lightningDuration = 0.55f;
        [Min(0.01f)] public float explosionDuration = 0.8f;
        [Min(0.01f)] public float mapFadeDuration = 1.5f;
        [Min(0)] public float finishHold = 0.3f;
        [Header("Look")]
        public float vortexDegreesPerSecond = -2f;
        [Range(0, 1)] public float lightningAlpha = 0.75f;
        [Range(0, 1)] public float explosionAlpha = 0.65f;
        public float lightningWidth = 0.10f;
        public int lightningSegments = 13;
        public float lightningJitter = 0.22f;
        public bool playOnStart = true;
        public bool useUnscaledTime = false;

        Coroutine running;
        bool finished;
        float clock;
        Color ruinsBase, arenaBase, explosionBase;
        Vector3 explosionScale;
        Material lineMaterial;

        void Awake()
        {
            if (ruins) ruinsBase = ruins.color;
            if (arena) arenaBase = arena.color;
            if (explosion) { explosionBase = explosion.color; explosionScale = explosion.transform.localScale; }
            if (lightning) { lineMaterial = lightning.material; lightning.enabled = false; }
            ResetVisuals();
        }
        void Start() { if (playOnStart) PlayIntro(); }
        void Update()
        {
            if (vortex) vortex.transform.Rotate(0, 0, vortexDegreesPerSecond * Delta());
        }
        float Delta() { return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime; }
        public void PlayIntro()
        {
            if (running != null) StopCoroutine(running);
            ResetVisuals();
            running = StartCoroutine(Sequence());
        }
        public void ResetIntro()
        {
            if (running != null) { StopCoroutine(running); running = null; }
            ResetVisuals();
        }
        void ResetVisuals()
        {
            finished = false;
            SetAlpha(ruins, ruinsBase, 0);
            SetAlpha(arena, arenaBase, 0);
            SetAlpha(explosion, explosionBase, 0);
            if (explosion) explosion.transform.localScale = explosionScale;
            if (lightning) lightning.enabled = false;
            if (player) player.SetActive(false);
            if (boss) boss.SetActive(false);
            if (cardHud) cardHud.SetActive(false);
        }
        static void SetAlpha(SpriteRenderer sr, Color original, float multiplier)
        {
            if (!sr) return;
            Color c = original; c.a = original.a * Mathf.Clamp01(multiplier); sr.color = c;
        }
        IEnumerator Wait(float seconds)
        {
            float t = 0;
            while (t < seconds) { t += Delta(); yield return null; }
        }
        IEnumerator Sequence()
        {
            yield return Wait(skyHold);
            if (lightning) lightning.enabled = true;
            clock = 0;
            while (clock < lightningDuration)
            {
                clock += Delta();
                DrawLightning();
                if (lightning) lightning.widthMultiplier = lightningWidth * Mathf.Clamp01(Mathf.Min(clock * 12f, (lightningDuration - clock) * 12f));
                yield return null;
            }
            if (lightning) lightning.enabled = false;
            clock = 0;
            while (clock < explosionDuration)
            {
                clock += Delta();
                float p = Mathf.Clamp01(clock / explosionDuration);
                float strength = Mathf.Sin(p * Mathf.PI);
                SetAlpha(explosion, explosionBase, strength * explosionAlpha);
                if (explosion) explosion.transform.localScale = explosionScale * Mathf.Lerp(0.3f, 2.2f, p);
                yield return null;
            }
            SetAlpha(explosion, explosionBase, 0);
            clock = 0;
            while (clock < mapFadeDuration)
            {
                clock += Delta();
                float p = Mathf.SmoothStep(0, 1, Mathf.Clamp01(clock / mapFadeDuration));
                SetAlpha(ruins, ruinsBase, p);
                SetAlpha(arena, arenaBase, p);
                yield return null;
            }
            SetAlpha(ruins, ruinsBase, 1);
            SetAlpha(arena, arenaBase, 1);
            yield return Wait(finishHold);
            if (player) player.SetActive(true);
            if (boss) boss.SetActive(true);
            if (cardHud) cardHud.SetActive(true);
            finished = true;
            running = null;
            onIntroComplete?.Invoke();
        }
        void DrawLightning()
        {
            if (!lightning || !lightningOrigin || !lightningTarget) return;
            int count = Mathf.Max(3, lightningSegments);
            lightning.positionCount = count;
            Vector3 a = lightningOrigin.position, b = lightningTarget.position;
            Vector3 sideways = Vector3.Cross((b - a).normalized, Vector3.forward);
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / (count - 1);
                float jitter = (i == 0 || i == count - 1) ? 0 : Random.Range(-lightningJitter, lightningJitter);
                lightning.SetPosition(i, Vector3.Lerp(a, b, t) + sideways * jitter);
            }
            Color c = new Color(0.95f, 0.48f, 1f, lightningAlpha);
            lightning.startColor = c; lightning.endColor = c;
        }
        void OnDisable() { if (lightning) lightning.enabled = false; }
        void OnDestroy() { /* LineRenderer owns no runtime-created material here. */ }
    }
}
