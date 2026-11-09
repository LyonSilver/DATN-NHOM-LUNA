using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class ChaosHeroData
{
    public string heroName = "Aether";
    public string title = "Keeper of the Astral Gate";
    [TextArea(2, 5)] public string description = "A mysterious guardian who bends the power of ancient stars.";
    public Sprite portrait;
    public Sprite fullArtwork;
    public string rarity = "LEGENDARY";
    public int level = 1;
    [Range(0,100)] public int attack = 78;
    [Range(0,100)] public int defense = 62;
    [Range(0,100)] public int magic = 95;
    public string skillName = "Astral Nova";
    [TextArea] public string skillDescription = "Unleashes a burst of astral energy.";
}

public class ChaosCard25DCharacterSelection : MonoBehaviour
{
    [Header("Hero data")]
    public List<ChaosHeroData> heroes = new List<ChaosHeroData>();
    public int selectedIndex;
    [Header("Main artwork")]
    public Image heroArtwork;
    public Image heroPortrait;
    [Header("Text")]
    public TMP_Text heroNameText, heroTitleText, heroDescriptionText, rarityText, levelText;
    public TMP_Text attackText, defenseText, magicText, skillNameText, skillDescriptionText, statusText;
    [Header("Hero list")]
    public Transform heroListRoot;
    public Button heroButtonPrefab;
    public Button confirmButton, backButton, previousButton, nextButton;
    [Header("Navigation")]
    public string confirmSceneName = "LienketLobby";
    public string backSceneName = "MENU_GAME";
    public bool loadSceneOnConfirm = true;
    [Header("Selection feedback")]
    public Color selectedButtonColor = new Color(0.30f, 0.16f, 0.58f, 1f);
    public Color normalButtonColor = new Color(0.055f, 0.075f, 0.16f, 0.96f);
    public UnityEvent<int, string> onHeroConfirmed;
    readonly List<Button> spawnedButtons = new List<Button>();
    Vector3 artworkBaseScale = Vector3.one;
    Coroutine introRoutine;

    void Start()
    {
        if (heroArtwork) artworkBaseScale = heroArtwork.rectTransform.localScale;
        BuildHeroList();
        if (heroes.Count > 0) SelectHero(Mathf.Clamp(selectedIndex, 0, heroes.Count - 1));
        if (confirmButton) confirmButton.onClick.AddListener(ConfirmHero);
        if (backButton) backButton.onClick.AddListener(GoBack);
        if (previousButton) previousButton.onClick.AddListener(SelectPrevious);
        if (nextButton) nextButton.onClick.AddListener(SelectNext);
        if (heroArtwork) introRoutine = StartCoroutine(ArtworkBreath());
    }

    void OnDestroy()
    {
        if (confirmButton) confirmButton.onClick.RemoveListener(ConfirmHero);
        if (backButton) backButton.onClick.RemoveListener(GoBack);
        if (previousButton) previousButton.onClick.RemoveListener(SelectPrevious);
        if (nextButton) nextButton.onClick.RemoveListener(SelectNext);
    }

    public void BuildHeroList()
    {
        if (!heroListRoot || !heroButtonPrefab) return;
        for (int i = heroListRoot.childCount - 1; i >= 0; i--)
            Destroy(heroListRoot.GetChild(i).gameObject);
        spawnedButtons.Clear();

        for (int i = 0; i < heroes.Count; i++)
        {
            int index = i;
            Button button = Instantiate(heroButtonPrefab, heroListRoot);
            button.gameObject.SetActive(true);
            button.name = $"HeroButton_{i + 1:00}_{heroes[i].heroName}";
            var texts = button.GetComponentsInChildren<TMP_Text>(true);
            if (texts.Length > 0) texts[0].text = heroes[i].heroName.ToUpperInvariant();
            if (texts.Length > 1) texts[1].text = heroes[i].rarity;
            var portrait = button.transform.Find("Portrait")?.GetComponent<Image>();
            if (portrait)
            {
                portrait.sprite = heroes[i].portrait;
                portrait.enabled = heroes[i].portrait != null;
            }
            button.onClick.AddListener(() => SelectHero(index));
            spawnedButtons.Add(button);
        }
        RefreshButtonStates();
    }

    public void SelectHero(int index)
    {
        if (heroes == null || heroes.Count == 0 || index < 0 || index >= heroes.Count) return;
        selectedIndex = index;
        ChaosHeroData h = heroes[index];
        SetImage(heroArtwork, h.fullArtwork ? h.fullArtwork : h.portrait);
        SetImage(heroPortrait, h.portrait);
        SetText(heroNameText, h.heroName.ToUpperInvariant());
        SetText(heroTitleText, h.title);
        SetText(heroDescriptionText, h.description);
        SetText(rarityText, h.rarity);
        SetText(levelText, $"LEVEL {h.level:00}");
        SetText(attackText, h.attack.ToString("00"));
        SetText(defenseText, h.defense.ToString("00"));
        SetText(magicText, h.magic.ToString("00"));
        SetText(skillNameText, h.skillName);
        SetText(skillDescriptionText, h.skillDescription);
        SetText(statusText, "HERO READY FOR SELECTION");
        RefreshButtonStates();
    }

