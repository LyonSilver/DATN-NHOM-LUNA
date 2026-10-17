
using UnityEngine;
using System.Collections;

public class EndButton : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private EnemyIntentManager enemyIntentManager;

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

    [Header("Enemy Dash Movement")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private bool dashOnNormalAttack = true;
    [SerializeField] private bool dashOnSkill = true;

    [Header("Status Settings")]
    [SerializeField] private float skipTurnDelay = 0.5f;

    private EnemyAttack enemyAttack;
    private bool isResolvingTurn = false;

    // =========================================
    // END TURN BUTTON
    // =========================================

    public void EndTurn()
    {
        if (battleStateMachine == null ||
            enemyIntentManager == null ||
            enemySpawner == null)
        {
            Debug.LogError(
                "EndButton: Thiếu FSM, EnemySpawner " +
                "hoặc EnemyIntentManager!"
            );
            return;
        }

        // Không cho kết thúc lượt nhiều lần.
        if (isResolvingTurn ||
            !battleStateMachine.IsPlayerTurn() ||
            battleStateMachine.IsBattleFinished())
        {
            return;
        }

        // Lấy hành động Enemy đã chuẩn bị.
        bool isAttack;

        if (!enemyIntentManager.TryConsumeIntent(
                out enemyAttack,
                out isAttack))
        {
            Debug.LogWarning(
                "EndButton: Chưa có Enemy Intent hợp lệ!"
            );
            return;
        }

        isResolvingTurn = true;

        Debug.Log(
            "========== PLAYER END TURN =========="
        );

        // =====================================
        // PROCESS STATUS EFFECTS
        // =====================================

        // Poison / Burn gây Damage khi kết thúc
        // Player Turn, kể cả khi Player không đánh.
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

        // =====================================
        // CHANGE TO ENEMY TURN
        // =====================================

        battleStateMachine.ChangeState(
            BattleStateMachine.BattleState.EnemyTurn
        );

        Debug.Log(
            "========== ENEMY TURN =========="
        );

        BeginEnemyAction(isAttack);
    }

    // =========================================
    // BEGIN ENEMY ACTION
    // =========================================

    private void BeginEnemyAction(bool isAttack)
    {
        if (!isResolvingTurn ||
            battleStateMachine == null ||
            battleStateMachine.IsBattleFinished() ||
            battleStateMachine.CurrentState !=
                BattleStateMachine.BattleState.EnemyTurn)
        {
            isResolvingTurn = false;
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
        // NORMAL ATTACK
        // =====================================

        if (isAttack)
        {
            Debug.Log(
                "Enemy thực hiện NORMAL ATTACK!"
            );

            StartCoroutine(
                ResolveAttack(enemyAttack)
            );
        }
        // =====================================
        // SKILL
        // =====================================
        else
        {
            Debug.Log(
                "Enemy thực hiện SKILL!"
            );

            StartCoroutine(
                ResolveSkill(enemyAttack)
            );
        }
    }

    // =========================================
    // NORMAL ATTACK
    // =========================================

    private IEnumerator ResolveAttack(
        EnemyAttack actingEnemy)
    {
        if (!CanAct(actingEnemy))
        {
            FinishTurn();
            yield break;
        }

        EnemyDashMovement movement =
            actingEnemy.GetComponentInParent<EnemyDashMovement>();

        bool useDash =
            dashOnNormalAttack &&
            movement != null &&
            playerTarget != null;

        // =====================================
        // 1. DASH TO PLAYER
        // =====================================

        if (useDash)
        {
            Debug.Log(
                "Enemy đang lướt tới Player..."
            );

            yield return movement.DashTo(playerTarget);
        }

        if (!CanAct(actingEnemy))
        {
            FinishTurn();
            yield break;
        }

        // =====================================
        // 2. ATTACK + DAMAGE
        // =====================================

        // Hàm AttackWithoutTiming trong
        // EnemyAttackBase hiện tại sẽ thực hiện
        // Normal Attack với Damage cố định.
        //
        // Nếu hàm này đã phát Animation Attack,
        // không gọi PlayAttackAnimation() lần nữa.
        actingEnemy.AttackWithoutTiming();

        Debug.Log(
            "Enemy đã thực hiện Normal Attack!"
        );

        // =====================================
        // 3. WAIT FOR ANIMATION
        // =====================================

        yield return new WaitForSeconds(
            Mathf.Max(0f, attackAnimationDuration)
        );

        // =====================================
        // 4. RETURN TO START
        // =====================================

        if (useDash &&
            movement != null &&
            CanAct(actingEnemy))
        {
            Debug.Log(
                "Enemy đang quay về vị trí ban đầu..."
            );

            yield return movement.ReturnToStart();
        }

        // =====================================
        // 5. FINISH ENEMY TURN
        // =====================================

        FinishTurn();
    }

    // =========================================
    // ENEMY SKILL
    // =========================================

    private IEnumerator ResolveSkill(
        EnemyAttack actingEnemy)
    {
        if (!CanAct(actingEnemy))
        {
            FinishTurn();
            yield break;
        }

        EnemyDashMovement movement =
            actingEnemy.GetComponentInParent<EnemyDashMovement>();

        bool useDash =
            dashOnSkill &&
            movement != null &&
            playerTarget != null;

        // =====================================
        // 1. DASH TO PLAYER
        // =====================================

        if (useDash)
        {
            Debug.Log(
                "Enemy đang lướt tới Player để dùng Skill..."
            );

            yield return movement.DashTo(playerTarget);
        }

        if (!CanAct(actingEnemy))
        {
            FinishTurn();
            yield break;
        }

        // =====================================
        // 2. EXECUTE SKILL
        // =====================================

        // UseSkill phát Animation và áp dụng
        // hiệu ứng Skill đã chọn trong Intent.
        actingEnemy.UseSkill();

        Debug.Log(
            $"{actingEnemy.gameObject.name} đã dùng Skill!"
        );

        // =====================================
        // 3. WAIT FOR ANIMATION
        // =====================================

        yield return new WaitForSeconds(
            Mathf.Max(0f, skillAnimationDuration)
        );

        // =====================================
        // 4. RETURN TO START
        // =====================================

        if (useDash &&
            movement != null &&
            CanAct(actingEnemy))
        {
            Debug.Log(
                "Enemy quay về sau khi dùng Skill..."
            );

            yield return movement.ReturnToStart();
        }

        // =====================================
        // 5. FINISH ENEMY TURN
        // =====================================

        FinishTurn();
    }

    // =========================================
    // FINISH ENEMY TURN
    // =========================================

    public void FinishTurn()
    {
        if (!isResolvingTurn)
            return;

        if (battleStateMachine == null ||
            battleStateMachine.IsBattleFinished() ||
            battleStateMachine.CurrentState !=
                BattleStateMachine.BattleState.EnemyTurn)
        {
            isResolvingTurn = false;
            return;
        }

        Debug.Log(
            "========== ENEMY ACTION FINISHED =========="
        );

        // =====================================
        // STUN / FREEZE
        // =====================================

        // Nếu Player bị Stun hoặc Freeze,
        // Enemy sẽ có thêm lượt hành động.
        if (playerStatusController != null &&
            playerStatusController.ConsumeSkippedTurn())
        {
            StartCoroutine(
                SkipPlayerTurn()
            );
            return;
        }

        // Player không bị mất lượt.
        CompletePlayerTurnTransition();
    }

    // =========================================
    // COMPLETE PLAYER TURN TRANSITION
    // =========================================

    private void CompletePlayerTurnTransition()
    {
        isResolvingTurn = false;

        if (battleStateMachine == null ||
            battleStateMachine.IsBattleFinished() ||
            battleStateMachine.CurrentState !=
                BattleStateMachine.BattleState.EnemyTurn)
        {
            return;
        }

        // =====================================
        // RESET CARD
        // =====================================

        if (deckManager != null)
        {
            deckManager.InitializeDeck();
        }

        // =====================================
        // RESET TURN / ENERGY
        // =====================================

        if (turnManager != null)
        {
            turnManager.StartTurn();
        }

        // =====================================
        // PLAYER TURN
        // =====================================

        battleStateMachine.ChangeState(
            BattleStateMachine.BattleState.PlayerTurn
        );

        Debug.Log(
            "========== PLAYER TURN =========="
        );

        // =====================================
        // PREPARE NEW ENEMY INTENT
        // =====================================

        if (enemyIntentManager != null)
        {
            bool success =
                enemyIntentManager.PrepareIntent();

            if (!success)
            {
                Debug.LogWarning(
                    "EndButton: Không thể chuẩn bị Intent mới!"
                );
            }
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

        // =====================================
        // PROCESS STATUS DAMAGE
        // =====================================

        // DoT vẫn gây Damage khi Player mất lượt.
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

        // =====================================
        // PREPARE NEXT ENEMY ACTION
        // =====================================

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
            Debug.LogWarning(
                "EndButton: Không thể lấy Enemy Intent!"
            );

            CompletePlayerTurnTransition();
            yield break;
        }

        Debug.Log(
            "========== ENEMY TURN (PLAYER SKIPPED) =========="
        );

        BeginEnemyAction(isAttack);
    }

    // =========================================
    // VALIDATE ENEMY ACTION
    // =========================================

    private bool CanAct(EnemyAttack actingEnemy)
    {
        return actingEnemy != null &&
            enemySpawner != null &&
            actingEnemy == enemySpawner.GetCurrentEnemyAttack() &&
            battleStateMachine != null &&
            !battleStateMachine.IsBattleFinished() &&
            battleStateMachine.CurrentState ==
                BattleStateMachine.BattleState.EnemyTurn;
    }
}