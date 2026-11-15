
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class RandomVideoBackground : MonoBehaviour
{
    [Header("Existing references")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private VideoClip[] videos = new VideoClip[4];

    [Header("Background Display")]
    [SerializeField] private float backgroundDistance = 20f;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = -1000;

    [Header("Playback")]
    [SerializeField, Min(0f)] private float delayBetweenVideos = 3f;
    [SerializeField, Range(0, 3)] private int firstVideoIndex;
    [SerializeField, Range(0f, 2f)] private float fadeDuration = 0.25f;
    [SerializeField, Min(1f)] private float prepareTimeout = 15f;
    [SerializeField, Min(16)] private int textureWidth = 1920;
    [SerializeField, Min(16)] private int textureHeight = 1080;

    private VideoPlayer secondaryPlayer;

    private RenderTexture[] textures;
    private VideoPlayer[] players;
    private GameObject[] backgroundObjects;
    private MeshRenderer[] backgroundRenderers;
    private Material[] backgroundMaterials;

    private Shader backgroundShader;

    private readonly List<int> remainingVideos = new List<int>();

    private Coroutine playbackRoutine;

    private readonly bool[] prepared = new bool[2];
    private readonly bool[] errors = new bool[2];
    private readonly bool[] ended = new bool[2];

    private bool initialized;
    private bool originalSettingsCaptured;

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
        // Automatically find VideoPlayer.
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // Automatically find Main Camera.
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (!ValidateSettings())
            return;

        if (!initialized && !Initialize())
        {
            ReleaseResources();
            return;
        }

        SetBackgroundEnabled(true);

        remainingVideos.Clear();
        playbackRoutine = StartCoroutine(PlaybackLoop());
    }


    private bool ValidateSettings()
    {
        if (videoPlayer == null || targetCamera == null ||
            videos == null || videos.Length != 4)
        {
            Debug.LogError(
                "RandomVideoBackground: Assign a VideoPlayer, Camera, and exactly four VideoClips.",
                this);

            return false;
        }

        for (int i = 0; i < videos.Length; i++)
        {
            if (videos[i] == null)
            {
                Debug.LogError(
                    "RandomVideoBackground: Missing VideoClip at index " + i + ".",
                    this);

                return false;
            }
        }

        if (textureWidth < 16 || textureHeight < 16 ||
            prepareTimeout <= 0f || backgroundDistance <= 0f)
        {
            Debug.LogError(
                "RandomVideoBackground: Invalid texture dimensions, timeout, or background distance.",
                this);

            return false;
        }

        return true;
    }

    private bool Initialize()
    {
        // Save original VideoPlayer settings.
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

        // Find a shader compatible with the current render pipeline.
        backgroundShader =
            Shader.Find("Universal Render Pipeline/Unlit");

        if (backgroundShader == null)
            backgroundShader = Shader.Find("Unlit/Transparent");

        if (backgroundShader == null)
            backgroundShader = Shader.Find("Sprites/Default");

        if (backgroundShader == null)
        {
            Debug.LogError(
                "RandomVideoBackground: Could not find a compatible background shader.",
                this);

            return false;
        }

        // Create the second VideoPlayer.
        secondaryPlayer = gameObject.AddComponent<VideoPlayer>();
        secondaryPlayer.hideFlags = HideFlags.HideInInspector;

        textures = new RenderTexture[2];
        players = new VideoPlayer[] { videoPlayer, secondaryPlayer };

        backgroundObjects = new GameObject[2];
        backgroundRenderers = new MeshRenderer[2];
        backgroundMaterials = new Material[2];

        for (int i = 0; i < 2; i++)
        {
            // Create RenderTexture.
            textures[i] = new RenderTexture(
                textureWidth,
                textureHeight,
                0,
                RenderTextureFormat.ARGB32);

            textures[i].name = "RandomVideoBackground_RT_" + i;

            if (!textures[i].Create())
            {
                Debug.LogError(
                    "RandomVideoBackground: Could not create RenderTexture " + i,
                    this);

                return false;
            }

            // Configure VideoPlayer.
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

            // Create independent background display.
            if (!CreateBackgroundDisplay(i))
                return false;
        }

        initialized = true;
        return true;
    }

    private bool CreateBackgroundDisplay(int index)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);

        quad.name = "VideoBackground_Quad_" + index;

        // Remove unnecessary Collider.
        Collider quadCollider = quad.GetComponent<Collider>();

        if (quadCollider != null)
            Destroy(quadCollider);

        backgroundObjects[index] = quad;

        // Create material.
        Material material = new Material(backgroundShader);

        material.name = "VideoBackground_Material_" + index;

        SetMaterialTexture(material, textures[index]);
        SetMaterialAlpha(material, 0f);

        backgroundMaterials[index] = material;

        // Configure MeshRenderer.
        MeshRenderer renderer = quad.GetComponent<MeshRenderer>();

        renderer.sharedMaterial = material;
        renderer.sortingLayerName = sortingLayerName;
        renderer.sortingOrder = sortingOrder;

        renderer.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        renderer.receiveShadows = false;
        renderer.lightProbeUsage =
            UnityEngine.Rendering.LightProbeUsage.Off;

        backgroundRenderers[index] = renderer;

        UpdateBackgroundTransform(index);

        return true;
    }

    private void LateUpdate()
    {
        if (!initialized || targetCamera == null)
            return;

        for (int i = 0; i < 2; i++)
        {
            UpdateBackgroundTransform(i);
        }
    }

    private void UpdateBackgroundTransform(int index)
    {
        if (backgroundObjects == null ||
            backgroundObjects[index] == null)
            return;

        Transform cameraTransform = targetCamera.transform;

        GameObject quad = backgroundObjects[index];

        quad.transform.position =
            cameraTransform.position +
            cameraTransform.forward * backgroundDistance;

        quad.transform.rotation = cameraTransform.rotation;

        float height;
        float width;

        if (targetCamera.orthographic)
        {
            height = targetCamera.orthographicSize * 2f;
            width = height * targetCamera.aspect;
        }
        else
        {
            height = 2f * backgroundDistance *
                Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

            width = height * targetCamera.aspect;
        }

        quad.transform.localScale =
            new Vector3(width, height, 1f);
    }

    private void SetBackgroundEnabled(bool value)
    {
        if (backgroundRenderers == null)
            return;

        for (int i = 0; i < backgroundRenderers.Length; i++)
        {
            if (backgroundRenderers[i] != null)
                backgroundRenderers[i].enabled = value;
        }
    }

    private void SetMaterialTexture(Material material, Texture texture)
    {
        if (material.HasProperty("_BaseMap"))
            material.SetTexture("_BaseMap", texture);

        if (material.HasProperty("_MainTex"))
            material.SetTexture("_MainTex", texture);
    }

    private void SetMaterialAlpha(Material material, float alpha)
    {
        Color color = Color.white;
        color.a = alpha;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);

        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);
    }

    private IEnumerator PlaybackLoop()
    {
        int currentSlot = 0;
        int currentVideo = firstVideoIndex;

        BeginPrepare(currentSlot, currentVideo);

        yield return WaitForPrepared(currentSlot);

        if (errors[currentSlot] || !prepared[currentSlot])
            yield break;

        SetMaterialAlpha(backgroundMaterials[currentSlot], 1f);

        players[currentSlot].Play();

        RefillVideos(currentVideo);

        while (isActiveAndEnabled)
        {
            int nextVideo = GetNextVideo(currentVideo);
            int nextSlot = 1 - currentSlot;

            BeginPrepare(nextSlot, nextVideo);

            // Wait until current video finishes.
            while (!ended[currentSlot] &&
                   !errors[currentSlot] &&
                   isActiveAndEnabled)
            {
                yield return null;
            }

            if (errors[currentSlot] || !isActiveAndEnabled)
                yield break;

            if (delayBetweenVideos > 0f)
            {
                yield return new WaitForSecondsRealtime(
                    delayBetweenVideos);
            }

            // Prepare the next video.
            yield return WaitForPrepared(nextSlot);

            if (errors[nextSlot] || !prepared[nextSlot])
                yield break;

            // Crossfade between the two backgrounds.
            yield return CrossfadeTo(currentSlot, nextSlot);

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

        while (!prepared[slot] &&
               !errors[slot] &&
               elapsed < prepareTimeout)
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

            Debug.LogError(
                "RandomVideoBackground: Timed out preparing video in slot " + slot,
                this);
        }
    }

    private IEnumerator CrossfadeTo(int currentSlot, int nextSlot)
    {
        players[nextSlot].Play();

        SetMaterialAlpha(backgroundMaterials[nextSlot], 0f);

        if (fadeDuration > 0f)
        {
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(elapsed / fadeDuration);

                // Fade out old video.
                SetMaterialAlpha(
                    backgroundMaterials[currentSlot],
                    1f - progress);

                // Fade in new video.
                SetMaterialAlpha(
                    backgroundMaterials[nextSlot],
                    progress);

                yield return null;
            }
        }

        SetMaterialAlpha(backgroundMaterials[currentSlot], 0f);
        SetMaterialAlpha(backgroundMaterials[nextSlot], 1f);
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

        int randomPosition =
            Random.Range(0, remainingVideos.Count);

        int selectedIndex = remainingVideos[randomPosition];

        remainingVideos.RemoveAt(randomPosition);

        return selectedIndex;
    }

    private int GetSlot(VideoPlayer player)
    {
        return players != null &&
               players.Length > 1 &&
               player == players[1] ? 1 : 0;
    }

    private void OnPlayerPrepared(VideoPlayer player)
    {
        prepared[GetSlot(player)] = true;
    }

    private void OnPlayerError(VideoPlayer player, string message)
    {
        int slot = GetSlot(player);

        errors[slot] = true;

        Debug.LogError(
            "RandomVideoBackground: VideoPlayer error in slot " +
            slot + ": " + message,
            this);
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

        SetBackgroundEnabled(false);
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

                if (player == null)
                    continue;

                player.prepareCompleted -= OnPlayerPrepared;
                player.errorReceived -= OnPlayerError;
                player.loopPointReached -= OnPlayerEnded;

                player.Stop();
            }
        }

        // Restore original VideoPlayer settings.
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

        // Release RenderTextures.
        if (textures != null)
        {
            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i] == null)
                    continue;

                textures[i].Release();
                DestroyCreatedObject(textures[i]);
            }
        }

        // Destroy materials.
        if (backgroundMaterials != null)
        {
            for (int i = 0; i < backgroundMaterials.Length; i++)
            {
                if (backgroundMaterials[i] != null)
                    DestroyCreatedObject(backgroundMaterials[i]);
            }
        }

        // Destroy background quads.
        if (backgroundObjects != null)
        {
            for (int i = 0; i < backgroundObjects.Length; i++)
            {
                if (backgroundObjects[i] != null)
                    DestroyCreatedObject(backgroundObjects[i]);
            }
        }

        if (secondaryPlayer != null)
            DestroyCreatedObject(secondaryPlayer);

        secondaryPlayer = null;

        textures = null;
        players = null;
        backgroundObjects = null;
        backgroundRenderers = null;
        backgroundMaterials = null;

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