    public void SelectPrevious() => SelectHero((selectedIndex - 1 + heroes.Count) % Mathf.Max(1, heroes.Count));
    public void SelectNext() => SelectHero((selectedIndex + 1) % Mathf.Max(1, heroes.Count));

    public void ConfirmHero()
    {
        if (heroes == null || heroes.Count == 0)
        {
            SetText(statusText, "NO HERO DATA");
            return;
        }
        var hero = heroes[Mathf.Clamp(selectedIndex, 0, heroes.Count - 1)];
        PlayerPrefs.SetInt("ChaosCard.SelectedHeroIndex", selectedIndex);
        PlayerPrefs.SetString("ChaosCard.SelectedHeroName", hero.heroName);
        PlayerPrefs.Save();
        SetText(statusText, $"SELECTED  •  {hero.heroName.ToUpperInvariant()}");
        onHeroConfirmed?.Invoke(selectedIndex, hero.heroName);

        if (loadSceneOnConfirm && !string.IsNullOrWhiteSpace(confirmSceneName))
        {
            if (Application.CanStreamedLevelBeLoaded(confirmSceneName))
                SceneManager.LoadScene(confirmSceneName);
            else
                SetText(statusText, $"SAVED: {hero.heroName}  •  SCENE NOT IN BUILD SETTINGS");
        }
    }

    public void GoBack()
    {
        if (!string.IsNullOrWhiteSpace(backSceneName) && Application.CanStreamedLevelBeLoaded(backSceneName))
            SceneManager.LoadScene(backSceneName);
        else SetText(statusText, "BACK SCENE NOT IN BUILD SETTINGS");
    }

    void RefreshButtonStates()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            var colors = spawnedButtons[i].colors;
            colors.normalColor = i == selectedIndex ? selectedButtonColor : normalButtonColor;
            colors.highlightedColor = Color.Lerp(colors.normalColor, new Color(0.65f,0.50f,1f), 0.25f);
            colors.pressedColor = Color.Lerp(colors.normalColor, Color.black, 0.25f);
            spawnedButtons[i].colors = colors;
            var marker = spawnedButtons[i].transform.Find("SelectedMarker");
            if (marker) marker.gameObject.SetActive(i == selectedIndex);
        }
    }

    IEnumerator ArtworkBreath()
    {
        while (true)
        {
            if (heroArtwork)
            {
                float t = (Mathf.Sin(Time.unscaledTime * 1.25f) + 1f) * 0.5f;
                heroArtwork.rectTransform.localScale = artworkBaseScale * Mathf.Lerp(0.985f, 1.015f, t);
            }
            yield return null;
        }
    }

    static void SetText(TMP_Text t, string value) { if (t) t.text = value; }
    static void SetImage(Image i, Sprite s)
    {
        if (!i) return;
        i.sprite = s;
        i.enabled = s != null;
        i.preserveAspect = true;
    }
}

public class ChaosCard25DParallax : MonoBehaviour
{
    public float strength = 12f;
    public float smooth = 4f;
    RectTransform rect;
    Vector2 startPos;
    void Awake() { rect = GetComponent<RectTransform>(); if (rect) startPos = rect.anchoredPosition; }
    void Update()
    {
        if (!rect) return;
        Vector2 mouse = new Vector2(Input.mousePosition.x / Mathf.Max(1, Screen.width), Input.mousePosition.y / Mathf.Max(1, Screen.height));
        Vector2 target = startPos + new Vector2((mouse.x - 0.5f) * strength, (mouse.y - 0.5f) * strength);
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, target, Time.unscaledDeltaTime * smooth);
    }
}

public class ChaosCard25DParticleField : MonoBehaviour
{
    public int particleCount = 34;
    public Color tint = new Color(0.58f, 0.72f, 1f, 0.65f);
    public float driftSpeed = 18f;
    readonly List<RectTransform> particles = new List<RectTransform>();
    readonly List<Vector2> speeds = new List<Vector2>();
    RectTransform root;

    void Start()
    {
        root = transform as RectTransform;
        if (!root) return;
        for (int i = 0; i < particleCount; i++)
        {
            var go = new GameObject("ArcaneDust", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            float size = UnityEngine.Random.Range(2.5f, 7f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchorMin = rt.anchorMax = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value);
            rt.anchoredPosition = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = new Color(tint.r, tint.g, tint.b, UnityEngine.Random.Range(0.15f, 0.7f));
            image.raycastTarget = false;
            particles.Add(rt);
            speeds.Add(new Vector2(UnityEngine.Random.Range(-0.15f,0.15f), UnityEngine.Random.Range(0.15f,0.7f)));
        }
    }
    void Update()
    {
        if (!root) return;
        for (int i=0;i<particles.Count;i++)
        {
            var p=particles[i];
            p.anchorMin += speeds[i] * (driftSpeed * Time.unscaledDeltaTime / 1000f);
            if (p.anchorMin.y > 1.05f) p.anchorMin = new Vector2(UnityEngine.Random.value, -0.04f);
            if (p.anchorMin.x < -0.05f) p.anchorMin = new Vector2(1.04f,p.anchorMin.y);
            if (p.anchorMin.x > 1.05f) p.anchorMin = new Vector2(-0.04f,p.anchorMin.y);
            p.anchorMax = p.anchorMin;
        }
    }
}
