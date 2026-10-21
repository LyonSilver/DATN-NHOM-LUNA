
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

[RequireComponent(typeof(CanvasGroup))]
public class CardDissolveUI : MonoBehaviour
{
    [Header("Dissolve Material")]
    [SerializeField] private Material dissolveMaterial;

    [Header("Shader Settings")]
    [SerializeField] private string dissolveProperty = "_Dissolve";

    [SerializeField, Min(0.05f)]
    private float dissolveDuration = 0.6f;

    [Header("Float Dissolve")]
    [SerializeField] private float startValue = 0f;
    [SerializeField] private float endValue = 1f;

    [Header("Vector Dissolve (Optional)")]
    [SerializeField] private Vector3 startOffset =
        new Vector3(0f, -0.5f, 0f);

    [SerializeField] private Vector3 endOffset =
        new Vector3(0f, 1.1f, 0f);

    [Header("Text / Other Graphics")]
    [SerializeField] private bool fadeOtherGraphics = true;

    [Range(0.1f, 1f)]
    [SerializeField] private float otherGraphicsFadeEnd = 0.65f;

    private CanvasGroup canvasGroup;

    private Image[] cardImages;
    private Material[] originalMaterials;

    private Graphic[] otherGraphics;
    private Color[] originalGraphicColors;

    private Material runtimeMaterial;
    private ShaderPropertyType propertyType;

    private float originalAlpha;
    private bool originalInteractable;
    private bool originalBlocksRaycasts;

    private bool hasSavedState;
    private bool cancelRequested;

    public bool IsPlaying { get; private set; }
    public bool LastPlaySucceeded { get; private set; }
    public bool UsedShaderDissolve { get; private set; }

    // =========================================
    // AWAKE
    // =========================================

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // =========================================
    // PLAY DISSOLVE
    // =========================================

    public IEnumerator PlayDissolve()
    {
        if (IsPlaying)
            yield break;

        LastPlaySucceeded = false;
        UsedShaderDissolve = false;
        cancelRequested = false;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Kiểm tra Material trước khi chạy.
        if (!ValidateShader())
        {
            Debug.LogError(
                "CardDissolveUI: Shader không hợp lệ. " +
                "Không thể thực hiện Dissolve!"
            );

            yield break;
        }

        // =====================================
        // 1. SAVE CARD STATE
        // =====================================

        SaveOriginalState();

        // =====================================
        // 2. PREPARE MATERIAL
        // =====================================

        PrepareRuntimeMaterial();

        IsPlaying = true;
        UsedShaderDissolve = true;

        // Không cho click Card lần nữa.
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // =====================================
        // 3. START DISSOLVE
        // =====================================

        SetShaderProgress(0f);

        Debug.Log(
            "CardDissolveUI: Bắt đầu Shader Dissolve!"
        );

        float elapsed = 0f;

        float duration = Mathf.Max(
            0.05f,
            dissolveDuration
        );

        while (elapsed < duration)
        {
            if (cancelRequested)
                yield break;

            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / duration
            );

            // =================================
            // SHADER DISSOLVE
            // =================================

            SetShaderProgress(progress);

            // KHÔNG giảm CanvasGroup.alpha ở đây.
            // Để Shader tự cắt hình theo Noise.
            canvasGroup.alpha = originalAlpha;

            // =================================
            // TEXT / OTHER GRAPHICS
            // =================================

            if (fadeOtherGraphics)
            {
                float fadeProgress = Mathf.Clamp01(
                    progress /
                    Mathf.Max(0.01f, otherGraphicsFadeEnd)
                );

                FadeOtherGraphics(fadeProgress);
            }

            yield return null;
        }

        if (cancelRequested)
            yield break;

        // =====================================
        // 4. COMPLETE DISSOLVE
        // =====================================

        SetShaderProgress(1f);

        if (fadeOtherGraphics)
            FadeOtherGraphics(1f);

        // Chỉ ẩn toàn bộ Card khi đã tan xong.
        canvasGroup.alpha = 0f;

        LastPlaySucceeded = true;
        IsPlaying = false;

