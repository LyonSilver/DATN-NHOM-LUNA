
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nextScene = "BattleScene";

    private bool isFinished;

    private void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (isFinished) return;

        isFinished = true;
        SceneManager.LoadScene(nextScene);
    }

    public void SkipIntro()
    {
        if (isFinished) return;

        isFinished = true;
        SceneManager.LoadScene(nextScene);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }
}
