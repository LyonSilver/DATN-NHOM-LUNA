using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SpriteRenderEnhancer : MonoBehaviour
{
    [Header("Sprite targets")]
    [SerializeField] private bool includeChildren = true;
    [Header("Texture smoothing")]
    [SerializeField] private FilterMode filterMode = FilterMode.Bilinear;
    [SerializeField, Range(0, 16)] private int anisoLevel = 1;
    [SerializeField, Range(-2f, 2f)] private float mipMapBias = 0f;
    [Header("Optional color adjustment")]
    [SerializeField] private bool overrideColor = false;
    [SerializeField] private Color spriteColor = Color.white;

    private readonly Dictionary<Texture2D, TextureSettings> originalTextures = new Dictionary<Texture2D, TextureSettings>();
    private readonly Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();

    private struct TextureSettings
    {
        public FilterMode Filter;
        public int Aniso;
        public float Bias;
    }

    private void OnEnable()
    {
        Apply();
    }

    public void Apply()
    {
        SpriteRenderer[] renderers = includeChildren
            ? GetComponentsInChildren<SpriteRenderer>(true)
            : GetComponents<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null) continue;
            if (overrideColor)
            {
                if (!originalColors.ContainsKey(renderer)) originalColors.Add(renderer, renderer.color);
                renderer.color = spriteColor;
            }

            Sprite sprite = renderer.sprite;
            if (sprite == null) continue;
            Texture2D texture = sprite.texture;
            if (texture == null) continue;
            if (!originalTextures.ContainsKey(texture))
            {
                originalTextures.Add(texture, new TextureSettings
                {
                    Filter = texture.filterMode,
                    Aniso = texture.anisoLevel,
                    Bias = texture.mipMapBias
                });
            }
            texture.filterMode = filterMode;
            texture.anisoLevel = Mathf.Clamp(anisoLevel, 0, 16);
            if (texture.mipmapCount > 1) texture.mipMapBias = mipMapBias;
        }
    }

    private void OnDisable()
    {
        // Texture settings are shared by all sprites referencing the same texture.
        // Avoid restoring them here, as that could disrupt another active character.
        // To make changes persistent, configure the texture's Import Settings instead.
        foreach (KeyValuePair<SpriteRenderer, Color> entry in originalColors)
        {
            if (entry.Key != null) entry.Key.color = entry.Value;
        }
        originalColors.Clear();
        originalTextures.Clear();
    }
}
