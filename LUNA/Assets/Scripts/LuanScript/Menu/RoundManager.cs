using UnityEngine;
using TMPro;

public class RoundManager : MonoBehaviour
{
    [Header("Round")]
    [SerializeField] private int currentRound = 1;
    [SerializeField] private int maxRound = 2;

    [Header("Managers")]
    [SerializeField] private CardEffectManager cardEffectManager;
    [SerializeField] private BattleStateMachine battleStateMachine;

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

    private bool battleFinished = false;
    private bool currentEnemyRewardClaimed = false;

    private void Start()
    {
        Time.timeScale = 1f;

        if (finishPanel != null)
            finishPanel.SetActive(false);

        SetGameplayUI(true);

        UpdateRoundUI();
        UpdateRewardUI();

        if (battleStateMachine != null)
        {
            battleStateMachine.StartPlayerTurn();
        }
        else
        {
            Debug.LogError(
                "RoundManager: Chưa gán BattleStateMachine!"
            );
        }
    }

    // Gọi khi HP của kẻ địch giảm về 0.
    public void EnemyDefeated()
    {
        if (battleFinished || currentEnemyRewardClaimed)
            return;

        currentEnemyRewardClaimed = true;

        Debug.Log("RoundManager: Enemy đã bị đánh bại!");

        // Nhận thưởng của round hiện tại.
        currentCoin += coinPerRound;
        currentExp += expPerRound;

        UpdateRewardUI();

        Debug.Log(
            "Reward: +" + coinPerRound + " Coin, +" +
            expPerRound + " EXP"
        );

        // Nếu đã hoàn thành round cuối.
        if (currentRound >= maxRound)
        {
            WinBattle();
            return;
        }

        // Chuyển sang round tiếp theo.
        currentRound++;

        Debug.Log(
            "RoundManager: Chuyển sang Round " +
            currentRound + "/" + maxRound
        );

        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.NextRound
            );
        }

        if (cardEffectManager != null)
        {
            cardEffectManager.NextRoundEnemyHP();
        }
        else
        {
            Debug.LogError(
                "RoundManager: Chưa gán CardEffectManager!"
            );
            return;
        }

        UpdateRoundUI();

        // Cho phép nhận thưởng khi hạ kẻ địch ở round mới.
        currentEnemyRewardClaimed = false;

        // Bắt đầu lượt người chơi của round mới.
        if (battleStateMachine != null)
        {
            battleStateMachine.StartPlayerTurn();
        }
    }

    // Gọi khi HP người chơi giảm về 0.
    public void PlayerDefeated()
    {
        if (battleFinished)
            return;

        battleFinished = true;

        Debug.Log("RoundManager: PLAYER DEFEATED!");

        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.Defeat
            );
        }

        ShowFinishPanel();
    }

    private void WinBattle()
    {
        if (battleFinished)
            return;

        battleFinished = true;

        Debug.Log("RoundManager: VICTORY!");
        Debug.Log(
            "TỔNG PHẦN THƯỞNG: " +
            currentCoin + " Coin / " +
            currentExp + " EXP"
        );

        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.Victory
            );
        }

        ShowFinishPanel();
    }

    private void ShowFinishPanel()
    {
        SetGameplayUI(false);

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

        Time.timeScale = 0f;
    }

    private void SetGameplayUI(bool isActive)
    {
        if (roundUI != null)
            roundUI.SetActive(isActive);

        if (cardUI != null)
            cardUI.SetActive(isActive);

        if (playerUI != null)
            playerUI.SetActive(isActive);

        if (enemyUI != null)
            enemyUI.SetActive(isActive);
    }

    private void UpdateRoundUI()
    {
        if (roundText != null)
        {
            roundText.text =
                "ROUND:\n" + currentRound + "/" + maxRound;
        }
    }

    private void UpdateRewardUI()
    {
        if (coinText != null)
            coinText.text = "Coin: " + currentCoin;

        if (expText != null)
            expText.text = "EXP: " + currentExp;
    }

    public int GetCurrentCoin()
    {
        return currentCoin;
    }

    public int GetCurrentExp()
    {
        return currentExp;
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public bool IsBattleFinished()
    {
        return battleFinished;
    }
}