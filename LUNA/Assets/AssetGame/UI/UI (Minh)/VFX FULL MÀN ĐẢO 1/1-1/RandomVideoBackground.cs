
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class RandomVideoBackground : MonoBehaviour
{
    [Header("Video Player")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoBackground;

    [Header("Danh sach 4 video")]
    [SerializeField] private VideoClip[] videos = new VideoClip[4];

    [Header("Cai dat")]
    [SerializeField, Min(0)]
    private float delayBetweenVideos = 3f;

    [SerializeField, Range(0, 3)]
    private int firstVideoIndex = 0;

    private readonly List<int> remainingVideos = new();

    private bool videoFinished;
    private bool videoError;

    private void OnEnable()
    {
        if (videoPlayer == null || videoBackground == null ||
            videos == null || videos.Length != 4)
        {
            Debug.LogError("Chua cai dat du VideoPlayer, RawImage va 4 videos!");
            return;
        }

        foreach (VideoClip clip in videos)
        {
            if (clip == null)
            {
                Debug.LogError("Co video chua duoc gan!");
                return;
            }
        }

        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;

        videoBackground.enabled = false;

        StartCoroutine(PlayVideoSequence());
    }

    private IEnumerator PlayVideoSequence()
    {
        int currentIndex = firstVideoIndex;

        // Video dau tien da duoc chon.
        RefillList(currentIndex);

        float delay = 0f;

        while (true)
        {
            videoFinished = false;
            videoError = false;

            videoPlayer.Stop();
            videoPlayer.clip = videos[currentIndex];

            // Chuan bi video trong khi cho.
            videoPlayer.Prepare();

            if (delay > 0f)
                yield return new WaitForSecondsRealtime(delay);

            // Doi video san sang, toi da 15 giay.
            float timeout = 15f;

            while (!videoPlayer.isPrepared &&
                   !videoError && timeout > 0f)
            {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            if (videoError || !videoPlayer.isPrepared)
            {
                Debug.LogError("Khong the chuan bi video!");
                yield break;
            }

            videoPlayer.Play();
            videoBackground.enabled = true;

            Debug.Log("Dang phat video: " + (currentIndex + 1));

            // Doi video chay xong.
            while (!videoFinished && !videoError)
                yield return null;

            // Hien thi anh nen tinh trong luc cho.
            videoBackground.enabled = false;

            if (videoError)
                yield break;

            // Lay video ngau nhien tiep theo.
            currentIndex = GetNextVideo(currentIndex);

            // Delay lay tu Inspector.
            delay = delayBetweenVideos;
        }
    }

    private int GetNextVideo(int previousIndex)
    {
        if (remainingVideos.Count == 0)
            RefillList(previousIndex);

        int randomPosition =
            Random.Range(0, remainingVideos.Count);

        int selectedIndex = remainingVideos[randomPosition];

        remainingVideos.RemoveAt(randomPosition);

        return selectedIndex;
    }

    private void RefillList(int excludedIndex)
    {
        remainingVideos.Clear();

        for (int i = 0; i < videos.Length; i++)
        {
            if (i != excludedIndex)
                remainingVideos.Add(i);
        }
    }

    private void OnVideoFinished(VideoPlayer player)
    {
        videoFinished = true;
    }

    private void OnVideoError(VideoPlayer player, string message)
    {
        videoError = true;
        Debug.LogError("VideoPlayer Error: " + message);
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.Stop();
        }

        if (videoBackground != null)
            videoBackground.enabled = false;
    }
}
