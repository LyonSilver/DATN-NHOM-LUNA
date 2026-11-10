using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FantasySettingsManager : MonoBehaviour
{
    [Header("UI")]
    public Slider masterSlider, musicSlider, sfxSlider;
    public TMP_Text masterValue, musicValue, sfxValue;
    public TMP_Dropdown resolutionDropdown, qualityDropdown;
    public Toggle fullscreenToggle;
    public Button saveButton, resetButton, backButton;
    [Header("Optional Audio Mixer")]
    public AudioMixer mixer;
    public string masterParameter = "MasterVolume";
    public string musicParameter = "MusicVolume";
    public string sfxParameter = "SFXVolume";
    [Header("Navigation")]
    public string fallbackSceneName = "MainMenu";

    const string Prefix = "FantasySettings.";
    readonly List<Resolution> resolutions = new List<Resolution>();
    bool loading;
    int previousSceneIndex = -1;

    void Awake()
    {
        previousSceneIndex = PlayerPrefs.GetInt(Prefix + "PreviousScene", -1);
        PopulateOptions();
        LoadSettings();
        if (masterSlider) masterSlider.onValueChanged.AddListener(v => { UpdateAudioLabels(); ApplyAudio(); });
        if (musicSlider) musicSlider.onValueChanged.AddListener(v => { UpdateAudioLabels(); ApplyAudio(); });
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(v => { UpdateAudioLabels(); ApplyAudio(); });
        if (resolutionDropdown) resolutionDropdown.onValueChanged.AddListener(_ => ApplyGraphics());
        if (qualityDropdown) qualityDropdown.onValueChanged.AddListener(_ => ApplyGraphics());
        if (fullscreenToggle) fullscreenToggle.onValueChanged.AddListener(_ => ApplyGraphics());
        if (saveButton) saveButton.onClick.AddListener(SaveSettings);
        if (resetButton) resetButton.onClick.AddListener(ResetSettings);
        if (backButton) backButton.onClick.AddListener(Back);
    }

    void PopulateOptions()
    {
        resolutions.Clear();
        if (resolutionDropdown)
        {
            resolutionDropdown.ClearOptions();
            var names = new List<string>();
            foreach (var r in Screen.resolutions)
            {
                bool exists = resolutions.Exists(x => x.width == r.width && x.height == r.height);
                if (exists) continue;
                resolutions.Add(r);
                names.Add(r.width + " x " + r.height);
            }
            if (names.Count == 0) names.Add(Screen.width + " x " + Screen.height);
            resolutionDropdown.AddOptions(names);
        }
        if (qualityDropdown)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        }
    }

    public void LoadSettings()
    {
        loading = true;
        if (masterSlider) masterSlider.value = PlayerPrefs.GetFloat(Prefix + "Master", 1f);
        if (musicSlider) musicSlider.value = PlayerPrefs.GetFloat(Prefix + "Music", 0.8f);
        if (sfxSlider) sfxSlider.value = PlayerPrefs.GetFloat(Prefix + "SFX", 0.8f);
        if (fullscreenToggle) fullscreenToggle.isOn = PlayerPrefs.GetInt(Prefix + "Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        if (qualityDropdown && qualityDropdown.options.Count > 0)
            qualityDropdown.value = Mathf.Clamp(PlayerPrefs.GetInt(Prefix + "Quality", QualitySettings.GetQualityLevel()), 0, qualityDropdown.options.Count - 1);
        if (resolutionDropdown && resolutions.Count > 0)
        {
            int width = PlayerPrefs.GetInt(Prefix + "Width", Screen.width);
            int height = PlayerPrefs.GetInt(Prefix + "Height", Screen.height);
            int idx = resolutions.FindIndex(r => r.width == width && r.height == height);
            if (idx < 0) idx = resolutions.FindIndex(r => r.width == Screen.width && r.height == Screen.height);
            resolutionDropdown.value = Mathf.Max(0, idx);
        }
        loading = false;
        UpdateAudioLabels();
        ApplyAudio();
        ApplyGraphics();
    }

    void UpdateAudioLabels()
    {
        if (masterValue && masterSlider) masterValue.text = Mathf.RoundToInt(masterSlider.value * 100) + "%";
        if (musicValue && musicSlider) musicValue.text = Mathf.RoundToInt(musicSlider.value * 100) + "%";
        if (sfxValue && sfxSlider) sfxValue.text = Mathf.RoundToInt(sfxSlider.value * 100) + "%";
    }

    void ApplyAudio()
    {
        if (loading) return;
        float master = masterSlider ? masterSlider.value : 1f;
        float music = musicSlider ? musicSlider.value : 0.8f;
        float sfx = sfxSlider ? sfxSlider.value : 0.8f;
        if (mixer)
        {
            mixer.SetFloat(masterParameter, ToDb(master));
            mixer.SetFloat(musicParameter, ToDb(music));
            mixer.SetFloat(sfxParameter, ToDb(sfx));
        }
        else AudioListener.volume = master;
    }

    static float ToDb(float linear) => Mathf.Log10(Mathf.Max(0.0001f, linear)) * 20f;

    void ApplyGraphics()
    {
        if (loading) return;
        if (qualityDropdown && qualityDropdown.options.Count > 0)
            QualitySettings.SetQualityLevel(qualityDropdown.value, true);
        bool fullscreen = fullscreenToggle && fullscreenToggle.isOn;
        if (resolutionDropdown && resolutions.Count > 0)
        {
            Resolution r = resolutions[Mathf.Clamp(resolutionDropdown.value, 0, resolutions.Count - 1)];
            Screen.SetResolution(r.width, r.height, fullscreen);
        }
        else Screen.fullScreen = fullscreen;
    }

    public void SaveSettings()
    {
        if (masterSlider) PlayerPrefs.SetFloat(Prefix + "Master", masterSlider.value);
        if (musicSlider) PlayerPrefs.SetFloat(Prefix + "Music", musicSlider.value);
        if (sfxSlider) PlayerPrefs.SetFloat(Prefix + "SFX", sfxSlider.value);
        if (fullscreenToggle) PlayerPrefs.SetInt(Prefix + "Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        if (qualityDropdown) PlayerPrefs.SetInt(Prefix + "Quality", qualityDropdown.value);
        if (resolutionDropdown && resolutions.Count > 0)
        {
            var r = resolutions[Mathf.Clamp(resolutionDropdown.value, 0, resolutions.Count - 1)];
            PlayerPrefs.SetInt(Prefix + "Width", r.width);
            PlayerPrefs.SetInt(Prefix + "Height", r.height);
        }
        PlayerPrefs.Save();
    }

    public void ResetSettings()
    {
        loading = true;
        if (masterSlider) masterSlider.value = 1f;
        if (musicSlider) musicSlider.value = 0.8f;
        if (sfxSlider) sfxSlider.value = 0.8f;
        if (qualityDropdown && qualityDropdown.options.Count > 0) qualityDropdown.value = Mathf.Clamp(QualitySettings.names.Length - 1, 0, qualityDropdown.options.Count - 1);
        if (fullscreenToggle) fullscreenToggle.isOn = false;
        if (resolutionDropdown && resolutions.Count > 0) resolutionDropdown.value = resolutions.Count - 1;
        loading = false;
        UpdateAudioLabels(); ApplyAudio(); ApplyGraphics(); SaveSettings();
    }

    public void Back()
    {
        SaveSettings();
        if (previousSceneIndex >= 0 && previousSceneIndex < SceneManager.sceneCountInBuildSettings &&
            previousSceneIndex != SceneManager.GetActiveScene().buildIndex)
        {
            SceneManager.LoadScene(previousSceneIndex);
            return;
        }
        if (Application.CanStreamedLevelBeLoaded(fallbackSceneName)) SceneManager.LoadScene(fallbackSceneName);
        else Debug.LogWarning("Fantasy Settings: Add a MainMenu scene to Build Settings, or set fallbackSceneName.");
    }

    public static void OpenSettings(string settingsSceneName = "SettingsScene")
    {
        if (!Application.CanStreamedLevelBeLoaded(settingsSceneName))
        {
            Debug.LogError("Fantasy Settings: Add SettingsScene to Build Settings / Build Profiles.");
            return;
        }
        PlayerPrefs.SetInt(Prefix + "PreviousScene", SceneManager.GetActiveScene().buildIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene(settingsSceneName);
    }
}
