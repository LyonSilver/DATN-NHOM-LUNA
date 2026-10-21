using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class GradientBackground : MonoBehaviour
{
    public Color top = new Color32(0x0F, 0x08, 0x26, 255);
    public Color middle = new Color32(0x3B, 0x1D, 0x78, 255);
    public Color bottom = new Color32(0x9A, 0x55, 0xC8, 255);
    public float width = 40f;
    public float height = 10.8f;

    Texture2D tex;

    void OnEnable() { Apply(); }
    void OnValidate() { Apply(); }

    void Apply()
    {
        const int h = 256;
        if (tex != null)
        {
            if (Application.isPlaying) Destroy(tex);
            else DestroyImmediate(tex);
        }

        tex = new Texture2D(1, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < h; y++)
        {
            float t = y / (h - 1f);   // 0 = Đáy, 1 = Đỉnh
            Color c = t < 0.5f
                ? Color.Lerp(bottom, middle, t * 2f)
                : Color.Lerp(middle, top, (t - 0.5f) * 2f);
            tex.SetPixel(0, y, c);
        }
        tex.Apply();

        const float ppu = 100f;
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 1, h), new Vector2(0.5f, 0.5f), ppu);
        GetComponent<SpriteRenderer>().sprite = sprite;
        transform.localScale = new Vector3(width / (1f / ppu), height / (h / ppu), 1f);
    }
}