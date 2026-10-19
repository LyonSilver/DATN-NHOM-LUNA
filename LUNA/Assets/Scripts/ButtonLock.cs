
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonLock : MonoBehaviour
{
    [Header("Lock Settings")]
    [SerializeField] private bool isLocked = true;

    [Header("Target Image")]
    [SerializeField] private Image targetImage;

    [Header("Lock Icon")]
    [SerializeField] private Sprite lockSprite;
    [SerializeField] private Vector2 iconSize =
        new Vector2(60f, 60f);

    [Header("Dark Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float darkness = 0.6f;

    private Button button;
    private Image lockIconImage;
    private Color originalColor;
    private Selectable.Transition originalTransition;

    private void Awake()
    {
        button = GetComponent<Button>();

        // Tự lấy Image của Button nếu chưa gán
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (targetImage == null)
            targetImage = button.targetGraphic as Image;

        // Lưu màu gốc của Image
        if (targetImage != null)
            originalColor = targetImage.color;

        originalTransition = button.transition;

        CreateLockIcon();
        UpdateLockState();
    }

    private void CreateLockIcon()
    {
        GameObject iconObject = new GameObject(
            "LockIcon",
            typeof(RectTransform),
            typeof(Image)
        );

        iconObject.transform.SetParent(transform, false);

        RectTransform rect =
            iconObject.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = iconSize;

        lockIconImage = iconObject.GetComponent<Image>();
        lockIconImage.sprite = lockSprite;
        lockIconImage.preserveAspect = true;
        lockIconImage.raycastTarget = false;

        iconObject.transform.SetAsLastSibling();
    }

    private void UpdateLockState()
    {
        if (button == null)
            return;

        // Ngăn Button Transition đổi màu Image khi khóa
        if (isLocked)
            button.transition = Selectable.Transition.None;

        button.interactable = !isLocked;

        if (!isLocked)
            button.transition = originalTransition;

        // Làm tối trực tiếp Image
        if (targetImage != null)
        {
            if (isLocked)
            {
                float brightness = 1f - darkness;

                targetImage.color = new Color(
                    originalColor.r * brightness,
                    originalColor.g * brightness,
                    originalColor.b * brightness,
                    originalColor.a
                );
            }
            else
            {
                targetImage.color = originalColor;
            }
        }

        // Hiển thị icon khóa
        if (lockIconImage != null)
        {
            lockIconImage.sprite = lockSprite;
            lockIconImage.rectTransform.sizeDelta = iconSize;

            lockIconImage.gameObject.SetActive(
                isLocked && lockSprite != null
            );
        }
    }

    public void Lock()
    {
        isLocked = true;
        UpdateLockState();
    }

    public void Unlock()
    {
        isLocked = false;
        UpdateLockState();
    }

    public void SetLock(bool locked)
    {
        isLocked = locked;
        UpdateLockState();
    }
}
