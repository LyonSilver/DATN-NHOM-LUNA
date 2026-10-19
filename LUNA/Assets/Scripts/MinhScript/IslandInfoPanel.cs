
using UnityEngine;
using TMPro;

public class IslandInfoPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private CanvasGroup contentGroup;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI difficultyText;

    [Header("Position Settings")]
    [SerializeField] private float gap = 28f;
    [SerializeField] private float screenMargin = 20f;
    [SerializeField] private float topHUDSpace = 95f;

    [Header("Morph Animation")]
    [SerializeField] private float morphSpeed = 4f;

    private IslandHoverEffect currentIsland;

    private Vector3 morphOrigin;
    private Vector3 morphTarget;

    private float progress;

    private void Awake()
    {
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();

        if (panelGroup == null)
            panelGroup = GetComponent<CanvasGroup>();

        panelGroup.alpha = 0f;
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;

        if (contentGroup != null)
            contentGroup.alpha = 0f;

        panelRect.localScale = new Vector3(0.08f, 0.65f, 1f);
    }

    public void Show(IslandHoverEffect island)
    {
        currentIsland = island;

        titleText.text = island.islandTitle;
        descriptionText.text = island.description;
        difficultyText.text = "Difficulty: " + island.difficulty;

        RectTransform islandRect =
            island.GetComponent<RectTransform>();

        RectTransform parent =
            panelRect.parent as RectTransform;

        // Điểm bắt đầu Morph nằm ở giữa đảo.
        Vector3 islandCenter =
            islandRect.TransformPoint(islandRect.rect.center);

        morphOrigin = parent.InverseTransformPoint(islandCenter);

        // Vị trí bảng sau khi mở rộng.
        morphTarget = CalculatePanelPosition(islandRect);
    }

    public void Hide(IslandHoverEffect island)
    {
        // Tránh đảo cũ đóng bảng của đảo mới.
        if (currentIsland != island)
            return;

        currentIsland = null;
    }

    private void Update()
    {
        float target = currentIsland != null ? 1f : 0f;

        progress = Mathf.MoveTowards(
            progress,
            target,
            morphSpeed * Time.unscaledDeltaTime
        );

        float t = Mathf.SmoothStep(0f, 1f, progress);

        // Di chuyển bảng từ đảo đến vị trí đích.
        panelRect.localPosition = Vector3.Lerp(
            morphOrigin,
            morphTarget,
            t
        );

        // Morph: từ dải sáng nhỏ thành bảng.
        float scaleX = Mathf.Lerp(0.08f, 1f, t);
        float scaleY = Mathf.Lerp(0.65f, 1f, t);

        panelRect.localScale =
            new Vector3(scaleX, scaleY, 1f);

        // Nền xuất hiện trước.
        panelGroup.alpha = t;

        // Nội dung xuất hiện sau khi bảng mở ra.
        if (contentGroup != null)
        {
            contentGroup.alpha =
                Mathf.InverseLerp(0.55f, 0.95f, t);
        }
    }

    private Vector3 CalculatePanelPosition(
        RectTransform islandRect)
    {
        RectTransform parent =
            panelRect.parent as RectTransform;

        Vector3[] corners = new Vector3[4];
        islandRect.GetWorldCorners(corners);

        Vector3 bottomLeft =
            parent.InverseTransformPoint(corners[0]);

        Vector3 topRight =
            parent.InverseTransformPoint(corners[2]);

        float islandMinX = Mathf.Min(bottomLeft.x, topRight.x);
        float islandMaxX = Mathf.Max(bottomLeft.x, topRight.x);
        float islandMinY = Mathf.Min(bottomLeft.y, topRight.y);
        float islandMaxY = Mathf.Max(bottomLeft.y, topRight.y);

        float centerX = (islandMinX + islandMaxX) * 0.5f;
        float centerY = (islandMinY + islandMaxY) * 0.5f;

        float w = panelRect.rect.width;
        float h = panelRect.rect.height;

        Vector2 right = new Vector2(
            islandMaxX + gap + w * 0.5f,
            centerY
        );

        Vector2 left = new Vector2(
            islandMinX - gap - w * 0.5f,
            centerY
        );

        Vector2 above = new Vector2(
            centerX,
            islandMaxY + gap + h * 0.5f
        );

        Vector2 below = new Vector2(
            centerX,
            islandMinY - gap - h * 0.5f
        );

        Rect bounds = parent.rect;

        // Đảo ở nửa trái ưu tiên mở sang phải.
        // Đảo ở nửa phải ưu tiên mở sang trái.
        Vector2[] candidates = centerX < bounds.center.x
            ? new Vector2[] { right, left, above, below }
            : new Vector2[] { left, right, above, below };

        // Tìm phía đầu tiên đủ không gian.
        foreach (Vector2 candidate in candidates)
        {
            if (FitsInside(candidate, bounds, w, h))
                return new Vector3(candidate.x, candidate.y, 0f);
        }

        // Nếu không phía nào đủ rộng, giữ bảng trong màn hình.
        Vector2 fallback = candidates[0];

        float minX = bounds.xMin + screenMargin + w * 0.5f;
        float maxX = bounds.xMax - screenMargin - w * 0.5f;

        float minY = bounds.yMin + screenMargin + h * 0.5f;
        float maxY = bounds.yMax - topHUDSpace - h * 0.5f;

        fallback.x = Mathf.Clamp(fallback.x, minX, maxX);
        fallback.y = Mathf.Clamp(fallback.y, minY, maxY);

        return new Vector3(fallback.x, fallback.y, 0f);
    }

    private bool FitsInside(
        Vector2 position,
        Rect bounds,
        float width,
        float height)
    {
        return
            position.x - width * 0.5f >= bounds.xMin + screenMargin &&
            position.x + width * 0.5f <= bounds.xMax - screenMargin &&
            position.y - height * 0.5f >= bounds.yMin + screenMargin &&
            position.y + height * 0.5f <= bounds.yMax - topHUDSpace;
    }
}