        Debug.Log(
            "CardDissolveUI: Dissolve hoàn tất!"
        );
    }

    // =========================================
    // VALIDATE SHADER
    // =========================================

    private bool ValidateShader()
    {
        if (dissolveMaterial == null)
        {
            Debug.LogError(
                "CardDissolveUI: Chưa gán Dissolve Material!"
            );
            return false;
        }

        Shader shader = dissolveMaterial.shader;

        if (shader == null)
            return false;

        if (!dissolveMaterial.HasProperty("_MainTex"))
        {
            Debug.LogError(
                "CardDissolveUI: Shader thiếu _MainTex!"
            );
            return false;
        }

        if (!dissolveMaterial.HasProperty(dissolveProperty))
        {
            Debug.LogError(
                "CardDissolveUI: Không tìm thấy Property: " +
                dissolveProperty
            );
            return false;
        }

        // Kiểm tra đúng kiểu dữ liệu.
        for (int i = 0; i < shader.GetPropertyCount(); i++)
        {
            if (shader.GetPropertyName(i) != dissolveProperty)
                continue;

            ShaderPropertyType type =
                shader.GetPropertyType(i);

            if (type == ShaderPropertyType.Float ||
                type == ShaderPropertyType.Range ||
                type == ShaderPropertyType.Vector)
            {
                propertyType = type;
                return true;
            }

            Debug.LogError(
                "CardDissolveUI: Property không phải " +
                "Float, Range hoặc Vector!"
            );

            return false;
        }

        return false;
    }

    // =========================================
    // SAVE ORIGINAL STATE
    // =========================================

    private void SaveOriginalState()
    {
        originalAlpha = canvasGroup.alpha;

        originalInteractable =
            canvasGroup.interactable;

        originalBlocksRaycasts =
            canvasGroup.blocksRaycasts;

        // Các Image sẽ dùng Shader Dissolve.
        cardImages =
            GetComponentsInChildren<Image>(true);

        originalMaterials =
            new Material[cardImages.Length];

        for (int i = 0; i < cardImages.Length; i++)
        {
            originalMaterials[i] =
                cardImages[i].material;
        }

        // Các Graphic khác (ví dụ TMP)
        // sẽ được làm mờ riêng.
        Graphic[] graphics =
            GetComponentsInChildren<Graphic>(true);

        List<Graphic> otherList =
            new List<Graphic>();

        foreach (Graphic graphic in graphics)
        {
            if (graphic == null)
                continue;

            // Image đã có Shader riêng.
            if (graphic is Image)
                continue;

            otherList.Add(graphic);
        }

        otherGraphics = otherList.ToArray();

        originalGraphicColors =
            new Color[otherGraphics.Length];

        for (int i = 0; i < otherGraphics.Length; i++)
        {
            originalGraphicColors[i] =
                otherGraphics[i].color;
        }

        hasSavedState = true;
    }

    // =========================================
    // PREPARE RUNTIME MATERIAL
    // =========================================

    private void PrepareRuntimeMaterial()
    {
        // Mỗi Card có một Material riêng.
        // Không làm các Card khác tan theo.
        runtimeMaterial = new Material(dissolveMaterial);

        runtimeMaterial.name =
            dissolveMaterial.name + "_Runtime";

        for (int i = 0; i < cardImages.Length; i++)
        {
            if (cardImages[i] == null)
                continue;

            cardImages[i].material = runtimeMaterial;
            cardImages[i].SetMaterialDirty();
        }
    }

    // =========================================
    // UPDATE SHADER DISSOLVE
    // =========================================

    private void SetShaderProgress(float progress)
    {
        if (runtimeMaterial == null)
            return;

        progress = Mathf.Clamp01(progress);

        // =====================================
        // FLOAT / RANGE
        // =====================================

        if (propertyType == ShaderPropertyType.Float ||
            propertyType == ShaderPropertyType.Range)
        {
            float value = Mathf.Lerp(
                startValue,
                endValue,
                progress
            );

            runtimeMaterial.SetFloat(
                dissolveProperty,
                value
            );
        }

        // =====================================
        // VECTOR
        // =====================================

        else if (propertyType == ShaderPropertyType.Vector)
        {
            Vector3 offset = Vector3.Lerp(
                startOffset,
                endOffset,
                progress
            );

            runtimeMaterial.SetVector(
                dissolveProperty,
                new Vector4(
                    offset.x,
                    offset.y,
                    offset.z,
                    0f
                )
            );
        }
    }

    // =========================================
    // FADE TEXT AND OTHER GRAPHICS
    // =========================================

    private void FadeOtherGraphics(float progress)
    {
        if (otherGraphics == null ||
            originalGraphicColors == null)
        {
            return;
        }

        progress = Mathf.Clamp01(progress);

        for (int i = 0; i < otherGraphics.Length; i++)
        {
            Graphic graphic = otherGraphics[i];

            if (graphic == null)
                continue;

            Color original =
                originalGraphicColors[i];

            original.a *= (1f - progress);

            graphic.color = original;
        }
    }

    // =========================================
    // RESET DISSOLVE
    // =========================================

    public void ResetDissolve()
    {
        cancelRequested = true;

        if (hasSavedState)
        {
            // =================================
            // RESTORE IMAGE MATERIALS
            // =================================

            if (cardImages != null &&
                originalMaterials != null)
            {
                for (int i = 0; i < cardImages.Length; i++)
                {
                    if (cardImages[i] == null)
                        continue;

                    cardImages[i].material =
                        originalMaterials[i];

                    cardImages[i].SetMaterialDirty();
                }
            }

            // =================================
            // RESTORE OTHER GRAPHICS
            // =================================

            if (otherGraphics != null &&
                originalGraphicColors != null)
            {
                for (int i = 0; i < otherGraphics.Length; i++)
                {
                    if (otherGraphics[i] == null)
                        continue;

                    otherGraphics[i].color =
                        originalGraphicColors[i];
                }
            }

            // =================================
            // RESTORE CANVAS GROUP
            // =================================

            if (canvasGroup != null)
            {
                canvasGroup.alpha = originalAlpha;

                canvasGroup.interactable =
                    originalInteractable;

                canvasGroup.blocksRaycasts =
                    originalBlocksRaycasts;
            }
        }

        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
            runtimeMaterial = null;
        }

        hasSavedState = false;

        IsPlaying = false;
        LastPlaySucceeded = false;
        UsedShaderDissolve = false;
    }

    // =========================================
    // CLEANUP
    // =========================================

    private void OnDisable()
    {
        ResetDissolve();
    }

    private void OnDestroy()
    {
        ResetDissolve();
    }
}
