
using System.Collections.Generic;
using UnityEngine;

public class IslandDistanceEffect : MonoBehaviour
{
    [System.Serializable]
    public class IslandSetting
    {
        public Transform island;

        [Range(0f, 1f)]
        public float distance = 0.5f;
    }

    [Header("Island Settings")]
    [SerializeField]
    private IslandSetting[] islands;

    [Header("Atmospheric Effect")]
    [SerializeField]
    private Color fogColor = new Color(
        0.78f, 0.87f, 1f, 1f
    );

    [Range(0f, 1f)]
    [SerializeField]
    private float fogStrength = 0.6f;

    [Range(0f, 1f)]
    [SerializeField]
    private float maxFade = 0.4f;

    private readonly Dictionary<SpriteRenderer, Color>
        originalColors = new();

    private void Awake()
    {
        foreach (var entry in islands)
        {
            if (entry.island == null) continue;

            var renderers = entry.island
                .GetComponentsInChildren<SpriteRenderer>(true);

            foreach (var sr in renderers)
            {
                if (!originalColors.ContainsKey(sr))
                    originalColors.Add(sr, sr.color);
            }
        }
    }

    private void Update()
    {
        foreach (var entry in islands)
        {
            if (entry.island == null) continue;

            float distance = entry.distance;

            var renderers = entry.island
                .GetComponentsInChildren<SpriteRenderer>(true);

            foreach (var sr in renderers)
            {
                if (!originalColors.TryGetValue(
                    sr, out Color original))
                    continue;

                Color color = Color.Lerp(
                    original,
                    fogColor,
                    distance * fogStrength
                );

                color.a = original.a *
                    (1f - distance * maxFade);

                sr.color = color;
            }
        }
    }
}
