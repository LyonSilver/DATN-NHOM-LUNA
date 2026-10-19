
using System.Collections;
using UnityEngine;

public class EndButton : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private EnemyIntentManager enemyIntentManager;

    [Header("Card")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private CardManager cardManager;

    [Header("Player Card Cinematic")]
    [SerializeField] private PlayerCardCinematic playerCardCinematic;

    [Header("Turn")]
    [SerializeField] private TurnManager turnManager;

    [Header("Battle FSM")]
    [SerializeField] private BattleStateMachine battleStateMachine;

    [Header("Player Status")]
    [SerializeField] private PlayerStatusController playerStatusController;

    [Header("Enemy Animation")]
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
    private bool isSkippingTurn = false;

    // =========================================
    // END TURN BUTTON
    // =========================================

    public void EndTurn()
    {
        // =====================================
        // 1. VALIDATE MANAGERS
        // =====================================

        if (battleStateMachine == null ||
            enemySpawner == null ||
            enemyIntentManager == null)
        {
            Debug.LogError(
                "EndButton: Thiếu BattleStateMachine, " +
                "EnemySpawner hoặc EnemyIntentManager!"
            );

            return;
        }

        // =====================================
        // 2. CHECK TURN STATE
        // =====================================

        if (isResolvingTurn ||
            isSkippingTurn ||
            !battleStateMachine.IsPlayerTurn() ||
            battleStateMachine.IsBattleFinished())
        {
            return;
        }

        // =====================================
        // 3. CHECK CARD MANAGER
        // =====================================

        // Player đang chọn và thực hiện Card.
        if (cardManager != null &&
            cardManager.IsUsingCard)
        {
            Debug.Log(
                "EndButton: Player đang sử dụng Card!"
            );

            return;
        }

        // =====================================
        // 4. CHECK PLAYER CINEMATIC
        // =====================================

        // Camera đang Zoom hoặc Player đang
        // chạy đến Enemy / quay về vị trí cũ.
        if (playerCardCinematic != null &&
            playerCardCinematic.IsPlaying)
        {
            Debug.Log(
                "EndButton: Card Cinematic chưa kết thúc!"
            );

            return;
        }

        // =====================================
        // 5. GET ENEMY INTENT
        // =====================================

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
        // 6. PROCESS STATUS EFFECTS
        // =====================================

        // Poison và Burn gây Damage
        // khi kết thúc Player Turn.
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
        // 7. CHANGE TO ENEMY TURN
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

        // Kiểm tra Enemy đang tồn tại.
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
                "Enemy thực hiện Normal Attack!"
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
                "Enemy thực hiện Skill!"
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

        // Lấy script di chuyển của Enemy.
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
        // 2. PERFORM ATTACK
        // =====================================

        // AttackWithoutTiming thực hiện
        // Damage cố định và phát Animation
        // theo EnemyAttackBase hiện tại.
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
                "Enemy quay về vị trí ban đầu..."
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

        // UseSkill sẽ phát Animation
        // và áp dụng Skill đã chọn
        // trong EnemyIntentManager.
        actingEnemy.UseSkill();

        Debug.Log(
            $"{actingEnemy.gameObject.name} đã dùng Skill!"
        );

        // =====================================
        // 3. WAIT FOR SKILL ANIMATION
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
        if (!isResolvingTurn || isSkippingTurn)
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
        // CHECK STUN / FREEZE
        // =====================================

        if (playerStatusController != null &&
            playerStatusController.ConsumeSkippedTurn())
        {
            StartCoroutine(SkipPlayerTurn());
            return;
        }

        // Player không mất lượt.
        CompletePlayerTurnTransition();
    }

    // =========================================
    // COMPLETE PLAYER TURN TRANSITION
    // =========================================

    private void CompletePlayerTurnTransition()
    {
        if (battleStateMachine == null ||
            battleStateMachine.IsBattleFinished() ||
            battleStateMachine.CurrentState !=
                BattleStateMachine.BattleState.EnemyTurn)
        {
            isResolvingTurn = false;
            isSkippingTurn = false;
            return;
        }

        isResolvingTurn = false;
        isSkippingTurn = false;

        // =====================================
        // 1. RESET CARD
        // =====================================

        if (deckManager != null)
        {
            deckManager.InitializeDeck();
        }

        // =====================================
        // 2. RECOVER MANA
        // =====================================

        // TurnManager.StartTurn hiện tại
        // chỉ hồi 40% Max Mana.
        if (turnManager != null)
        {
            turnManager.StartTurn();
        }

        // =====================================
        // 3. START PLAYER TURN
        // =====================================

        battleStateMachine.ChangeState(
            BattleStateMachine.BattleState.PlayerTurn
        );

        Debug.Log(
            "========== PLAYER TURN =========="
        );

        // =====================================
        // 4. PREPARE NEW ENEMY INTENT
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
        isSkippingTurn = true;

        Debug.Log(
            "Player mất lượt vì Stun hoặc Freeze!"
        );

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, skipTurnDelay)
        );

        // =====================================
        // CHECK BATTLE
        // =====================================

        if (battleStateMachine == null ||
            battleStateMachine.IsBattleFinished() ||
            battleStateMachine.CurrentState !=
                BattleStateMachine.BattleState.EnemyTurn)
        {
            isSkippingTurn = false;
            isResolvingTurn = false;
            yield break;
        }

        // =====================================
        // PROCESS DOT
        // =====================================

        // Poison / Burn vẫn gây Damage
        // dù Player đang bị mất lượt.
        if (playerStatusController != null)
        {
            playerStatusController.ProcessTurnEndEffects();

            if (playerStatusController.IsPlayerDefeated ||
                battleStateMachine.IsBattleFinished())
            {
                isSkippingTurn = false;
                isResolvingTurn = false;
                yield break;
            }
        }

        // =====================================
        // PREPARE ENEMY NEXT INTENT
        // =====================================

        if (enemyIntentManager == null ||
            !enemyIntentManager.PrepareIntent())
        {
            Debug.LogError(
                "EndButton: Không thể chuẩn bị Enemy Intent!"
            );

            isSkippingTurn = false;
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

            isSkippingTurn = false;
            CompletePlayerTurnTransition();
            yield break;
        }

        Debug.Log(
            "========== ENEMY TURN (PLAYER SKIPPED) =========="
        );

        // Cho phép hành động Enemy tiếp theo
        // hoàn tất và tiếp tục kiểm tra Stun.
        isSkippingTurn = false;

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
