using UnityEngine;

namespace ChaosBossArena
{
    [DisallowMultipleComponent]
    public sealed class ChaosVortexMotion : MonoBehaviour
    {
        [Range(-15f, 15f)] public float degreesPerSecond = -1.5f;
        [Range(0f, .08f)] public float breatheAmount = .012f;
        [Min(0f)] public float breatheCyclesPerSecond = .12f;
        [Range(0f, 1f)] public float opacity = .78f;
        public bool animateDuringPause = false;
        Vector3 baseScale;
        SpriteRenderer sprite;
        float phase;
        void Awake() { baseScale = transform.localScale; sprite = GetComponent<SpriteRenderer>(); }
        void OnEnable() { baseScale = transform.localScale; sprite = GetComponent<SpriteRenderer>(); }
        void Update()
        {
            float dt = animateDuringPause ? Time.unscaledDeltaTime : Time.deltaTime;
            phase += dt;
            transform.Rotate(0f, 0f, degreesPerSecond * dt, Space.Self);
            float scale = 1f + Mathf.Sin(phase * Mathf.PI * 2f * breatheCyclesPerSecond) * breatheAmount;
            transform.localScale = baseScale * scale;
            if (sprite != null) { Color c = sprite.color; c.a = opacity; sprite.color = c; }
        }
        void OnDisable() { if (baseScale != Vector3.zero) transform.localScale = baseScale; }
    }
}
