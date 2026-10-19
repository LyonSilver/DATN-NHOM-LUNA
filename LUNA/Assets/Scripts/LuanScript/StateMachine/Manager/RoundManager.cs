
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
    [Header("Round Settings")]
    [SerializeField] private int currentRound = 1;
    [SerializeField] private int maxRound = 2;

    [Header("Managers")]
    [SerializeField] private BattleStateMachine battleStateMachine;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private EnemyIntentManager enemyIntentManager;

    [Header("Card Cinematic")]
    [SerializeField] private CardManager cardManager;
    [SerializeField] private PlayerCardCinematic playerCardCinematic;

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

    private bool battleFinished;
    private bool isChangingRound;
    private bool isTransitioning;
    private bool currentEnemyRewardClaimed;
    private bool resultPanelShown;
    private bool isLoadingScene;
    private bool playerWon;

    public int CurrentRound => currentRound;
    public bool BattleFinished => battleFinished;

    // =========================================
    // START
    // =========================================

    private void Start()
    {
        Time.timeScale = 1f;

        maxRound = Mathf.Max(1, maxRound);
        currentRound = Mathf.Clamp(currentRound, 1, maxRound);

        battleFinished = false;
        isChangingRound = false;
        isTransitioning = false;
        currentEnemyRewardClaimed = false;
        resultPanelShown = false;
        isLoadingScene = false;
        playerWon = false;

        if (finishPanel != null)
            finishPanel.SetActive(false);

        SetGameplayUI(true);

        UpdateRoundUI();
        UpdateRewardUI();

        if (enemySpawner == null ||
            battleStateMachine == null)
        {
            Debug.LogError(
                "RoundManager: Thiếu EnemySpawner hoặc BattleStateMachine!"
            );
            return;
        }

        // Spawn Enemy Round đầu.
        if (!SpawnRoundEnemy(currentRound))
        {
            Debug.LogError(
                "RoundManager: Không thể bắt đầu trận!"
            );
            return;
        }

        battleStateMachine.StartPlayerTurn();

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
            return false;

        // Round 1 -> Element 0.
        // Round 2 -> Element 1.
        int enemyIndex = round - 1;

        enemySpawner.SpawnEnemy(enemyIndex);

        EnemyHealth enemyHealth =
            enemySpawner.GetCurrentEnemyHealth();

        EnemyAttack enemyAttack =
            enemySpawner.GetCurrentEnemyAttack();

        if (enemyHealth == null || enemyAttack == null)
        {
            Debug.LogError(
                $"RoundManager: Enemy Round {round} " +
                "thiếu EnemyHealth hoặc EnemyAttack!"
            );
            return false;
        }

        Debug.Log(
            $"Spawn Enemy Round {round} | " +
            $"HP: {enemyHealth.CurrentHP}/{enemyHealth.MaxHP}"
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

        if (battleFinished || isTransitioning || isChangingRound)
            return;

        if (!enemyIntentManager.PrepareIntent())
        {
            Debug.LogWarning(
                "RoundManager: Không thể chuẩn bị Enemy Intent!"
            );
        }
    }

    private void ClearEnemyIntent()
    {
        if (enemyIntentManager != null)
            enemyIntentManager.ClearIntent();
    }

    // =========================================
    // CHECK CARD CINEMATIC
    // =========================================

    private bool IsCardActionPlaying()
    {
        // CardManager đang xử lý việc sử dụng bài.
        if (cardManager != null && cardManager.IsUsingCard)
            return true;

        // Camera / Player vẫn đang trong Cinematic.
        if (playerCardCinematic != null &&
            playerCardCinematic.IsPlaying)
        {
            return true;
        }

        return false;
    }

    // =========================================
    // WAIT CARD + ENEMY DEATH
    // =========================================

    private IEnumerator WaitForDeathAndCardAction()
    {
        float deathDuration = 0f;

        if (enemySpawner != null)
        {
            EnemyHealth enemy =
                enemySpawner.GetCurrentEnemyHealth();

            if (enemy != null)
            {
                deathDuration = Mathf.Max(
                    0f,
                    enemy.DeathAnimationDuration
                );
            }
        }

        // Bắt đầu đếm thời gian ngay khi Coroutine chạy.
        float deathFinishTime =
            Time.time + deathDuration;

        Debug.Log(
            $"RoundManager: Chờ Enemy Death ({deathDuration}s) " +
            "và Player Card Cinematic..."
        );

        // Chờ ĐỒNG THỜI:
        // 1. Enemy phát xong Death.
        // 2. Player dùng bài xong.
        // 3. Camera quay về bình thường.
        while (Time.time < deathFinishTime ||
               IsCardActionPlaying())
        {
            yield return null;
        }

        Debug.Log(
            "RoundManager: Enemy Death và Card Cinematic đã hoàn tất!"
        );
    }

    // =========================================
    // ENEMY DEFEATED
    // =========================================

    public void EnemyDefeated()
    {
        if (battleFinished ||
            currentEnemyRewardClaimed ||
            isChangingRound)
        {
            return;
        }

        currentEnemyRewardClaimed = true;

        ClearEnemyIntent();

        // Cộng Reward một lần cho mỗi Enemy.
        currentCoin += coinPerRound;
        currentExp += expPerRound;

        UpdateRewardUI();

        Debug.Log(
            $"RoundManager: Enemy chết! " +
            $"+{coinPerRound} Coin, +{expPerRound} EXP"
        );

        // =====================================
        // FINAL ROUND
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
    // NEXT ROUND
    // =========================================

    private IEnumerator NextRoundCoroutine()
    {
        Debug.Log(
            "RoundManager: Chuẩn bị chuyển Round..."
        );

        ClearEnemyIntent();

        // Đợi Player chạy về, Camera Zoom Out
        // và Enemy phát xong Death Animation.
        yield return WaitForDeathAndCardAction();

        if (battleFinished)
        {
            isChangingRound = false;
            yield break;
        }

        // Đến đây mới chuyển FSM sang NextRound.
        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.NextRound
            );
        }

        // =====================================
        // HIDE OLD ROUND UI
        // =====================================

        if (cardUI != null)
            cardUI.SetActive(false);

        if (enemyUI != null)
            enemyUI.SetActive(false);

        // =====================================
        // DESPAWN OLD ENEMY
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
        // WAIT BEFORE NEXT ROUND
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
        // SPAWN NEXT ROUND
        // =====================================

        int nextRound = currentRound + 1;

        if (!SpawnRoundEnemy(nextRound))
        {
            Debug.LogError(
                "RoundManager: Spawn Enemy Round mới thất bại!"
            );

            isChangingRound = false;
            yield break;
        }

        currentRound = nextRound;

        UpdateRoundUI();

        // =====================================
        // RESTORE GAMEPLAY UI
        // =====================================

        if (enemyUI != null)
            enemyUI.SetActive(true);

        if (cardUI != null)
            cardUI.SetActive(true);

        // Mở khóa Reward cho Enemy mới.
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

        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.Defeat
            );
        }

        // CardEffectManager đã chờ Player Death.
        ShowFinishPanel(defeatText, false);
    }

    // =========================================
    // WIN BATTLE
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
            "========== PLAYER VICTORY =========="
        );

        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.Victory
            );
        }

        StartCoroutine(ShowVictoryPanelAfterDeath());
    }

    // =========================================
    // SHOW VICTORY
    // =========================================

    private IEnumerator ShowVictoryPanelAfterDeath()
    {
        // Chờ Player Card Cinematic và Enemy Death.
        yield return WaitForDeathAndCardAction();

        // Xóa Enemy cuối cùng.
        if (enemySpawner != null)
        {
            enemySpawner.DespawnCurrentEnemy();
        }

        UpdateRewardUI();

        ShowFinishPanel(victoryText, true);

        Debug.Log(
            "RoundManager: Victory Panel đã hiển thị!"
        );
    }

    // =========================================
    // RESULT PANEL
    // =========================================

    private void ShowFinishPanel(
        string result,
        bool isVictory)
    {
        if (resultPanelShown)
            return;

        resultPanelShown = true;

        // =====================================
        // RESULT TITLE
        // =====================================

        if (resultTitleText != null)
            resultTitleText.text = result;

        // =====================================
        // RESULT BUTTONS
        // =====================================

        // Home: luôn hiện.
        if (homeButton != null)
            homeButton.SetActive(true);

        // Again: chỉ hiện khi thua.
        if (againButton != null)
            againButton.SetActive(!isVictory);

        // Next: chỉ hiện khi thắng.
        if (nextButton != null)
            nextButton.SetActive(isVictory);

        // =====================================
        // REWARDS
        // =====================================

        UpdateRewardUI();

        // =====================================
        // HIDE GAMEPLAY
        // =====================================

        SetGameplayUI(false);

        // =====================================
        // SHOW PANEL
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

        Time.timeScale = 0f;
    }

    // =========================================
    // CONTINUE TO NEXT SCENE
    // =========================================

    public void ContinueToNextScene()
    {
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

        if (!Application.CanStreamedLevelBeLoaded(
                nextSceneName))
        {
            Debug.LogError(
                $"RoundManager: Scene {nextSceneName} " +
                "chưa được thêm vào Build Profiles!"
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

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, transitionDelay)
        );

        Time.timeScale = 1f;

        SceneManager.LoadScene(nextSceneName);
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
    // UPDATE ROUND UI
    // =========================================

    private void UpdateRoundUI()
    {
        if (roundText == null)
            return;

        roundText.text =
            "ROUND:\n" +
            currentRound + "/" + maxRound;
    }

    // =========================================
    // UPDATE REWARD UI
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
