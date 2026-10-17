
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

        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }

        SetGameplayUI(true);

        UpdateRoundUI();
        UpdateRewardUI();

        // Spawn Enemy đầu tiên.
        if (!SpawnRoundEnemy(currentRound))
        {
            Debug.LogError(
                "RoundManager: Không thể bắt đầu Round!"
            );
            return;
        }

        if (battleStateMachine == null)
        {
            Debug.LogError(
                "RoundManager: Chưa gán BattleStateMachine!"
            );
            return;
        }

        // Bắt đầu Player Turn.
        battleStateMachine.StartPlayerTurn();

        // Enemy chọn trước hành động.
        PrepareEnemyIntent();

        Debug.Log(
            $"RoundManager: Bắt đầu Round " +
            $"{currentRound}/{maxRound}"
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

        // Round 1 = Element 0
        // Round 2 = Element 1
        int enemyIndex = round - 1;

        enemySpawner.SpawnEnemy(enemyIndex);

        // Kiểm tra Enemy đã được tạo.
        EnemyAttack attack =
            enemySpawner.GetCurrentEnemyAttack();

        if (attack == null)
        {
            Debug.LogError(
                $"RoundManager: Không tìm thấy EnemyAttack " +
                $"sau khi Spawn Round {round}. " +
                "Kiểm tra Enemy Prefab và EnemySpawner!"
            );

            return false;
        }

        Debug.Log(
            $"RoundManager: Spawn Enemy cho Round {round}"
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
            Debug.LogError(
                "RoundManager: Chưa gán EnemyIntentManager!"
            );
            return;
        }

        // Chỉ tạo Intent khi trận đấu còn diễn ra.
        if (battleFinished || isTransitioning)
            return;

        bool prepared =
            enemyIntentManager.PrepareIntent();

        if (prepared)
        {
            Debug.Log(
                "RoundManager: Enemy đã chuẩn bị Intent."
            );
        }
        else
        {
            Debug.LogWarning(
                "RoundManager: Không thể chuẩn bị Enemy Intent!"
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
        if (battleFinished ||
            currentEnemyRewardClaimed ||
            isChangingRound)
        {
            return;
        }

        currentEnemyRewardClaimed = true;

        Debug.Log(
            "RoundManager: Enemy đã bị đánh bại!"
        );

        // Xóa Intent của Enemy vừa chết.
        ClearEnemyIntent();

        // Nhận thưởng Round hiện tại.
        currentCoin += coinPerRound;
        currentExp += expPerRound;

        UpdateRewardUI();

        Debug.Log(
            $"Reward: +{coinPerRound} Coin | " +
            $"+{expPerRound} EXP"
        );

        // Nếu đã hoàn thành Round cuối.
        if (currentRound >= maxRound)
        {
            WinBattle();
            return;
        }

        // Chuyển sang Round mới.
        StartCoroutine(NextRoundCoroutine());
    }

    // =========================================
    // NEXT ROUND
    // =========================================

    private IEnumerator NextRoundCoroutine()
    {
        isChangingRound = true;

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

        // Ẩn Card trong lúc chuyển Round.
        if (cardUI != null)
        {
            cardUI.SetActive(false);
        }

        // Ẩn Enemy HP UI.
        if (enemyUI != null)
        {
            enemyUI.SetActive(false);
        }

        // Đảm bảo Intent cũ không hiển thị.
        ClearEnemyIntent();

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
        {
            isChangingRound = false;
            yield break;
        }

        // Xác định Round tiếp theo.
        int nextRound = currentRound + 1;

        // Spawn Enemy mới.
        if (!SpawnRoundEnemy(nextRound))
        {
            Debug.LogError(
                "RoundManager: Chuyển Round thất bại!"
            );

            // Không mở PlayerTurn khi chưa có Enemy.
            isChangingRound = false;
            yield break;
        }

        currentRound = nextRound;

        UpdateRoundUI();

        Debug.Log(
            $"RoundManager: Round " +
            $"{currentRound}/{maxRound}"
        );

        // Hiện lại Enemy HP UI.
        if (enemyUI != null)
        {
            enemyUI.SetActive(true);
        }

        // Hiện lại Card.
        if (cardUI != null)
        {
            cardUI.SetActive(true);
        }

        // Reset trạng thái nhận thưởng.
        currentEnemyRewardClaimed = false;

        isChangingRound = false;

        // Bắt đầu Player Turn mới.
        if (battleStateMachine != null)
        {
            battleStateMachine.StartPlayerTurn();
        }
        else
        {
            Debug.LogError(
                "RoundManager: Chưa gán BattleStateMachine!"
            );
            yield break;
        }

        // Quan trọng:
        // Enemy mới chọn trước Attack hoặc Skill.
        PrepareEnemyIntent();

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

        Debug.Log(
            "RoundManager: PLAYER DEFEATED!"
        );

        // Ẩn Intent khi Player thua.
        ClearEnemyIntent();

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

        // Ẩn Intent khi Victory.
        ClearEnemyIntent();

        Debug.Log(
            "RoundManager: VICTORY!"
        );

        Debug.Log(
            $"TỔNG PHẦN THƯỞNG: " +
            $"{currentCoin} Coin / {currentExp} EXP"
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
        // Khóa Gameplay UI.
        SetGameplayUI(false);

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, transitionDelay)
        );

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError(
                "RoundManager: Chưa nhập Scene tiếp theo!"
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
    // UPDATE ROUND UI
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
    // UPDATE REWARD UI
    // =========================================

    private void UpdateRewardUI()
    {
        if (coinText != null)
        {
            coinText.text = "Coin: " + currentCoin;
        }

        if (expText != null)
        {
            expText.text = "EXP: " + currentExp;
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
