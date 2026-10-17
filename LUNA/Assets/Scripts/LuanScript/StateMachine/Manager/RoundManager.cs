
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

    [Header("Enemy Intent")]
    [SerializeField] private EnemyIntentManager enemyIntentManager;

    [Header("Enemy Spawn")]
    [SerializeField] private float spawnDelay = 2f;

    [Header("Round UI")]
    [SerializeField] private TMP_Text roundText;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject roundUI;
    [SerializeField] private GameObject cardUI;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject enemyUI;

    [Header("Result Panel")]
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private TMP_Text resultTitleText;

    [Header("Result Buttons")]
    [SerializeField] private GameObject homeButton;
    [SerializeField] private GameObject againButton;
    [SerializeField] private GameObject nextButton;

    [Header("Result Text")]
    [SerializeField] private string victoryText = "VICTORY";
    [SerializeField] private string defeatText = "DEFEAT";

    [Header("Reward UI")]
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text expText;

    [Header("Reward Settings")]
    [SerializeField] private int currentCoin = 0;
    [SerializeField] private int currentExp = 0;

    [SerializeField] private int coinPerRound = 50;
    [SerializeField] private int expPerRound = 100;

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private float transitionDelay = 0.5f;

    [Header("Reward Panel Controller")]
[SerializeField] private ResultRewardUI resultRewardUI;

    private bool isTransitioning = false;
    private bool isChangingRound = false;
    private bool battleFinished = false;
    private bool currentEnemyRewardClaimed = false;
    private bool isLoadingScene = false;
    private bool playerWon = false;
    private bool resultPanelShown = false;

    // =========================================
    // START
    // =========================================

    private void Start()
    {
        Time.timeScale = 1f;

        maxRound = Mathf.Max(1, maxRound);
        currentRound = Mathf.Clamp(
            currentRound, 1, maxRound
        );

        isTransitioning = false;
        isChangingRound = false;
        battleFinished = false;
        currentEnemyRewardClaimed = false;
        isLoadingScene = false;
        playerWon = false;
        resultPanelShown = false;

        // Ẩn ResultPanel khi bắt đầu trận.
        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }

        // Hiển thị Gameplay.
        SetGameplayUI(true);

        UpdateRoundUI();
        UpdateRewardUI();

        if (battleStateMachine == null ||
            enemySpawner == null)
        {
            Debug.LogError(
                "RoundManager: Thiếu BattleStateMachine " +
                "hoặc EnemySpawner!"
            );

            return;
        }

        // Spawn Enemy đầu tiên.
        if (!SpawnRoundEnemy(currentRound))
        {
            Debug.LogError(
                "RoundManager: Không thể bắt đầu trận!"
            );

            return;
        }

        // Bắt đầu Player Turn.
        battleStateMachine.StartPlayerTurn();

        // Enemy chuẩn bị hành động trước.
        PrepareEnemyIntent();

        Debug.Log(
            $"========== ROUND {currentRound}/{maxRound} =========="
        );
    }

    // =========================================
    // SPAWN ENEMY
    // =========================================

    private bool SpawnRoundEnemy(int round)
    {
        if (enemySpawner == null)
        {
            Debug.LogError(
                "RoundManager: Chưa gán EnemySpawner!"
            );

            return false;
        }

        // Round 1 = Index 0.
        // Round 2 = Index 1.
        int enemyIndex = round - 1;

        enemySpawner.SpawnEnemy(enemyIndex);

        EnemyHealth health =
            enemySpawner.GetCurrentEnemyHealth();

        EnemyAttack attack =
            enemySpawner.GetCurrentEnemyAttack();

        if (health == null || attack == null)
        {
            Debug.LogError(
                $"RoundManager: Enemy Round {round} " +
                "thiếu EnemyHealth hoặc EnemyAttack!"
            );

            return false;
        }

        Debug.Log(
            $"RoundManager: Spawn Enemy Round {round} | " +
            $"HP: {health.CurrentHP}/{health.MaxHP}"
        );

        return true;
    }

    // =========================================
    // ENEMY INTENT
    // =========================================

    private void PrepareEnemyIntent()
    {
        if (enemyIntentManager == null)
        {
            Debug.LogWarning(
                "RoundManager: Chưa gán EnemyIntentManager!"
            );

            return;
        }

        if (battleFinished || isTransitioning)
            return;

        bool success =
            enemyIntentManager.PrepareIntent();

        if (success)
        {
            Debug.Log(
                "RoundManager: Enemy đã chuẩn bị Intent!"
            );
        }
        else
        {
            Debug.LogWarning(
                "RoundManager: Chuẩn bị Intent thất bại!"
            );
        }
    }

    private void ClearEnemyIntent()
    {
        if (enemyIntentManager != null)
        {
            enemyIntentManager.ClearIntent();
        }
    }

    // =========================================
    // ENEMY DEFEATED
    // =========================================

    public void EnemyDefeated()
    {
        // Không xử lý trùng.
        if (battleFinished ||
            currentEnemyRewardClaimed ||
            isChangingRound)
        {
            return;
        }

        currentEnemyRewardClaimed = true;

        ClearEnemyIntent();

        Debug.Log(
            "RoundManager: Enemy đã bị đánh bại!"
        );

        // =====================================
        // REWARD
        // =====================================

        currentCoin += coinPerRound;
        currentExp += expPerRound;

        UpdateRewardUI();

        Debug.Log(
            $"Reward: +{coinPerRound} Coin | " +
            $"+{expPerRound} EXP"
        );

        // =====================================
        // CHECK LAST ROUND
        // =====================================

        if (currentRound >= maxRound)
        {
            WinBattle();
            return;
        }

        // =====================================
        // NEXT ROUND
        // =====================================

        isChangingRound = true;

        StartCoroutine(NextRoundCoroutine());
    }

    // =========================================
    // WAIT ENEMY DEATH ANIMATION
    // =========================================

    private IEnumerator WaitForEnemyDeathAnimation()
    {
        if (enemySpawner == null)
            yield break;

        EnemyHealth enemy =
            enemySpawner.GetCurrentEnemyHealth();

        if (enemy == null)
        {
            Debug.LogWarning(
                "RoundManager: Không tìm thấy EnemyHealth!"
            );

            yield break;
        }

        float duration = Mathf.Max(
            0f,
            enemy.DeathAnimationDuration
        );

        Debug.Log(
            $"RoundManager: Chờ Death Animation " +
            $"{duration} giây."
        );

        if (duration > 0f)
        {
            yield return new WaitForSeconds(duration);
        }

        Debug.Log(
            "RoundManager: Death Animation hoàn tất!"
        );
    }

    // =========================================
    // NEXT ROUND
    // =========================================

    private IEnumerator NextRoundCoroutine()
    {
        Debug.Log(
            "RoundManager: Đang chuyển Round..."
        );

        // Chuyển FSM sang NextRound.
        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.NextRound
            );
        }

        ClearEnemyIntent();

        // =====================================
        // WAIT DEATH ANIMATION
        // =====================================

        yield return WaitForEnemyDeathAnimation();

        if (battleFinished)
        {
            isChangingRound = false;
            yield break;
        }

        // =====================================
        // HIDE OLD UI
        // =====================================

        if (cardUI != null)
        {
            cardUI.SetActive(false);
        }

        if (enemyUI != null)
        {
            enemyUI.SetActive(false);
        }

        // =====================================
        // DESPAWN ENEMY
        // =====================================

        if (enemySpawner == null)
        {
            Debug.LogError(
                "RoundManager: Thiếu EnemySpawner!"
            );

            isChangingRound = false;
            yield break;
        }

        enemySpawner.DespawnCurrentEnemy();

        // =====================================
        // SPAWN DELAY
        // =====================================

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, spawnDelay)
        );

        if (battleFinished)
        {
            isChangingRound = false;
            yield break;
        }

        // =====================================
        // SPAWN NEXT ENEMY
        // =====================================

        int nextRound = currentRound + 1;

        if (!SpawnRoundEnemy(nextRound))
        {
            Debug.LogError(
                "RoundManager: Spawn Enemy mới thất bại!"
            );

            isChangingRound = false;
            yield break;
        }

        // Chỉ tăng Round sau khi Spawn thành công.
        currentRound = nextRound;

        UpdateRoundUI();

        // =====================================
        // RESTORE UI
        // =====================================

        if (enemyUI != null)
        {
            enemyUI.SetActive(true);
        }

        if (cardUI != null)
        {
            cardUI.SetActive(true);
        }

        // Cho phép Enemy mới nhận Reward.
        currentEnemyRewardClaimed = false;
        isChangingRound = false;

        // =====================================
        // START PLAYER TURN
        // =====================================

        if (battleStateMachine != null)
        {
            battleStateMachine.StartPlayerTurn();
        }

        PrepareEnemyIntent();

        Debug.Log(
            $"========== ROUND {currentRound}/{maxRound} =========="
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
        playerWon = false;

        ClearEnemyIntent();

        Debug.Log(
            "========== PLAYER DEFEATED =========="
        );

        // Chuyển FSM sang Defeat.
        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.Defeat
            );
        }

        // =====================================
        // SHOW DEFEAT PANEL
        // =====================================

        // Home: hiện.
        // Again: hiện.
        // Next: ẩn.
        ShowFinishPanel(defeatText, false);
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
        playerWon = true;

        ClearEnemyIntent();

        Debug.Log(
            "========== VICTORY =========="
        );

        Debug.Log(
            $"TOTAL REWARD: " +
            $"{currentCoin} Coin / {currentExp} EXP"
        );

        // Chuyển FSM sang Victory.
        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.Victory
            );
        }

        // Chờ Enemy phát Death rồi hiện Panel.
        StartCoroutine(ShowVictoryPanelAfterDeath());
    }

    // =========================================
    // SHOW VICTORY PANEL AFTER DEATH
    // =========================================

    private IEnumerator ShowVictoryPanelAfterDeath()
    {
        // Chờ Death Animation của Enemy cuối.
        yield return WaitForEnemyDeathAnimation();

        // Xóa Enemy cuối cùng.
        if (enemySpawner != null)
        {
            enemySpawner.DespawnCurrentEnemy();
        }

        // Cập nhật Reward.
        UpdateRewardUI();

        // =====================================
        // SHOW VICTORY PANEL
        // =====================================

        // Home: hiện.
        // Again: ẩn.
        // Next: hiện.
        ShowFinishPanel(victoryText, true);

        Debug.Log(
            "RoundManager: Đã hiển thị Victory Panel!"
        );
    }

    // =========================================
    // SHOW RESULT PANEL
    // =========================================

    private void ShowFinishPanel(
        string result,
        bool isVictory)
    {
        // Không hiện nhiều lần.
        if (resultPanelShown)
            return;

        resultPanelShown = true;

        // =====================================
        // RESULT TITLE
        // =====================================

        if (resultTitleText != null)
        {
            resultTitleText.text = result;
        }

        // =====================================
        // BUTTON SETTINGS
        // =====================================

        // Home luôn được hiển thị.
        if (homeButton != null)
        {
            homeButton.SetActive(true);
        }

        // Thua: hiển thị Again.
        // Thắng: ẩn Again.
        if (againButton != null)
        {
            againButton.SetActive(!isVictory);
        }

        // Thắng: hiển thị Next.
        // Thua: ẩn Next.
        if (nextButton != null)
        {
            nextButton.SetActive(isVictory);
        }

        // =====================================
        // UPDATE REWARD
        // =====================================

        UpdateRewardUI();

        // =====================================
        // HIDE GAMEPLAY UI
        // =====================================

        SetGameplayUI(false);

        // =====================================
        // SHOW RESULT PANEL
        // =====================================

        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "RoundManager: Chưa gán ResultPanel!"
            );
        }

        Debug.Log(
            isVictory
                ? "ResultPanel: VICTORY!"
                : "ResultPanel: DEFEAT!"
        );

        // Dừng Gameplay sau khi hiện Panel.
        Time.timeScale = 0f;
    }

    // =========================================
    // CONTINUE TO NEXT SCENE
    // =========================================

    public void ContinueToNextScene()
    {
        // Chỉ cho Next khi đã thắng.
        if (!battleFinished ||
            !playerWon ||
            !resultPanelShown ||
            isLoadingScene)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogError(
                "RoundManager: Chưa nhập Next Scene Name!"
            );

            return;
        }

        StartCoroutine(LoadNextSceneAfterDelay());
    }

    // =========================================
    // LOAD NEXT SCENE
    // =========================================

    private IEnumerator LoadNextSceneAfterDelay()
    {
        isLoadingScene = true;

        // Realtime để chạy khi TimeScale = 0.
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, transitionDelay)
        );

        Time.timeScale = 1f;

        Debug.Log(
            $"RoundManager: Load Scene {nextSceneName}"
        );

        SceneManager.LoadScene(nextSceneName);
    }

    // =========================================
    // GAMEPLAY UI
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
    // ROUND UI
    // =========================================

    private void UpdateRoundUI()
    {
        if (roundText != null)
        {
            roundText.text =
                "ROUND:\n" +
                currentRound + "/" + maxRound;
        }
    }

    // =========================================
    // REWARD UI
    // =========================================

    private void UpdateRewardUI()
    {
        if (coinText != null)
        {
            coinText.text =
                "Coin: " + currentCoin;
        }

        if (expText != null)
        {
            expText.text =
                "EXP: " + currentExp;
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