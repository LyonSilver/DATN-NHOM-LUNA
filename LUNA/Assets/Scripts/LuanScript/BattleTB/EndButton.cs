
using UnityEngine;
using System.Collections;

public class EndButton : MonoBehaviour
{
    [Header("Enemy Spawner")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Timing")]
    [SerializeField] private TimingManager timingManager;

    [Range(0f, 100f)]
    [SerializeField] private float timingChance = 100f;

    [Header("Card")]
    [SerializeField] private DeckManager deckManager;

    [Header("Turn")]
    [SerializeField] private TurnManager turnManager;

    [Header("Battle FSM")]
    [SerializeField] private BattleStateMachine battleStateMachine;

    [Header("Player Status")]
    [SerializeField] private PlayerStatusController playerStatusController;

    [Header("Animation Duration")]
    [SerializeField] private float attackAnimationDuration = 1f;
    [SerializeField] private float skillAnimationDuration = 1f;

    [Header("Status Settings")]
    [SerializeField] private float skipTurnDelay = 0.5f;

    private EnemyAttack enemyAttack;

    private bool isResolvingTurn = false;
    private bool isWaitingForTiming = false;

    // =========================================
    // START
    // =========================================

    private void Start()
    {
        if (timingManager != null)
        {
            timingManager.OnTimingResult += HandleTimingResult;
            timingManager.StopTiming();
        }
    }

    private void OnDestroy()
    {
        if (timingManager != null)
        {
            timingManager.OnTimingResult -= HandleTimingResult;
        }
    }

    // =========================================
    // END TURN
    // =========================================

    public void EndTurn()
    {
        if (battleStateMachine == null)
        {
            Debug.LogError(
                "EndButton: Chưa gán BattleStateMachine!"
            );
            return;
        }

        if (isResolvingTurn)
            return;

        // Chỉ được End Turn trong PlayerTurn.
        if (!battleStateMachine.IsPlayerTurn())
        {
            Debug.Log(
                "EndButton: Hiện không phải Player Turn."
            );
            return;
        }

        if (battleStateMachine.IsBattleFinished())
            return;

        // Kiểm tra Enemy hiện tại.
        enemyAttack = enemySpawner != null
            ? enemySpawner.GetCurrentEnemyAttack()
            : null;

        if (enemyAttack == null)
        {
            Debug.LogWarning(
                "EndButton: Không tìm thấy EnemyAttack!"
            );
            return;
        }

        // =========================================
        // PROCESS DOT
        // =========================================

        if (playerStatusController != null)
        {
            playerStatusController.ProcessTurnEndEffects();

            // Player có thể chết do Poison hoặc Burn.
            if (playerStatusController.IsPlayerDefeated ||
                battleStateMachine.IsBattleFinished())
            {
                return;
            }
        }

        isResolvingTurn = true;

        // Chuyển sang Enemy Turn.
        battleStateMachine.ChangeState(
            BattleStateMachine.BattleState.EnemyTurn
        );

        Debug.Log("========== ENEMY TURN ==========");

        BeginEnemyAction();
    }

    // =========================================
    // BEGIN ENEMY ACTION
    // =========================================

    private void BeginEnemyAction()
    {
        if (!isResolvingTurn ||
            battleStateMachine == null ||
            battleStateMachine.IsBattleFinished() ||
            battleStateMachine.CurrentState !=
            BattleStateMachine.BattleState.EnemyTurn)
        {
            return;
        }

        // Lấy lại Enemy hiện tại.
        enemyAttack = enemySpawner != null
            ? enemySpawner.GetCurrentEnemyAttack()
            : null;

        if (enemyAttack == null)
        {
            Debug.LogWarning(
                "EndButton: Không có Enemy để hành động!"
            );

            FinishTurn();
            return;
        }

        // true = Normal Attack
        // false = Skill
        bool isAttack = enemyAttack.RandomAction();

        if (!isAttack)
        {
            Debug.Log("Enemy chọn SKILL.");

            StartCoroutine(ResolveSkill());
            return;
        }

        Debug.Log("Enemy chọn NORMAL ATTACK.");

        // Timing Chance chỉ áp dụng Normal Attack.
        float randomValue = Random.Range(0f, 100f);

        if (randomValue < timingChance &&
            timingManager != null)
        {
            Debug.Log(
                "Enemy Attack: Timing xuất hiện!"
            );

            isWaitingForTiming = true;
            timingManager.StartTiming();
        }
        else
        {
            Debug.Log(
                "Enemy Attack: Không có Timing."
            );

            StartCoroutine(
                ResolveAttack(false, 1f)
            );
        }
    }

    // =========================================
    // RESOLVE SKILL
    // =========================================

    private IEnumerator ResolveSkill()
    {
        EnemyAttack actingEnemy = enemyAttack;

        if (actingEnemy != null)
        {
            // UseSkill tự phát Animation
            // và áp dụng hiệu ứng đã chọn.
            actingEnemy.UseSkill();
        }

        // Chờ Animation Skill.
        yield return new WaitForSeconds(
            Mathf.Max(0f, skillAnimationDuration)
        );

        FinishTurn();
    }

    // =========================================
    // RESOLVE ATTACK
    // =========================================

    private IEnumerator ResolveAttack(
        bool hasTiming,
        float multiplier)
    {
        EnemyAttack actingEnemy = enemyAttack;

        if (actingEnemy != null)
        {
            actingEnemy.PlayAttackAnimation();
        }

        // Chờ Animation Attack.
        yield return new WaitForSeconds(
            Mathf.Max(0f, attackAnimationDuration)
        );

        if (actingEnemy != null &&
            enemySpawner != null &&
            actingEnemy == enemySpawner.GetCurrentEnemyAttack() &&
            battleStateMachine != null &&
            battleStateMachine.CurrentState ==
            BattleStateMachine.BattleState.EnemyTurn)
        {
            if (hasTiming)
            {
                actingEnemy.ApplyTimingDamage(multiplier);
            }
            else
            {
                actingEnemy.AttackWithoutTiming();
            }
        }

        FinishTurn();
    }

    // =========================================
    // TIMING RESULT
    // =========================================

    private void HandleTimingResult(
        TimingManager.TimingResult result,
        float multiplier)
    {
        if (!isResolvingTurn ||
            !isWaitingForTiming ||
            battleStateMachine == null ||
            battleStateMachine.CurrentState !=
            BattleStateMachine.BattleState.EnemyTurn)
        {
            return;
        }

        isWaitingForTiming = false;

        Debug.Log(
            "Timing Result = " + result +
            " | Multiplier = " + multiplier
        );

        if (timingManager != null)
        {
            timingManager.StopTiming();
        }

        StartCoroutine(
            ResolveAttack(true, multiplier)
        );
    }

    // =========================================
    // FINISH TURN
    // =========================================

    public void FinishTurn()
    {
        if (!isResolvingTurn)
            return;

        if (timingManager != null)
        {
            timingManager.StopTiming();
        }

        isWaitingForTiming = false;

        if (battleStateMachine == null ||
            battleStateMachine.IsBattleFinished())
        {
            isResolvingTurn = false;
            return;
        }

        // Không ghi đè NextRound, Victory, Defeat.
        if (battleStateMachine.CurrentState !=
            BattleStateMachine.BattleState.EnemyTurn)
        {
            isResolvingTurn = false;
            return;
        }

        // =====================================
        // CHECK STUN / FREEZE
        // =====================================

        if (playerStatusController != null &&
            playerStatusController.ConsumeSkippedTurn())
        {
            Debug.Log(
                "Player bị Stun hoặc Freeze! Bỏ qua lượt."
            );

            StartCoroutine(SkipPlayerTurn());
            return;
        }

        // =====================================
        // NORMAL PLAYER TURN
        // =====================================

        isResolvingTurn = false;

        if (deckManager != null)
        {
            deckManager.InitializeDeck();
        }

        if (turnManager != null)
        {
            turnManager.StartTurn();
        }

        battleStateMachine.ChangeState(
            BattleStateMachine.BattleState.PlayerTurn
        );

        Debug.Log("========== PLAYER TURN ==========");
    }

    // =========================================
    // SKIP PLAYER TURN
    // =========================================

    private IEnumerator SkipPlayerTurn()
    {
        Debug.Log("Player mất lượt tiếp theo!");

        // Giữ FSM tại EnemyTurn để Player
        // không thể sử dụng Card trong lúc bị khóa.

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, skipTurnDelay)
        );

        if (battleStateMachine == null ||
            battleStateMachine.IsBattleFinished() ||
            battleStateMachine.CurrentState !=
            BattleStateMachine.BattleState.EnemyTurn)
        {
            isResolvingTurn = false;
            yield break;
        }

        // Lượt Player bị bỏ qua cũng được tính
        // là một lượt để xử lý Poison và Burn.
        if (playerStatusController != null)
        {
            playerStatusController.ProcessTurnEndEffects();

            if (playerStatusController.IsPlayerDefeated ||
                battleStateMachine.IsBattleFinished())
            {
                isResolvingTurn = false;
                yield break;
            }
        }

        Debug.Log(
            "========== ENEMY TURN (PLAYER SKIPPED) =========="
        );

        // Enemy tiếp tục hành động.
        BeginEnemyAction();
    }
}