
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class RoundManager : MonoBehaviour
{
    [Header("Battle Systems")]
    [SerializeField] private BattleStateMachine battleStateMachine;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private EnemyIntentManager enemyIntentManager;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private PlayerCardCinematic playerCardCinematic;
    [SerializeField] private BattleResultTracker battleStats;

    [Header("Stage Test - Optional")]
    [Tooltip("Dùng khi chạy trực tiếp MapBT mà không qua SelectMap")]
    [SerializeField] private StageData testStage;

    [Header("Round")]
    [SerializeField] private float spawnDelay = 2f;

    [Header("Gameplay UI")]
    [SerializeField] private TMP_Text roundText;

    [FormerlySerializedAs("cardUI")]
    [SerializeField] private GameObject battleHUD;

    [Header("Result UI")]
    [SerializeField] private GameObject finishPanel;

    [Header("Mission System")]
    [SerializeField] private MissionRewardManager missionRewardManager;

    // =====================================
    // STAGE DATA
    // =====================================

    private StageData activeStage;
    private Difficulty activeDifficulty;

    private int currentRound = 1;
    private int maxRound = 1;

    private int currentCoin;
    private int currentExp;

    // =====================================
    // BATTLE STATES
    // =====================================

    private bool battleFinished;
    private bool isChangingRound;
    private bool enemyRewardClaimed;
    private bool resultPanelShown;
    private bool stageRewardsSaved;
    private bool playerWon;

    // =====================================
    // PUBLIC PROPERTIES
    // =====================================

    public int CurrentRound => currentRound;
    public bool BattleFinished => battleFinished;

    public StageData ActiveStage => activeStage;
    public Difficulty ActiveDifficulty => activeDifficulty;

    // =====================================
    // START
    // =====================================

    private void Start()
    {
        Time.timeScale = 1f;

        battleFinished = false;
        isChangingRound = false;
        enemyRewardClaimed = false;
        resultPanelShown = false;
        stageRewardsSaved = false;
        playerWon = false;

        currentCoin = 0;
        currentExp = 0;

        // Lấy Stage từ SelectMap.
        // Nếu chạy trực tiếp MapBT thì dùng Test Stage.
        activeStage = GameSession.CurrentStage;

        if (activeStage == null)
            activeStage = testStage;

        activeDifficulty = GameSession.CurrentDifficulty;

        if (activeStage == null)
        {
            Debug.LogError(
                "RoundManager: Chưa có StageData! " +
                "Hãy chọn Stage ở SelectMap hoặc gán Test Stage."
            );
            return;
        }

        maxRound = activeStage.RoundCount;
        currentRound = 1;

        if (maxRound <= 0)
        {
            Debug.LogError(
                "RoundManager: Enemy Indices đang trống!"
            );
            return;
        }

        if (battleStateMachine == null || enemySpawner == null)
        {
            Debug.LogError(
                "RoundManager: Thiếu FSM hoặc EnemySpawner!"
            );
            return;
        }

        // Reset thống kê trận đấu.
        if (battleStats != null)
            battleStats.ResetStats();

        // Reset UI.
        if (finishPanel != null)
            finishPanel.SetActive(false);

        SetGameplayUI(true);
        UpdateRoundUI();

        // Bắt đầu Round 1.
        if (!SpawnRoundEnemy(currentRound))
            return;

        battleStateMachine.StartPlayerTurn();
        PrepareEnemyIntent();

        Debug.Log(
            $"START STAGE: {activeStage.displayName} | " +
            $"Difficulty: {activeDifficulty} | " +
            $"Rounds: {maxRound}"
        );
    }

    // =====================================
    // SPAWN ENEMY
    // =====================================

    private bool SpawnRoundEnemy(int round)
    {
        if (enemySpawner == null || activeStage == null)
            return false;

        int enemyIndex = activeStage.GetEnemyIndex(round);

        if (enemyIndex < 0)
        {
            Debug.LogError(
                $"RoundManager: Enemy Index không hợp lệ ở Round {round}!"
            );
            return false;
        }

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
            $"ROUND {round}/{maxRound} | " +
            $"Enemy Index: {enemyIndex} | " +
            $"HP: {health.CurrentHP}/{health.MaxHP}"
        );

        return true;
    }

    // =====================================
    // ENEMY INTENT
    // =====================================

    private void PrepareEnemyIntent()
    {
        if (battleFinished || isChangingRound)
            return;

        if (enemyIntentManager != null)
        {
            enemyIntentManager.PrepareIntent();
        }
    }

    private void ClearEnemyIntent()
    {
        if (enemyIntentManager != null)
            enemyIntentManager.ClearIntent();
    }

    // =====================================
    // CHECK CARD ACTION
    // =====================================

    private bool IsCardActionPlaying()
    {
        if (cardManager != null && cardManager.IsUsingCard)
            return true;

        if (playerCardCinematic != null &&
            playerCardCinematic.IsPlaying)
            return true;

        return false;
    }

    // =====================================
    // WAIT FOR ENEMY DEATH + CINEMATIC
    // =====================================

    private IEnumerator WaitForBattleAction()
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

        float endTime = Time.unscaledTime + deathDuration;

        while (Time.unscaledTime < endTime ||
               IsCardActionPlaying())
        {
            yield return null;
        }
    }

    // =====================================
    // ENEMY DEFEATED
    // =====================================

    public void EnemyDefeated()
    {
        // Ngăn một Enemy được tính nhiều lần.
        if (battleFinished ||
            isChangingRound ||
            enemyRewardClaimed)
        {
            return;
        }

        enemyRewardClaimed = true;

        // Mission 1: ghi nhận theo Round trước khi chuyển Round.
        if (missionRewardManager != null)
            missionRewardManager.RegisterEnemyDefeated(currentRound);

        ClearEnemyIntent();

        // Ghi Kill Count.
        if (battleStats != null)
            battleStats.AddKill();

        Debug.Log(
            $"Enemy Defeated | Round {currentRound}/{maxRound}"
        );

        // Đã đánh hết Round.
        if (currentRound >= maxRound)
        {
            WinBattle();
            return;
        }

        // Chuyển Round tiếp theo.
        isChangingRound = true;
        StartCoroutine(NextRoundCoroutine());
    }

    // =====================================
    // NEXT ROUND
    // =====================================

    private IEnumerator NextRoundCoroutine()
    {
        // Đợi Player đánh xong và Enemy chết.
        yield return WaitForBattleAction();

        if (battleFinished)
        {
            isChangingRound = false;
            yield break;
        }

        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.NextRound
            );
        }

        // Ẩn HUD tạm thời.
        if (battleHUD != null)
            battleHUD.SetActive(false);

        // Xóa Enemy cũ.
        enemySpawner.DespawnCurrentEnemy();

        // Đợi trước khi Spawn Enemy mới.
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, spawnDelay)
        );

        if (battleFinished)
        {
            isChangingRound = false;
            yield break;
        }

        int nextRound = currentRound + 1;

        if (!SpawnRoundEnemy(nextRound))
        {
            Debug.LogError(
                "RoundManager: Không thể Spawn Enemy mới!"
            );

            isChangingRound = false;
            yield break;
        }

        currentRound = nextRound;

        UpdateRoundUI();

        enemyRewardClaimed = false;
        isChangingRound = false;

        // Hiện lại HUD.
        if (battleHUD != null)
            battleHUD.SetActive(true);

        // Bắt đầu lượt Player.
        if (battleStateMachine != null)
            battleStateMachine.StartPlayerTurn();

        PrepareEnemyIntent();

        Debug.Log(
            $"========== ROUND {currentRound}/{maxRound} =========="
        );
    }

    // =====================================
    // PLAYER DEFEATED
    // =====================================

    public void PlayerDefeated()
    {
        if (battleFinished)
            return;

        battleFinished = true;
        playerWon = false;

        ClearEnemyIntent();

        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.Defeat
            );
        }

        ShowFinishPanel(false);
    }

    // =====================================
    // VICTORY
    // =====================================

    private void WinBattle()
    {
        if (battleFinished)
            return;

        battleFinished = true;
        playerWon = true;

        ClearEnemyIntent();

        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.Victory
            );
        }

        // Tính thưởng theo Stage và độ khó.
        currentCoin =
            activeStage.GetRewardCoin(activeDifficulty);

        currentExp =
            activeStage.GetRewardExp(activeDifficulty);

        StartCoroutine(ShowVictoryCoroutine());
    }

    // =====================================
    // SHOW VICTORY
    // =====================================

    private IEnumerator ShowVictoryCoroutine()
    {
        // Đợi Enemy Death và Card Cinematic.
        yield return WaitForBattleAction();

        if (enemySpawner != null)
            enemySpawner.DespawnCurrentEnemy();

        // Chốt kết quả Mission trước khi lưu số sao của Stage.
        if (missionRewardManager != null)
            missionRewardManager.RegisterBattleEnded(true, currentRound);

        // Lưu kết quả trận thắng một lần.
        SaveStageVictory();

        // Hiện Result.
        ShowFinishPanel(true);
    }

    // =====================================
    // SAVE STAGE VICTORY
    // =====================================

    private void SaveStageVictory()
    {
        if (stageRewardsSaved || activeStage == null)
            return;

        stageRewardsSaved = true;

        _SaveData save = _SaveSystem.Load();

        // Cộng Coin và EXP.
        save.coin += Mathf.Max(0, currentCoin);
        save.exp += Mathf.Max(0, currentExp);

        // Xử lý lên Level theo hệ thống hiện tại.
        save.playerLevel = Mathf.Max(
            1,
            save.playerLevel
        );

        int safety = 0;

        while (safety < 1000)
        {
            int requiredExp = 50 * save.playerLevel;

            if (save.exp < requiredExp)
                break;

            save.exp -= requiredExp;
            save.playerLevel++;

            safety++;
        }

        // Số sao lấy từ kết quả Mission thay vì hardcode 2 sao.
        int stars = missionRewardManager != null &&
                    missionRewardManager.FinalReward != null
            ? missionRewardManager.FinalReward.stars
            : 0;
        int score = 0;

        // ReportStageResult đã gọi Save()
        // bên trong _SaveSystem.
        _SaveSystem.ReportStageResult(
            activeStage.stageId,
            stars,
            score
        );

        Debug.Log(
            $"SAVED STAGE: {activeStage.stageId} | " +
            $"Coin +{currentCoin} | EXP +{currentExp}"
        );
    }

    // =====================================
    // RESULT PANEL
    // =====================================

    private void ShowFinishPanel(bool isVictory)
    {
        if (resultPanelShown)
            return;

        if (finishPanel == null)
        {
            Debug.LogError(
                "RoundManager: Chưa gán ResultPanel!"
            );
            return;
        }

        resultPanelShown = true;

        // Chốt kết quả Mission; hàm có cơ chế chống gọi lặp.
        // MissionRewardManager sẽ chuyển dữ liệu sang MissionResultUI.
        if (missionRewardManager != null)
        {
            missionRewardManager.RegisterBattleEnded(isVictory, currentRound);
        }
        else
        {
            Debug.LogWarning(
                "RoundManager: Chưa gán MissionRewardManager!"
            );
        }

        SetGameplayUI(false);
        finishPanel.SetActive(true);

        Time.timeScale = 0f;

        Debug.Log(
            isVictory ? "BATTLE VICTORY" : "BATTLE DEFEAT"
        );
    }

    // =====================================
    // SHOW / HIDE GAMEPLAY UI
    // =====================================

    private void SetGameplayUI(bool active)
    {
        if (battleHUD != null)
            battleHUD.SetActive(active);

        if (roundText != null)
            roundText.gameObject.SetActive(active);
    }

    // =====================================
    // UPDATE ROUND TEXT
    // =====================================

    private void UpdateRoundUI()
    {
        if (roundText != null)
        {
            roundText.text =
                $"ROUND:\n{currentRound}/{maxRound}";
        }
    }

    // =====================================
    // RESULT BUTTONS - SCENE NAVIGATION
    // =====================================

    // Next: sau Victory trở về SelectMap.
    public void ContinueToNextScene()
    {
        if (!battleFinished ||
            !playerWon ||
            !resultPanelShown)
        {
            return;
        }

        ReturnToSelectMap();
    }

    // Again: chơi lại Stage hiện tại.
    public void RetryBattle()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    // Home: trở về SelectMap.
    public void ReturnToSelectMap()
    {
        LoadScene("SelectMap");
    }

    private void LoadScene(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"RoundManager: Scene '{sceneName}' " +
                "chưa được thêm vào Build Profiles!"
            );
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // =====================================
    // GETTERS - GIỮ TƯƠNG THÍCH SCRIPT CŨ
    // =====================================

    public int GetCurrentCoin() => currentCoin;
    public int GetCurrentExp() => currentExp;
    public int GetCurrentRound() => currentRound;
    public bool IsBattleFinished() => battleFinished;
    public bool IsChangingRound() => isChangingRound;
}