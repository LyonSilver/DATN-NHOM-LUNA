using UnityEngine;
using TMPro;

public class DamageTestController : MonoBehaviour
{
    [Header("SYSTEM REFERENCES")]
    [SerializeField] private ComboBreakSystem comboBreakSystem;
    [SerializeField] private StatusHandler playerStatusHandler;

    [Header("ENEMY HP - TEST")]
    [SerializeField] private int maxEnemyHP = 100;

    private int currentEnemyHP;

    [Header("CARD DAMAGE")]
    [SerializeField] private int slashDamage = 10;
    [SerializeField] private int heavyHitDamage = 20;

    [Header("CHAOS MODIFIER")]
    [SerializeField] private bool doubleDamage = false;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI enemyHPText;
    [SerializeField] private TextMeshProUGUI damageText;

    private void Start()
    {
        maxEnemyHP = Mathf.Max(1, maxEnemyHP);

        currentEnemyHP = maxEnemyHP;

        if (comboBreakSystem == null)
        {
            Debug.LogWarning(
                "Chua gan ComboBreakSystem cho DamageTestController!"
            );
        }

        UpdateUI();
    }

    // XỬ LÝ MỘT LẦN TẤN CÔNG
    private void Attack(TimingGrade grade, bool isHeavyHit = false)
    {
        // Không tấn công Enemy đã hết HP
        if (currentEnemyHP <= 0)
        {
            if (damageText != null)
                damageText.text = "Enemy da bi danh bai!";

            return;
        }

        // 1. Xác định sát thương cơ bản
        int baseDamage = isHeavyHit
            ? heavyHitDamage
            : slashDamage;

        // 2. Lấy hệ số Status
        float statusMultiplier = 1f;

        if (playerStatusHandler != null)
        {
            statusMultiplier =
                playerStatusHandler.GetDamageMultiplier();
        }

        // 3. Lấy hệ số Chaos
        float chaosMultiplier = doubleDamage ? 2f : 1f;

        // 4. Tính Damage
        int finalDamage = DamageCalculator.CalculateDamage(
            baseDamage,
            grade,
            statusMultiplier,
            chaosMultiplier
        );

        // 5. Trừ HP của Enemy
        currentEnemyHP = Mathf.Max(
            0,
            currentEnemyHP - finalDamage
        );

        // 6. Cập nhật Combo / Break
        if (comboBreakSystem != null)
        {
            comboBreakSystem.RegisterTiming(
                grade,
                isHeavyHit
            );
        }

        // 7. Hiển thị kết quả
        string cardName = isHeavyHit ? "HEAVY HIT" : "SLASH";

        if (damageText != null)
        {
            damageText.text =
                cardName + " - " + grade +
                "\nDamage: " + finalDamage;
        }

        Debug.Log(
            cardName +
            " | Timing: " + grade +
            " | Damage: " + finalDamage +
            " | Enemy HP: " + currentEnemyHP
        );

        UpdateUI();

        // 8. Kiểm tra Enemy bị đánh bại
        if (currentEnemyHP <= 0)
        {
            Debug.Log("ENEMY DEFEATED!");

            if (damageText != null)
            {
                damageText.text += "\nENEMY DEFEATED!";
            }
        }
    }

    // CẬP NHẬT GIAO DIỆN
    private void UpdateUI()
    {
        if (enemyHPText != null)
        {
            enemyHPText.text =
                "Enemy HP: " +
                currentEnemyHP + "/" + maxEnemyHP;
        }
    }

    // CÁC HÀM TEST

    public void TestGood()
    {
        Attack(TimingGrade.GOOD);
    }

    public void TestGreat()
    {
        Attack(TimingGrade.GREAT);
    }

    public void TestPerfect()
    {
        Attack(TimingGrade.PERFECT);
    }

    public void TestMiss()
    {
        Attack(TimingGrade.MISS);
    }

    public void TestHeavyHit()
    {
        Attack(TimingGrade.PERFECT, true);
    }

    // RESET ENEMY
    public void ResetEnemy()
    {
        currentEnemyHP = maxEnemyHP;

        if (comboBreakSystem != null)
        {
            comboBreakSystem.ResetForNewEnemy();
        }

        if (damageText != null)
        {
            damageText.text = "READY!";
        }

        UpdateUI();
    }
}