
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CutsceneLoader : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName;

    private bool hasLoadedScene = false;

    private void Start()
    {
        if (videoPlayer != null && videoPlayer.clip != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Play();
        }
        else
        {
            LoadNextScene();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        LoadNextScene();
    }

    public void LoadNextScene()
    {
        if (hasLoadedScene) return;

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogError("Chưa nhập tên Scene tiếp theo!");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError("Scene chưa được thêm vào Build Profiles!");
            return;
        }

        hasLoadedScene = true;
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }
}