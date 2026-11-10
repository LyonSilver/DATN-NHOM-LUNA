using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FantasySettingsPROController : MonoBehaviour
{
    public FantasySettingsManager settings;
    public GameObject audioPage, graphicsPage, settingsPanel;
    public Button save, apply, reset, graphics, audio, music, fullscreen, back, close;
    public TMP_Text musicState;
    bool musicEnabled = true;

    void Start()
    {
        if (save) save.onClick.AddListener(() => settings.SaveSettings());
        if (apply) apply.onClick.AddListener(() => { settings.SaveSettings(); ShowAudio(); });
        if (reset) reset.onClick.AddListener(() => settings.ResetSettings());
        if (graphics) graphics.onClick.AddListener(ShowGraphics);
        if (audio) audio.onClick.AddListener(ShowAudio);
        if (music) music.onClick.AddListener(ToggleMusic);
        if (fullscreen) fullscreen.onClick.AddListener(() => { if (settings.fullscreenToggle) settings.fullscreenToggle.isOn = !settings.fullscreenToggle.isOn; });
        if (back) back.onClick.AddListener(() => settings.Back());
        if (close) close.onClick.AddListener(() => settings.Back());
        musicEnabled = PlayerPrefs.GetInt("FantasySettings.MusicEnabled", 1) == 1;
        UpdateMusic(); ShowAudio();
    }
    public void ShowAudio() { if (audioPage) audioPage.SetActive(true); if (graphicsPage) graphicsPage.SetActive(false); }
    public void ShowGraphics() { if (audioPage) audioPage.SetActive(false); if (graphicsPage) graphicsPage.SetActive(true); }
    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;
        PlayerPrefs.SetInt("FantasySettings.MusicEnabled", musicEnabled ? 1 : 0);
        PlayerPrefs.Save(); UpdateMusic();
    }
    void UpdateMusic()
    {
        if (musicState) musicState.text = musicEnabled ? "Music ON" : "Music OFF";
        // Connect music AudioSources to the Music AudioMixer group to mute them in a real game.
        if (settings && settings.mixer) settings.mixer.SetFloat(settings.musicParameter,
            musicEnabled && settings.musicSlider ? Mathf.Log10(Mathf.Max(0.0001f, settings.musicSlider.value)) * 20f : -80f);
    }
}
