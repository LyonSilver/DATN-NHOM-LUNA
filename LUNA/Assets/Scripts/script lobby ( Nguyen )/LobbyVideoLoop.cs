
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class LobbyVideoLoop : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            Debug.LogWarning("[LobbyVideoLoop] VideoPlayer component not found. Adding one.");
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true;
        videoPlayer.loopPointReached += OnVideoEnded;
    }

    private void Start()
    {
        if (videoPlayer != null)
            videoPlayer.Play();
    }

    private void OnVideoEnded(VideoPlayer source)
    {
        if (source == null)
            return;

        source.time = 0;
        source.Play();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnded;
    }
}
