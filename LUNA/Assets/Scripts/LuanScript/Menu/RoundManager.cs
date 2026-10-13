using UnityEngine;
using TMPro;

public class RoundManager : MonoBehaviour
{
    [Header("Round")]
    [SerializeField] private int currentRound = 1;
    [SerializeField] private int maxRound = 2;

    [Header("Enemy")]
    [SerializeField] private CardEffectManager cardEffectManager;

    [Header("Round UI")]
    [SerializeField] private TMP_Text roundText;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject roundUI;
    [SerializeField] private GameObject cardUI;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject enemyUI;

    [Header("Finish Panel")]
    [SerializeField] private GameObject finishPanel;

    [Header("Reward")]
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text expText;

    [SerializeField] private int currentCoin = 0;
    [SerializeField] private int currentExp = 0;

    [SerializeField] private int coinPerRound = 50;
    [SerializeField] private int expPerRound = 100;

    [Header("Battle State Machine")]
    [SerializeField] private BattleStateMachine battleStateMachine;


    private void Start()
    {
        // Ẩn Finish Panel khi bắt đầu
        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }

        // Đảm bảo các UI gameplay đang bật
        SetGameplayUI(true);

        // Đảm bảo game không bị freeze
        Time.timeScale = 1f;

        UpdateRoundUI();
        UpdateRewardUI();
    }


    public void EnemyDefeated()
    {
        Debug.Log("RoundManager: Enemy đã chết!");


        // =========================================
        // NHẬN REWARD CHO ROUND
        // =========================================

        currentCoin += coinPerRound;
        currentExp += expPerRound;

        UpdateRewardUI();

        Debug.Log(
            "Reward Round " +
            currentRound +
            ": +" +
            coinPerRound +
            " Coin, +" +
            expPerRound +
            " EXP"
        );

        Debug.Log(
            "Tổng hiện tại: " +
            currentCoin +
            " Coin, " +
            currentExp +
            " EXP"
        );


        // =========================================
        // ROUND CUỐI CÙNG
        // =========================================

        if (currentRound >= maxRound)
        {
            Debug.Log(
                "RoundManager: Đã hoàn thành tất cả Round!"
            );

            Debug.Log(
                "TỔNG PHẦN THƯỞNG: " +
                currentCoin +
                " Coin / " +
                currentExp +
                " EXP"
            );

            // Tắt UI gameplay
            SetGameplayUI(false);

            // Hiện Finish Panel
            if (finishPanel != null)
            {
                finishPanel.SetActive(true);
            }
            else
            {
                Debug.LogError(
                    "RoundManager: Chưa gán Finish Panel!"
                );
            }

            // Freeze game
            Time.timeScale = 0f;

            return;
        }


        // =========================================
        // CHUYỂN SANG ROUND TIẾP THEO
        // =========================================

        currentRound++;

        Debug.Log(
            "RoundManager: Chuyển sang Round " +
            currentRound +
            "/" +
            maxRound
        );


        if (cardEffectManager != null)
        {
            cardEffectManager.NextRoundEnemyHP();
        }
        else
        {
            Debug.LogError(
                "RoundManager: Chưa gán CardEffectManager!"
            );
        }


        UpdateRoundUI();
    }


    // =========================================
    // BẬT / TẮT GAMEPLAY UI
    // =========================================

    private void SetGameplayUI(bool isActive)
    {
        if (roundUI != null)
        {
            roundUI.SetActive(isActive);
        }

        if (cardUI != null)
        {
            cardUI.SetActive(isActive);
        }

        if (playerUI != null)
        {
            playerUI.SetActive(isActive);
        }

        if (enemyUI != null)
        {
            enemyUI.SetActive(isActive);
        }
    }


    // =========================================
    // UPDATE ROUND UI
    // =========================================

    private void UpdateRoundUI()
    {
        if (roundText != null)
        {
            roundText.text =
                "ROUND:\n" +
                currentRound +
                "/" +
                maxRound;
        }
        else
        {
            Debug.LogWarning(
                "RoundManager: Chưa gán Round Text!"
            );
        }
    }


    // =========================================
    // UPDATE REWARD UI
    // =========================================

    private void UpdateRewardUI()
    {
        if (coinText != null)
        {
            coinText.text =
                "Coin: " +
                currentCoin;
        }

        if (expText != null)
        {
            expText.text =
                "EXP: " +
                currentExp;
        }
    }


    // =========================================
    // GETTERS
    // =========================================

    public int GetCurrentCoin()
    {
        return currentCoin;
    }

    public int GetCurrentExp()
    {
        return currentExp;
    }

    public void PlayerDefeated()
    {
        Debug.Log("PLAYER DEFEATED!");

        // Chuyển FSM sang trạng thái thua
        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.Defeat
            );
        }

        // Ẩn giao diện chiến đấu
        SetGameplayUI(false);

        // Hiện bảng kết thúc
        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
        }

        // Dừng game
        Time.timeScale = 0f;
    }
}