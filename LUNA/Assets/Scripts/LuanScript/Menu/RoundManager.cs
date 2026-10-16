
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class RoundManager : MonoBehaviour
{
    [Header("Round")]
    [SerializeField] private int currentRound = 1;
    [SerializeField] private int maxRound = 2;

    [Header("Managers")]
    [SerializeField] private BattleStateMachine battleStateMachine;
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Enemy Spawn")]
    [SerializeField] private float spawnDelay = 2f;

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

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private float transitionDelay = 0.5f;

    private bool isTransitioning = false;
    private bool isChangingRound = false;
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

        // Spawn Enemy đầu tiên.
        if (enemySpawner != null)
        {
            enemySpawner.SpawnEnemy(currentRound - 1);
        }
        else
        {
            Debug.LogError(
                "RoundManager: Chưa gán EnemySpawner!"
            );
            return;
        }

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

    // =========================================
    // ENEMY DEFEATED
    // =========================================

    public void EnemyDefeated()
    {
        if (battleFinished ||
            currentEnemyRewardClaimed ||
            isChangingRound)
            return;

        currentEnemyRewardClaimed = true;

        Debug.Log("RoundManager: Enemy đã bị đánh bại!");

        // Nhận thưởng Round hiện tại.
        currentCoin += coinPerRound;
        currentExp += expPerRound;

        UpdateRewardUI();

        Debug.Log(
            "Reward: +" + coinPerRound + " Coin, +" +
            expPerRound + " EXP"
        );

        // Nếu là Round cuối thì Victory.
        if (currentRound >= maxRound)
        {
            WinBattle();
            return;
        }

        // Chuyển sang Round tiếp theo.
        StartCoroutine(NextRoundCoroutine());
    }

    // =========================================
    // NEXT ROUND
    // =========================================

    private IEnumerator NextRoundCoroutine()
    {
        isChangingRound = true;

        Debug.Log(
            "RoundManager: Đang chuyển sang Round mới..."
        );

        // Chuyển FSM sang trạng thái NextRound.
        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.NextRound
            );
        }

        // Tạm ẩn Card và Enemy HP UI.
        if (cardUI != null)
            cardUI.SetActive(false);

        if (enemyUI != null)
            enemyUI.SetActive(false);

        if (enemySpawner == null)
        {
            Debug.LogError(
                "RoundManager: Chưa gán EnemySpawner!"
            );

            isChangingRound = false;
            yield break;
        }

        // Xóa Enemy cũ.
        enemySpawner.DespawnCurrentEnemy();

        // Chờ trước khi Spawn Enemy mới.
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, spawnDelay)
        );

        if (battleFinished)
            yield break;

        // Tăng Round.
        currentRound++;

        UpdateRoundUI();

        Debug.Log(
            "RoundManager: Round " +
            currentRound + "/" + maxRound
        );

        // Hiện lại Enemy HP UI.
        if (enemyUI != null)
            enemyUI.SetActive(true);

        // Spawn Enemy mới.
        // Round 1 = Element 0
        // Round 2 = Element 1
        enemySpawner.SpawnEnemy(currentRound - 1);

        // Hiện lại Card.
        if (cardUI != null)
            cardUI.SetActive(true);

        // Cho phép nhận thưởng ở Round mới.
        currentEnemyRewardClaimed = false;
        isChangingRound = false;

        // Bắt đầu Player Turn.
        if (battleStateMachine != null)
        {
            battleStateMachine.StartPlayerTurn();
        }

        Debug.Log(
            "RoundManager: Bắt đầu Player Turn!"
        );
    }

    // =========================================
    // PLAYER DEFEATED
    // =========================================

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

    // =========================================
    // VICTORY
    // =========================================

    private void WinBattle()
    {
        if (battleFinished || isTransitioning)
            return;

        battleFinished = true;
        isTransitioning = true;

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

        StartCoroutine(LoadNextSceneAfterDelay());
    }

    // =========================================
    // LOAD NEXT SCENE
    // =========================================

    private IEnumerator LoadNextSceneAfterDelay()
    {
        SetGameplayUI(false);

        yield return new WaitForSecondsRealtime(
            transitionDelay
        );

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError(
                "RoundManager: Chưa nhập tên Scene tiếp theo!"
            );

            isTransitioning = false;
            yield break;
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(nextSceneName);
    }

    // =========================================
    // FINISH PANEL
    // =========================================

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

    // =========================================
    // GAMEPLAY UI
    // =========================================

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

    // =========================================
    // ROUND UI
    // =========================================

    private void UpdateRoundUI()
    {
        if (roundText != null)
        {
            roundText.text =
                "ROUND:\n" + currentRound + "/" + maxRound;
        }
    }

    // =========================================
    // REWARD UI
    // =========================================

    private void UpdateRewardUI()
    {
        if (coinText != null)
            coinText.text = "Coin: " + currentCoin;

        if (expText != null)
            expText.text = "EXP: " + currentExp;
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

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public bool IsBattleFinished()
    {
        return battleFinished;
    }

    public bool IsChangingRound()
    {
        return isChangingRound;
    }
}
