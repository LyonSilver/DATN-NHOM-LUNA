
using UnityEngine;
using System.Collections;

public class EndButton : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private EnemyIntentManager enemyIntentManager;

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
    private bool isResolvingTurn;
    private bool isWaitingForTiming;

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
        if (battleStateMachine == null ||
            enemyIntentManager == null)
        {
            Debug.LogError(
                "EndButton: Thiếu FSM hoặc EnemyIntentManager!"
            );
            return;
        }

        if (isResolvingTurn ||
            !battleStateMachine.IsPlayerTurn() ||
            battleStateMachine.IsBattleFinished())
        {
            return;
        }

        // Lấy đúng hành động đã báo trước.
        bool isAttack;

        if (!enemyIntentManager.TryConsumeIntent(
                out enemyAttack,
                out isAttack))
        {
            Debug.LogWarning(
                "EndButton: Chưa có Intent hợp lệ!"
            );
            return;
        }

        isResolvingTurn = true;

        // Xử lý DoT khi kết thúc Player Turn.
        if (playerStatusController != null)
        {
            playerStatusController.ProcessTurnEndEffects();

            if (playerStatusController.IsPlayerDefeated ||
                battleStateMachine.IsBattleFinished())
            {
                isResolvingTurn = false;
                return;
            }
        }

        battleStateMachine.ChangeState(
            BattleStateMachine.BattleState.EnemyTurn
        );

        Debug.Log("========== ENEMY TURN ==========");

        BeginEnemyAction(isAttack);
    }

    // =========================================
    // ENEMY ACTION
    // =========================================

    private void BeginEnemyAction(bool isAttack)
    {
        if (!isResolvingTurn ||
            battleStateMachine == null ||
            battleStateMachine.IsBattleFinished() ||
            battleStateMachine.CurrentState !=
                BattleStateMachine.BattleState.EnemyTurn)
        {
            return;
        }

        if (enemyAttack == null ||
            enemySpawner == null ||
            enemyAttack != enemySpawner.GetCurrentEnemyAttack())
        {
            Debug.LogWarning(
                "EndButton: Enemy hiện tại không hợp lệ!"
            );

            CompletePlayerTurnTransition();
            return;
        }

        // =====================================
        // SKILL
        // =====================================

        if (!isAttack)
        {
            Debug.Log("Enemy thực hiện SKILL đã báo!");

            StartCoroutine(ResolveSkill());
            return;
        }

        // =====================================
        // NORMAL ATTACK
        // =====================================

        Debug.Log(
            "Enemy thực hiện NORMAL ATTACK đã báo!"
        );

        // Timing Chance chỉ dành cho Normal Attack.
        float randomValue = Random.Range(0f, 100f);

        if (randomValue < timingChance &&
            timingManager != null)
        {
            isWaitingForTiming = true;
            timingManager.StartTiming();
        }
        else
        {
            StartCoroutine(
                ResolveAttack(false, 1f)
            );
        }
    }

    // =========================================
    // SKILL
    // =========================================

    private IEnumerator ResolveSkill()
    {
        EnemyAttack actingEnemy = enemyAttack;

        if (actingEnemy != null &&
            enemySpawner != null &&
            actingEnemy == enemySpawner.GetCurrentEnemyAttack())
        {
            // Phát Animation và thực hiện Skill.
            actingEnemy.UseSkill();
        }

        yield return new WaitForSeconds(
            Mathf.Max(0f, skillAnimationDuration)
        );

        FinishTurn();
    }

    // =========================================
    // NORMAL ATTACK
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
            $"Timing Result: {result} | " +
            $"Multiplier: {multiplier}"
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
    // FINISH ENEMY TURN
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
            battleStateMachine.IsBattleFinished() ||
            battleStateMachine.CurrentState !=
                BattleStateMachine.BattleState.EnemyTurn)
        {
            isResolvingTurn = false;
            return;
        }

        // Stun / Freeze làm mất lượt Player.
        if (playerStatusController != null &&
            playerStatusController.ConsumeSkippedTurn())
        {
            StartCoroutine(SkipPlayerTurn());
            return;
        }

        CompletePlayerTurnTransition();
    }

    // =========================================
    // START NEXT PLAYER TURN
    // =========================================

    private void CompletePlayerTurnTransition()
    {
        isResolvingTurn = false;
        isWaitingForTiming = false;

        if (battleStateMachine == null ||
            battleStateMachine.IsBattleFinished() ||
            battleStateMachine.CurrentState !=
                BattleStateMachine.BattleState.EnemyTurn)
        {
            return;
        }

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

        // Chuẩn bị và hiện Intent mới.
        if (enemyIntentManager != null)
        {
            enemyIntentManager.PrepareIntent();
        }
    }

    // =========================================
    // SKIP PLAYER TURN
    // =========================================

    private IEnumerator SkipPlayerTurn()
    {
        Debug.Log(
            "Player mất lượt vì Stun hoặc Freeze!"
        );

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

        // DoT vẫn được tính khi mất lượt.
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

        // Player không có lượt để xem Intent,
        // nhưng Enemy vẫn phải chọn hành động mới.
        if (enemyIntentManager == null ||
            !enemyIntentManager.PrepareIntent())
        {
            Debug.LogError(
                "EndButton: Không thể chuẩn bị Enemy Intent!"
            );

            CompletePlayerTurnTransition();
            yield break;
        }

        bool isAttack;

        if (!enemyIntentManager.TryConsumeIntent(
                out enemyAttack,
                out isAttack))
        {
            CompletePlayerTurnTransition();
            yield break;
        }

        Debug.Log(
            "========== ENEMY TURN (PLAYER SKIPPED) =========="
        );

        BeginEnemyAction(isAttack);
    }
}
