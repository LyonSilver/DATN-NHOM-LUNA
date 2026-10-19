
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class IslandCinematicController : MonoBehaviour
{
    [Serializable]
    public class IslandChoice
    {
        public IslandHoverEffect island;
        public string sceneName;
    }

    private enum CinematicState
    {
        Idle,
        Entering,
        Open,
        Leaving
    }

    [Header("Six Islands")]
    [SerializeField] private IslandChoice[] islands;

    [Header("Cinematic Layer")]
    [SerializeField] private RectTransform cinematicLayer;
    [SerializeField] private CanvasGroup dimGroup;

    [Header("Flying Island")]
    [SerializeField] private RectTransform flyingRect;
    [SerializeField] private Image flyingImage;

    [Header("Information Card")]
    [SerializeField] private RectTransform cardRect;
    [SerializeField] private CanvasGroup cardGroup;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text difficultyText;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button backButton;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.75f;
    [SerializeField] private float islandWidthRatio = 0.34f;
    [SerializeField] private float islandLeftRatio = 0.22f;
    [SerializeField] private float cardRightRatio = 0.21f;

    private CinematicState state = CinematicState.Idle;

    private IslandChoice selected;
    private Image selectedOriginalImage;

    private RectTransform[] islandRects;
    private Vector2[] homePositions;
    private Vector2[] exitPositions;

    private Vector2 flyStartPosition;
    private Vector2 flyStartSize;
    private Vector2 flyTargetSize;
    private Vector2 flyFinalPosition;

    private Vector2 cardStartPosition;
    private Vector2 cardFinalPosition;

    private void Awake()
    {
        if (cinematicLayer == null ||
            flyingRect == null ||
            flyingImage == null ||
            cardRect == null ||
            cardGroup == null ||
            dimGroup == null ||
            playButton == null ||
            backButton == null ||
            islands == null ||
            islands.Length == 0)
        {
            Debug.LogError(
                "IslandCinematicController: thiếu tham chiếu Inspector!",
                this
            );
            enabled = false;
            return;
        }

        islandRects = new RectTransform[islands.Length];
        homePositions = new Vector2[islands.Length];
        exitPositions = new Vector2[islands.Length];

        // Tự đăng ký sự kiện click cho từng đảo.
        for (int i = 0; i < islands.Length; i++)
        {
            IslandChoice choice = islands[i];

            if (choice == null || choice.island == null)
                continue;

            islandRects[i] =
                choice.island.GetComponent<RectTransform>();

            Button button =
                choice.island.GetComponent<Button>();

            if (button != null)
            {
                IslandChoice captured = choice;

                button.onClick.AddListener(
                    () => SelectIsland(captured)
                );
            }
        }

        playButton.onClick.AddListener(PlayLevel);
        backButton.onClick.AddListener(Back);

        cinematicLayer.gameObject.SetActive(false);
    }

    public void SelectIsland(IslandChoice choice)
    {
        
        if (state != CinematicState.Idle)
            return;

        if (choice == null || choice.island == null)
            return;

        RectTransform sourceRect =
            choice.island.GetComponent<RectTransform>();

        Image sourceImage =
            choice.island.GetComponent<Image>();

        if (sourceImage == null || sourceImage.sprite == null)
        {
            Debug.LogWarning("Đảo chưa có sprite!");
            return;
        }

        selected = choice;
        selectedOriginalImage = sourceImage;

        // Chụp tọa độ thế giới trước khi khóa Hover.
        Vector3 worldCenter =
            sourceRect.TransformPoint(sourceRect.rect.center);

        flyStartPosition =
            cinematicLayer.InverseTransformPoint(worldCenter);

        Vector3[] corners = new Vector3[4];
        sourceRect.GetWorldCorners(corners);

        Vector3 bottomLeft =
            cinematicLayer.InverseTransformPoint(corners[0]);

        Vector3 topRight =
            cinematicLayer.InverseTransformPoint(corners[2]);

        flyStartSize = new Vector2(
            Mathf.Abs(topRight.x - bottomLeft.x),
            Mathf.Abs(topRight.y - bottomLeft.y)
        );

        // Khóa Hover và lưu vị trí gốc của cả sáu đảo.
        for (int i = 0; i < islands.Length; i++)
        {
            if (islands[i] == null ||
                islands[i].island == null)
                continue;

            islands[i].island.SetCinematicMode(true);

            RectTransform rt = islandRects[i];

            if (rt == null) continue;

            homePositions[i] = rt.anchoredPosition;
            exitPositions[i] =
                CalculateExitPosition(rt, homePositions[i]);
        }

        // Chuẩn bị hình đảo phóng lớn.
        flyingImage.sprite = sourceImage.sprite;
        flyingImage.preserveAspect = true;

        Outline glow = flyingImage.GetComponent<Outline>();

        if (glow != null)
        {
            Color color = choice.island.glowColor;
            color.a = 0.9f;

            glow.effectColor = color;
        }

        // Thông tin đảo.
        titleText.text = choice.island.islandTitle;
        descriptionText.text = choice.island.description;
        difficultyText.text =
            "Độ khó: " + choice.island.difficulty;

        float layerWidth = cinematicLayer.rect.width;
        float layerHeight = cinematicLayer.rect.height;

        // Giới hạn kích thước đảo để vừa màn hình.
        float maxWidth = layerWidth * islandWidthRatio;
        float maxHeight = layerHeight * 0.6f;

        float scaleFactor = Mathf.Min(
            maxWidth / Mathf.Max(1f, flyStartSize.x),
            maxHeight / Mathf.Max(1f, flyStartSize.y)
        );

        flyTargetSize = flyStartSize * scaleFactor;

        // Đích cuối của đảo: bên trái.
        flyFinalPosition = new Vector2(
            -layerWidth * islandLeftRatio,
            0f
        );

        // Đích cuối của bảng: bên phải.
        cardFinalPosition = new Vector2(
            layerWidth * cardRightRatio,
            -20f
        );

        // Bảng bắt đầu lệch khỏi phía phải.
        cardStartPosition =
            cardFinalPosition + Vector2.right * 220f;

        // Ẩn ảnh đảo gốc; giữ GameObject và Button.
        selectedOriginalImage.enabled = false;

        // Hiển thị lớp cinematic.
        cinematicLayer.gameObject.SetActive(true);

        cardGroup.interactable = false;
        cardGroup.blocksRaycasts = false;

        playButton.interactable =
            !string.IsNullOrWhiteSpace(choice.sceneName);

        state = CinematicState.Entering;

        ApplyAnimation(0f);
        StartCoroutine(Animate(true));
    }

    // Xác định hướng trượt ra ngoài màn hình.
    private Vector2 CalculateExitPosition(
        RectTransform rt,
        Vector2 home)
    {
        float width = cinematicLayer.rect.width;
        float height = cinematicLayer.rect.height;

        float nx = home.x / Mathf.Max(1f, width * 0.5f);
        float ny = home.y / Mathf.Max(1f, height * 0.5f);

        Vector2 offset;

        if (Mathf.Abs(nx) >= Mathf.Abs(ny))
        {
            float dir = nx < 0f ? -1f : 1f;

            offset = new Vector2(
                dir * (width + rt.rect.width),
                0f
            );
        }
        else
        {
            float dir = ny < 0f ? -1f : 1f;

            offset = new Vector2(
                0f,
                dir * (height + rt.rect.height)
            );
        }

        return home + offset;
    }

    // Easing mượt theo khoảng thời gian.
    private float SmoothRange(
        float t,
        float start,
        float end)
    {
        float value = Mathf.InverseLerp(start, end, t);
        return Mathf.SmoothStep(0f, 1f, value);
    }

    private void ApplyAnimation(float progress)
    {
        // Giai đoạn 1: tất cả đảo khác rời màn hình.
        float islandsT = SmoothRange(progress, 0f, 0.78f);

        for (int i = 0; i < islands.Length; i++)
        {
            if (islandRects[i] == null)
                continue;

            if (islands[i] == selected)
                continue;

            islandRects[i].anchoredPosition =
                Vector2.Lerp(
                    homePositions[i],
                    exitPositions[i],
                    islandsT
                );
        }

        // Giai đoạn 2: đảo được chọn bay ra giữa.
        float flyT = SmoothRange(progress, 0f, 0.62f);

        Vector2 centerPosition = Vector2.Lerp(
            flyStartPosition,
            Vector2.zero,
            flyT
        );

        flyingRect.sizeDelta = Vector2.Lerp(
            flyStartSize,
            flyTargetSize,
            flyT
        );

        // Giai đoạn 3: đảo dịch trái, bảng hiện bên phải.
        float detailT = SmoothRange(progress, 0.58f, 1f);

        flyingRect.anchoredPosition = Vector2.Lerp(
            centerPosition,
            flyFinalPosition,
            detailT
        );

        cardRect.anchoredPosition = Vector2.Lerp(
            cardStartPosition,
            cardFinalPosition,
            detailT
        );

        cardGroup.alpha = detailT;

        // Làm tối nền.
        dimGroup.alpha = SmoothRange(progress, 0f, 0.7f);
    }

    private IEnumerator Animate(bool entering)
    {
        float elapsed = 0f;

        float start = entering ? 0f : 1f;
        float end = entering ? 1f : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            float progress = Mathf.Lerp(start, end, t);

            ApplyAnimation(progress);

            yield return null;
        }

        ApplyAnimation(end);

        if (entering)
        {
            state = CinematicState.Open;

            cardGroup.interactable = true;
            cardGroup.blocksRaycasts = true;
        }
        else
        {
            // Đã quay về vị trí ban đầu.
            if (selectedOriginalImage != null)
                selectedOriginalImage.enabled = true;

            cinematicLayer.gameObject.SetActive(false);

            for (int i = 0; i < islands.Length; i++)
            {
                if (islands[i] == null ||
                    islands[i].island == null)
                    continue;

                islands[i].island.SetCinematicMode(false);
            }

            selected = null;
            selectedOriginalImage = null;

            state = CinematicState.Idle;
        }
    }

    public void Back()
    {
        if (state != CinematicState.Open)
            return;

        state = CinematicState.Leaving;

        cardGroup.interactable = false;
        cardGroup.blocksRaycasts = false;

        StartCoroutine(Animate(false));
    }

    public void PlayLevel()
    {
        if (state != CinematicState.Open ||
            selected == null)
            return;

        string sceneName = selected.sceneName;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("Chưa nhập Scene Name!");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                "Scene chưa có trong Build Profiles: " +
                sceneName
            );
            return;
        }

        state = CinematicState.Leaving;
        SceneManager.LoadScene(sceneName);
    }
}
