using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class StagePanel : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] NodeSelector selector;
    [SerializeField] CanvasGroup group;
    [SerializeField] RectTransform panelRect;

    [Header("Chữ")]
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] TMP_Text bestText;
    [SerializeField] TMP_Text rewardText;

    [Header("Nút")]
    [SerializeField] Button easyButton;
    [SerializeField] Button normalButton;
    [SerializeField] Button hardButton;
    [SerializeField] Button startButton;
    [SerializeField] Button closeButton;

    [Header("Chuyển động")]
    [SerializeField] float slideTime = 0.25f;

    static readonly float[] multipliers = { 1f, 1.5f, 2f };

    StageData current;
    Difficulty difficulty = Difficulty.Normal;
    float shownX, hiddenX;
    bool isOpen;
    int openFrame;
    Coroutine slide;

    static Color Hex(string h)
    {
        ColorUtility.TryParseHtmlString("#" + h, out Color c);
        return c;
    }

    void Awake()
    {
        shownX = panelRect.anchoredPosition.x;
        hiddenX = shownX + panelRect.rect.width + 80f;
        SetPosX(hiddenX);
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        if (easyButton != null) easyButton.onClick.AddListener(() => SetDifficulty(Difficulty.Easy));
        if (normalButton != null) normalButton.onClick.AddListener(() => SetDifficulty(Difficulty.Normal));
        if (hardButton != null) hardButton.onClick.AddListener(() => SetDifficulty(Difficulty.Hard));
        if (startButton != null) startButton.onClick.AddListener(StartStage);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    void OnEnable()
    {
        if (selector != null) selector.StageChosen += Open;
    }

    void OnDisable()
    {
        if (selector != null) selector.StageChosen -= Open;
    }

    void SetPosX(float x)
    {
        Vector2 p = panelRect.anchoredPosition;
        p.x = x;
        panelRect.anchoredPosition = p;
    }

    public void Open(StageData data)
    {
        if (data == null) return;
        current = data;
        isOpen = true;
        openFrame = Time.frameCount;
        difficulty = GameSession.CurrentDifficulty;

        if (selector != null) selector.InputLocked = true;

        titleText.text = data.displayName + (data.isBossStage ? "  [BOSS]" : "");
        descriptionText.text = data.description;

        StageProgress p = _SaveSystem.Load().Get(data.stageId);
        bestText.text = p.cleared
            ? "Tốt nhất: " + p.bestStars + "/3 sao   |   Điểm cao: " + p.highScore
            : "Chưa hoàn thành";

        Refresh();
        StartSlide(true);
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        if (selector != null) selector.InputLocked = false;
        StartSlide(false);
    }

    void SetDifficulty(Difficulty d)
    {
        difficulty = d;
        GameSession.CurrentDifficulty = d;
        Refresh();
    }

    void Paint(Button b, bool on)
    {
        if (b == null) return;
        b.image.color = on ? Hex("F5C242") : Hex("4A4A55");
        TMP_Text t = b.GetComponentInChildren<TMP_Text>();
        if (t != null) t.color = on ? Hex("1B0F3A") : Hex("F4EEDC");
    }

    void Refresh()
    {
        Paint(easyButton, difficulty == Difficulty.Easy);
        Paint(normalButton, difficulty == Difficulty.Normal);
        Paint(hardButton, difficulty == Difficulty.Hard);

        if (current == null) return;
        float m = multipliers[(int)difficulty];
        int coin = Mathf.RoundToInt(current.rewardCoin * m);
        int exp = Mathf.RoundToInt(current.rewardExp * m);
        string r = "Thưởng: " + coin + " Coin, " + exp + " EXP";
        if (current.isBossStage) r += "\nBoss: có cơ hội nhận Card";
        rewardText.text = r;
    }

    void StartStage()
    {
        if (current == null) return;
        GameSession.CurrentStage = current;
        GameSession.CurrentDifficulty = difficulty;

        string scene = current.sceneToLoad;
        if (Application.CanStreamedLevelBeLoaded(scene))
            SceneManager.LoadScene(scene);
        else
            Debug.LogWarning("Scene \"" + scene + "\" chưa có trong Build Settings (xem Bước 9).");
    }

    void StartSlide(bool show)
    {
        if (slide != null) StopCoroutine(slide);
        slide = StartCoroutine(Slide(show));
    }

    IEnumerator Slide(bool show)
    {
        group.blocksRaycasts = show;
        group.interactable = show;

        float fromX = panelRect.anchoredPosition.x;
        float toX = show ? shownX : hiddenX;
        float fromA = group.alpha;
        float toA = show ? 1f : 0f;

        float e = 0f;
        while (e < slideTime)
        {
            e += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, e / slideTime);
            SetPosX(Mathf.Lerp(fromX, toX, k));
            group.alpha = Mathf.Lerp(fromA, toA, k);
            yield return null;
        }
        SetPosX(toX);
        group.alpha = toA;
    }

    void Update()
    {
        if (!isOpen || Time.frameCount == openFrame) return;
        Keyboard k = Keyboard.current;
        if (k == null) return;

        if (k.escapeKey.wasPressedThisFrame) Close();
        else if (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame) StartStage();
        else if (k.digit1Key.wasPressedThisFrame) SetDifficulty(Difficulty.Easy);
        else if (k.digit2Key.wasPressedThisFrame) SetDifficulty(Difficulty.Normal);
        else if (k.digit3Key.wasPressedThisFrame) SetDifficulty(Difficulty.Hard);
    }
}