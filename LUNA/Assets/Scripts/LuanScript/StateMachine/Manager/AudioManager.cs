
using System;
using UnityEngine;

[Serializable]
public class SoundData
{
    public string soundName;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Default BGM")]
    [SerializeField] private AudioClip defaultBGM;

    [Header("Sound Effects")]
    [SerializeField] private SoundData[] soundEffects;

    private const string MasterKey = "MasterVolume";
    private const string BGMKey = "BGMVolume";
    private const string SFXKey = "SFXVolume";

    public float MasterVolume { get; private set; }
    public float BGMVolume { get; private set; }
    public float SFXVolume { get; private set; }

    private void Awake()
    {
        // Singleton: chỉ tồn tại một AudioManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Tự tạo AudioSource nếu chưa gán
        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();

        if (sfxSource == null || sfxSource == bgmSource)
            sfxSource = gameObject.AddComponent<AudioSource>();

        bgmSource.playOnAwake = false;
        bgmSource.loop = true;

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        LoadVolume();

        // Phát nhạc nền ban đầu
        if (defaultBGM != null)
            PlayBGM(defaultBGM);
    }

    // ===========================
    // BGM SYSTEM
    // ===========================

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        // Không phát lại nếu bài hát đang chạy
        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    // ===========================
    // SFX SYSTEM
    // ===========================

    public void PlaySFX(string soundName)
    {
        if (string.IsNullOrEmpty(soundName))
            return;

        foreach (SoundData sound in soundEffects)
        {
            if (sound == null) continue;

            if (string.Equals(
                sound.soundName,
                soundName,
                StringComparison.OrdinalIgnoreCase))
            {
                if (sound.clip != null)
                {
                    sfxSource.PlayOneShot(
                        sound.clip,
                        sound.volume
                    );
                }

                return;
            }
        }

        Debug.LogWarning(
            "Không tìm thấy âm thanh: " + soundName
        );
    }

    // Phát trực tiếp một AudioClip
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volume)
        );
    }

    // Dùng trực tiếp cho UI Button
    public void PlayButtonClick()
    {
        PlaySFX("ButtonClick");
    }

    // ===========================
    // VOLUME SYSTEM
    // ===========================

    public void SetMasterVolume(float volume)
    {
        MasterVolume = Mathf.Clamp01(volume);

        AudioListener.volume = MasterVolume;

        PlayerPrefs.SetFloat(MasterKey, MasterVolume);
    }

    public void SetBGMVolume(float volume)
    {
        BGMVolume = Mathf.Clamp01(volume);

        bgmSource.volume = BGMVolume;

        PlayerPrefs.SetFloat(BGMKey, BGMVolume);
    }

    public void SetSFXVolume(float volume)
    {
        SFXVolume = Mathf.Clamp01(volume);

        sfxSource.volume = SFXVolume;

        PlayerPrefs.SetFloat(SFXKey, SFXVolume);
    }

    // ===========================
    // SAVE / LOAD
    // ===========================

    private void LoadVolume()
    {
        SetMasterVolume(
            PlayerPrefs.GetFloat(MasterKey, 1f)
        );

        SetBGMVolume(
            PlayerPrefs.GetFloat(BGMKey, 1f)
        );

        SetSFXVolume(
            PlayerPrefs.GetFloat(SFXKey, 1f)
        );
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}
