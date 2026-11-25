
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SkipCutscene : MonoBehaviour
{
    [Header("Cutscene Settings")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject skipButton;

    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "SelectMap";

    [Header("Skip Button Settings")]
    [SerializeField] private float hideDelay = 3f;

    private bool isFinished;
    private float hideTimer;

    private void Start()
    {
        // Ẩn nút Skip khi bắt đầu cutscene
        if (skipButton != null)
            skipButton.SetActive(false);
    }

    private void OnEnable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void Update()
    {
        if (isFinished || skipButton == null)
            return;

        // Nhấp chuột vào bất kỳ vị trí nào
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShowSkipButton();
        }

        // Đếm thời gian tự động ẩn nút
        if (skipButton.activeSelf)
        {
            hideTimer -= Time.unscaledDeltaTime;

            if (hideTimer <= 0f)
            {
                skipButton.SetActive(false);
            }
        }
    }

    private void ShowSkipButton()
    {
        skipButton.SetActive(true);

        // Reset thời gian mỗi khi nhấp chuột
        hideTimer = hideDelay;
    }

    public void Skip()
    {
        FinishCutscene();
    }

    private void OnVideoFinished(VideoPlayer player)
    {
        FinishCutscene();
    }

    private void FinishCutscene()
    {
        if (isFinished) return;

        isFinished = true;

        if (videoPlayer != null)
            videoPlayer.Stop();

        if (skipButton != null)
            skipButton.SetActive(false);

        SceneManager.LoadScene(nextSceneName);
    }
}
