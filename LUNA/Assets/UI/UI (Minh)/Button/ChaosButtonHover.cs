using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ChaosButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("UI Components")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private TMP_Text buttonText;

    [Header("Hover Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float idleBrightness = 0.3f;

    [Min(0.01f)]
    [SerializeField] private float fadeDuration = 0.2f;

    private Color originalImageColor;
    private Color originalTextColor;

    private float currentBrightness;
    private float targetBrightness;

    private void Awake()
    {
        // Tự tìm Image
        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }

        // Tự tìm TextMeshPro bên trong Button
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TMP_Text>(true);
        }

        // Lưu màu sắc gốc
        if (buttonImage != null)
        {
            originalImageColor = buttonImage.color;
        }

        if (buttonText != null)
        {
            originalTextColor = buttonText.color;
        }

        // Trạng thái ban đầu: tối
        currentBrightness = idleBrightness;
        targetBrightness = idleBrightness;

        ApplyBrightness(currentBrightness);
    }

    private void Update()
    {
        // Chuyển độ sáng mượt mà
        float speed = (1f - idleBrightness) / fadeDuration;

        currentBrightness = Mathf.MoveTowards(
            currentBrightness,
            targetBrightness,
            speed * Time.unscaledDeltaTime
        );

        ApplyBrightness(currentBrightness);
    }

    // Khi chuột đi vào Button
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetBrightness = 1f;
    }

    // Khi chuột rời Button
    public void OnPointerExit(PointerEventData eventData)
    {
        targetBrightness = idleBrightness;
    }

    private void ApplyBrightness(float brightness)
    {
        // Điều chỉnh độ sáng hình ảnh
        if (buttonImage != null)
        {
            buttonImage.color = DimColor(
                originalImageColor,
                brightness
            );
        }

        // Điều chỉnh độ sáng chữ
        if (buttonText != null)
        {
            buttonText.color = DimColor(
                originalTextColor,
                brightness
            );
        }
    }

    private Color DimColor(Color color, float brightness)
    {
        return new Color(
            color.r * brightness,
            color.g * brightness,
            color.b * brightness,
            color.a
        );
    }
}