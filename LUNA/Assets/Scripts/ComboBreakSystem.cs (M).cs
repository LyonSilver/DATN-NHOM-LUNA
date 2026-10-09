using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TimingGrade
{
    MISS,
    GOOD,
    GREAT,
    PERFECT
}

public class ComboBreakSystem : MonoBehaviour
{
    [Header("COMBO SYSTEM")]
    [SerializeField] private int currentCombo = 0;

    [Header("BREAK SYSTEM")]
    [SerializeField] private int currentBreak = 0;
    [SerializeField] private int maxBreak = 100;

    private bool isEnemyBroken = false;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI breakText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [SerializeField] private Slider breakSlider;

    public int CurrentCombo => currentCombo;
    public int CurrentBreak => currentBreak;
    public bool IsEnemyBroken => isEnemyBroken;

    private void Start()
    {
        maxBreak = Mathf.Max(1, maxBreak);

        currentCombo = 0;
        currentBreak = 0;
        isEnemyBroken = false;

        if (breakSlider != null)
        {
            breakSlider.minValue = 0;
            breakSlider.maxValue = maxBreak;
        }

        UpdateUI();
    }

    // NHẬN KẾT QUẢ TIMING
    public void RegisterTiming(
        TimingGrade grade,
        bool isHeavyHit = false,
        bool isPerfectCounter = false)
    {
        // MISS: Mất Combo
        if (grade == TimingGrade.MISS)
        {
            currentCombo = 0;

            ShowFeedback("MISS!");

            UpdateUI();
            return;
        }

        // Timing thành công: Tăng Combo
        currentCombo++;

        int breakAmount = 0;

        switch (grade)
        {
            case TimingGrade.GOOD:
                breakAmount = 0;
                break;

            case TimingGrade.GREAT:
                breakAmount = 15;
                break;

            case TimingGrade.PERFECT:
                breakAmount = 30;
                break;
        }

        // Heavy Hit đạt Perfect được cộng thêm Break
        if (isHeavyHit && grade == TimingGrade.PERFECT)
        {
            breakAmount += 25;
        }

        // Trong bản thử nghiệm:
        // Perfect Counter gây Break ngay lập tức.
        if (isPerfectCounter && grade == TimingGrade.PERFECT)
        {
            breakAmount = maxBreak;
        }

        AddBreak(breakAmount);

        if (isEnemyBroken)
        {
            ShowFeedback("ENEMY BREAK!");
        }
        else
        {
            ShowFeedback(grade.ToString());
        }

        UpdateUI();
    }

    // CỘNG BREAK
    private void AddBreak(int amount)
    {
        if (isEnemyBroken)
        {
            return;
        }

        currentBreak += amount;

        currentBreak = Mathf.Clamp(
            currentBreak,
            0,
            maxBreak
        );

        if (currentBreak >= maxBreak)
        {
            TriggerBreak();
        }
    }

    // KÍCH HOẠT BREAK
    private void TriggerBreak()
    {
        isEnemyBroken = true;

        currentBreak = 0;

        Debug.Log("ENEMY BREAK!");
    }

    // GỌI KHI ĐẾN LƯỢT ENEMY
    // Trả về true nếu Enemy phải bỏ lượt.
    public bool TryConsumeEnemyTurn()
    {
        if (!isEnemyBroken)
        {
            return false;
        }

        isEnemyBroken = false;

        ShowFeedback("Enemy mất 1 lượt!");

        UpdateUI();

        return true;
    }

    // RESET KHI GẶP ENEMY MỚI
    public void ResetForNewEnemy()
    {
        currentCombo = 0;
        currentBreak = 0;
        isEnemyBroken = false;

        ShowFeedback("NEW ENEMY");

        UpdateUI();
    }

    // CẬP NHẬT GIAO DIỆN
    private void UpdateUI()
    {
        if (comboText != null)
        {
            comboText.text = "COMBO: " + currentCombo;
        }

        if (breakText != null)
        {
            breakText.text = isEnemyBroken
                ? "BREAK!"
                : "BREAK: " + currentBreak + "/" + maxBreak;
        }

        if (breakSlider != null)
        {
            breakSlider.value = currentBreak;
        }
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    // CÁC HÀM TEST TRONG UNITY

    public void TestGood()
    {
        RegisterTiming(TimingGrade.GOOD);
    }

    public void TestGreat()
    {
        RegisterTiming(TimingGrade.GREAT);
    }

    public void TestPerfect()
    {
        RegisterTiming(TimingGrade.PERFECT);
    }

    public void TestMiss()
    {
        RegisterTiming(TimingGrade.MISS);
    }

    public void TestHeavyHit()
    {
        RegisterTiming(TimingGrade.PERFECT, true);
    }

    public void TestEnemyTurn()
    {
        bool skipTurn = TryConsumeEnemyTurn();

        if (!skipTurn)
        {
            ShowFeedback("Enemy hành động bình thường.");
        }
    }
}