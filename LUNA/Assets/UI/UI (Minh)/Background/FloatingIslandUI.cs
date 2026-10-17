using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FloatingIslandUI : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float floatHeight = 15f;
    [SerializeField] private float floatSpeed = 0.7f;
    [SerializeField] private float startOffset = 0f;

    private RectTransform rectTransform;
    private Vector2 startPosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // Lưu vị trí ban đầu
        startPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // Tạo chuyển động lên xuống mượt mà
        float offsetY = Mathf.Sin(
            Time.unscaledTime * floatSpeed + startOffset
        ) * floatHeight;

        // Giữ nguyên trục X, chỉ thay đổi trục Y
        rectTransform.anchoredPosition =
            startPosition + new Vector2(0f, offsetY);
    }
}