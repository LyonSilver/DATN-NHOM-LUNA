using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class RandomVideoBackground : MonoBehaviour
{
    [Header("Existing references")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoBackground;
    [SerializeField] private VideoClip[] videos = new VideoClip[4];

    [Header("Playback")]
    [SerializeField, Min(0f)] private float delayBetweenVideos = 3f;
    [SerializeField, Range(0, 3)] private int firstVideoIndex;
    [SerializeField, Range(0f, 2f)] private float fadeDuration = 0.25f;
    [SerializeField, Min(1f)] private float prepareTimeout = 15f;
    [SerializeField, Min(16)] private int textureWidth = 1920;
    [SerializeField, Min(16)] private int textureHeight = 1080;

    private VideoPlayer secondaryPlayer;
    private RawImage secondaryImage;
    private RenderTexture[] textures;
    private VideoPlayer[] players;
    private readonly List<int> remainingVideos = new List<int>();
    private Coroutine playbackRoutine;
    private readonly bool[] prepared = new bool[2];
    private readonly bool[] errors = new bool[2];
    private readonly bool[] ended = new bool[2];
    private bool initialized;
    private bool originalSettingsCaptured;

    private Texture originalBackgroundTexture;
    private Color originalBackgroundColor;
    private bool originalBackgroundEnabled;
    private RenderTexture originalTargetTexture;
    private VideoRenderMode originalRenderMode;
    private VideoSource originalSource;
    private VideoClip originalClip;
    private bool originalPlayOnAwake;
    private bool originalLooping;
    private bool originalWaitForFirstFrame;
    private bool originalSendFrameReadyEvents;
    private VideoAudioOutputMode originalAudioOutputMode;

    private void OnEnable()
    {
        if (!ValidateSettings()) return;

        if (!initialized && !Initialize())
        {
            ReleaseResources();
            return;
        }

        remainingVideos.Clear();
        playbackRoutine = StartCoroutine(PlaybackLoop());
    }

    private bool ValidateSettings()
    {
        if (videoPlayer == null || videoBackground == null || videos == null || videos.Length != 4)
        {
            Debug.LogError("RandomVideoBackground: Assign a VideoPlayer, a RawImage, and exactly four VideoClips.", this);
            return false;
        }

        for (int i = 0; i < videos.Length; i++)
        {
            if (videos[i] == null)
            {
                Debug.LogError("RandomVideoBackground: Missing VideoClip at index " + i + ".", this);
                return false;
            }
        }

        if (firstVideoIndex < 0 || firstVideoIndex >= videos.Length)
        {
            Debug.LogError("RandomVideoBackground: First video index is out of range.", this);
            return false;
        }

        if (textureWidth <= 0 || textureHeight <= 0 || prepareTimeout <= 0f)
        {
            Debug.LogError("RandomVideoBackground: Texture dimensions and prepare timeout must be greater than zero.", this);
            return false;
        }

        return true;
    }

    private bool Initialize()
    {
        originalBackgroundTexture = videoBackground.texture;
        originalBackgroundColor = videoBackground.color;
        originalBackgroundEnabled = videoBackground.enabled;
        originalTargetTexture = videoPlayer.targetTexture;
        originalRenderMode = videoPlayer.renderMode;
        originalSource = videoPlayer.source;
        originalClip = videoPlayer.clip;
        originalPlayOnAwake = videoPlayer.playOnAwake;
        originalLooping = videoPlayer.isLooping;
        originalWaitForFirstFrame = videoPlayer.waitForFirstFrame;
        originalSendFrameReadyEvents = videoPlayer.sendFrameReadyEvents;
        originalAudioOutputMode = videoPlayer.audioOutputMode;
        originalSettingsCaptured = true;

        secondaryPlayer = gameObject.AddComponent<VideoPlayer>();
        secondaryPlayer.hideFlags = HideFlags.HideInInspector;

        GameObject overlay = new GameObject(
            "VideoCrossfadeOverlay",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(RawImage));
        overlay.transform.SetParent(videoBackground.transform.parent, false);

        RectTransform baseRect = videoBackground.rectTransform;
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = baseRect.anchorMin;
        overlayRect.anchorMax = baseRect.anchorMax;
        overlayRect.pivot = baseRect.pivot;
        overlayRect.anchoredPosition = baseRect.anchoredPosition;
        overlayRect.sizeDelta = baseRect.sizeDelta;
        overlayRect.localRotation = baseRect.localRotation;
        overlayRect.localScale = baseRect.localScale;
        overlay.transform.SetSiblingIndex(videoBackground.transform.GetSiblingIndex() + 1);

        secondaryImage = overlay.GetComponent<RawImage>();
        secondaryImage.raycastTarget = false;
        secondaryImage.enabled = false;
        secondaryImage.uvRect = videoBackground.uvRect;
        secondaryImage.material = videoBackground.material;
        secondaryImage.color = new Color(
            originalBackgroundColor.r,
            originalBackgroundColor.g,
            originalBackgroundColor.b,
            0f);

        textures = new RenderTexture[2];
        players = new[] { videoPlayer, secondaryPlayer };

        for (int i = 0; i < players.Length; i++)
        {
            textures[i] = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);
            textures[i].name = "RandomVideoBackground_RT_" + i;
            if (!textures[i].Create())
            {
                Debug.LogError("RandomVideoBackground: Could not create render texture " + i + ".", this);
                return false;
            }

            VideoPlayer player = players[i];
            player.Stop();
            player.source = VideoSource.VideoClip;
            player.playOnAwake = false;
            player.isLooping = false;
            player.waitForFirstFrame = true;
            player.audioOutputMode = VideoAudioOutputMode.None;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.targetTexture = textures[i];
            player.sendFrameReadyEvents = false;
            player.prepareCompleted += OnPlayerPrepared;
            player.errorReceived += OnPlayerError;
            player.loopPointReached += OnPlayerEnded;
        }

        initialized = true;
        return true;
    }

    private IEnumerator PlaybackLoop()
    {
        int currentSlot = 0;
        int currentVideo = firstVideoIndex;

        BeginPrepare(currentSlot, currentVideo);
        yield return WaitForPrepared(currentSlot);
        if (errors[currentSlot] || !prepared[currentSlot]) yield break;

        videoBackground.texture = textures[currentSlot];
        videoBackground.color = originalBackgroundColor;
        videoBackground.enabled = true;
        players[currentSlot].Play();
        RefillVideos(currentVideo);

        while (isActiveAndEnabled)
        {
            int nextVideo = GetNextVideo(currentVideo);
            int nextSlot = 1 - currentSlot;
            BeginPrepare(nextSlot, nextVideo);

            while (!ended[currentSlot] && !errors[currentSlot] && isActiveAndEnabled)
                yield return null;

            if (errors[currentSlot] || !isActiveAndEnabled) yield break;

            if (delayBetweenVideos > 0f)
                yield return new WaitForSecondsRealtime(delayBetweenVideos);

            yield return WaitForPrepared(nextSlot);
            if (errors[nextSlot] || !prepared[nextSlot]) yield break;

            yield return CrossfadeTo(nextSlot);
            players[currentSlot].Stop();
            currentSlot = nextSlot;
            currentVideo = nextVideo;
        }
    }

    private void BeginPrepare(int slot, int videoIndex)
    {
        prepared[slot] = false;
        errors[slot] = false;
        ended[slot] = false;
        players[slot].Stop();
        players[slot].clip = videos[videoIndex];
        players[slot].Prepare();
    }

    private IEnumerator WaitForPrepared(int slot)
    {
        float elapsed = 0f;
        while (!prepared[slot] && !errors[slot] && elapsed < prepareTimeout)
        {
            if (players[slot].isPrepared)
            {
                prepared[slot] = true;
                break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!prepared[slot] && !errors[slot])
        {
            errors[slot] = true;
            Debug.LogError("RandomVideoBackground: Timed out preparing video in slot " + slot + ".", this);
        }
    }

    private IEnumerator CrossfadeTo(int nextSlot)
    {
        players[nextSlot].Play();

        if (fadeDuration > 0f)
        {
            secondaryImage.texture = textures[nextSlot];
            secondaryImage.color = new Color(
                originalBackgroundColor.r,
                originalBackgroundColor.g,
                originalBackgroundColor.b,
                0f);
            secondaryImage.enabled = true;
            yield return null;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = originalBackgroundColor.a * Mathf.Clamp01(elapsed / fadeDuration);
                secondaryImage.color = new Color(
                    originalBackgroundColor.r,
                    originalBackgroundColor.g,
                    originalBackgroundColor.b,
                    alpha);
                yield return null;
            }
        }

        videoBackground.texture = textures[nextSlot];
        videoBackground.color = originalBackgroundColor;
        videoBackground.enabled = true;
        secondaryImage.enabled = false;
        secondaryImage.color = new Color(
            originalBackgroundColor.r,
            originalBackgroundColor.g,
            originalBackgroundColor.b,
            0f);
    }

    private void RefillVideos(int excludedIndex)
    {
        remainingVideos.Clear();
        for (int i = 0; i < videos.Length; i++)
        {
            if (i != excludedIndex)
                remainingVideos.Add(i);
        }
    }

    private int GetNextVideo(int previousIndex)
    {
        if (remainingVideos.Count == 0)
            RefillVideos(previousIndex);

        int randomPosition = Random.Range(0, remainingVideos.Count);
        int selectedIndex = remainingVideos[randomPosition];
        remainingVideos.RemoveAt(randomPosition);
        return selectedIndex;
    }

    private int GetSlot(VideoPlayer player)
    {
        return players != null && players.Length > 1 && player == players[1] ? 1 : 0;
    }

    private void OnPlayerPrepared(VideoPlayer player)
    {
        prepared[GetSlot(player)] = true;
    }

    private void OnPlayerError(VideoPlayer player, string message)
    {
        int slot = GetSlot(player);
        errors[slot] = true;
        Debug.LogError("RandomVideoBackground: VideoPlayer error in slot " + slot + ": " + message, this);
    }

    private void OnPlayerEnded(VideoPlayer player)
    {
        ended[GetSlot(player)] = true;
    }

    private void OnDisable()
    {
        if (playbackRoutine != null)
        {
            StopCoroutine(playbackRoutine);
            playbackRoutine = null;
        }

        if (players != null)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null)
                    players[i].Stop();
            }
        }

        if (secondaryImage != null)
        {
            secondaryImage.enabled = false;
            secondaryImage.texture = null;
        }

        if (originalSettingsCaptured && videoBackground != null)
        {
            videoBackground.texture = originalBackgroundTexture;
            videoBackground.color = originalBackgroundColor;
            videoBackground.enabled = originalBackgroundEnabled;
        }
    }

    private void OnDestroy()
    {
        ReleaseResources();
    }

    private void ReleaseResources()
    {
        if (players != null)
        {
            for (int i = 0; i < players.Length; i++)
            {
                VideoPlayer player = players[i];
                if (player == null) continue;

                player.prepareCompleted -= OnPlayerPrepared;
                player.errorReceived -= OnPlayerError;
                player.loopPointReached -= OnPlayerEnded;
                player.Stop();
            }
        }

        if (originalSettingsCaptured && videoPlayer != null)
        {
            videoPlayer.targetTexture = originalTargetTexture;
            videoPlayer.renderMode = originalRenderMode;
            videoPlayer.source = originalSource;
            videoPlayer.clip = originalClip;
            videoPlayer.playOnAwake = originalPlayOnAwake;
            videoPlayer.isLooping = originalLooping;
            videoPlayer.waitForFirstFrame = originalWaitForFirstFrame;
            videoPlayer.sendFrameReadyEvents = originalSendFrameReadyEvents;
            videoPlayer.audioOutputMode = originalAudioOutputMode;
            originalSettingsCaptured = false;
        }

        if (textures != null)
        {
            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i] == null) continue;
                textures[i].Release();
                DestroyCreatedObject(textures[i]);
            }
        }

        if (secondaryImage != null)
            DestroyCreatedObject(secondaryImage.gameObject);
        if (secondaryPlayer != null)
            DestroyCreatedObject(secondaryPlayer);

        secondaryPlayer = null;
        secondaryImage = null;
        textures = null;
        players = null;
        initialized = false;
    }

    private void DestroyCreatedObject(Object target)
    {
        if (Application.isPlaying)
            Destroy(target);
        else
            DestroyImmediate(target);
    }
}
