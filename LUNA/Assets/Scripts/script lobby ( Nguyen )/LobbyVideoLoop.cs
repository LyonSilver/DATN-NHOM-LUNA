
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class LobbyVideoLoop : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true;
        videoPlayer.loopPointReached += OnVideoEnded;
    }

    private void Start()
    {
        videoPlayer.Play();
    }

    private void OnVideoEnded(VideoPlayer source)
    {
        source.time = 0;
        source.Play();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnded;
    }
}
