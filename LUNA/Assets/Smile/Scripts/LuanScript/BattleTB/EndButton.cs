
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
    [SerializeField] private PlayerCardCinematic playerCardCinematic;

    [Header("Turn")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private BattleStateMachine battleStateMachine;
    [SerializeField] private PlayerStatusController playerStatusController;

    [Header("Enemy Animation Duration")]
    [SerializeField] private float attackAnimationDuration = 1f;
    [SerializeField] private float skillAnimationDuration = 1f;

    [Header("Enemy Dash")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private bool dashOnNormalAttack = true;
    [SerializeField] private bool dashOnSkill = true;

    [Header("Status")]
    [SerializeField] private float skipTurnDelay = 0.5f;

    private EnemyAttack enemyAttack;
    private bool isResolvingTurn;
    private bool isSkippingTurn;

    // =====================================
    // END TURN BUTTON
    // =====================================

    public void EndTurn()
    {
        if (!CanEndTurn())
            return;

        if (!TryGetEnemyIntent(out bool isAttack))
            return;

        isResolvingTurn = true;

        // Xử lý Poison / Burn của Player.
        if (!ProcessPlayerStatus())
        {
            isResolvingTurn = false;
            return;
        }

        battleStateMachine.ChangeState(
            BattleStateMachine.BattleState.EnemyTurn
        );

        BeginEnemyAction(isAttack);
    }

    // =====================================
    // VALIDATE PLAYER TURN
    // =====================================

    private bool CanEndTurn()
    {
        if (battleStateMachine == null ||
            enemySpawner == null ||
            enemyIntentManager == null)
        {
            Debug.LogError(
                "EndButton: Thiếu Battle Systems!"
            );
            return false;
        }

        if (isResolvingTurn || isSkippingTurn)
            return false;

        if (!battleStateMachine.IsPlayerTurn() ||
            battleStateMachine.IsBattleFinished())
            return false;

        if (cardManager != null && cardManager.IsUsingCard)
            return false;

        if (playerCardCinematic != null &&
            playerCardCinematic.IsPlaying)
            return false;

        return true;
    }

    // =====================================
    // GET ENEMY INTENT
    // =====================================

    private bool TryGetEnemyIntent(out bool isAttack)
    {
        isAttack = true;

        if (enemyIntentManager == null ||
            !enemyIntentManager.TryConsumeIntent(
                out enemyAttack,
                out isAttack))
        {
            Debug.LogWarning(
                "EndButton: Không lấy được Enemy Intent!"
            );
            return false;
        }

        return true;
    }

    // =====================================
    // PROCESS PLAYER STATUS
    // =====================================

    private bool ProcessPlayerStatus()
    {
        if (playerStatusController != null)
        {
            playerStatusController.ProcessTurnEndEffects();

            if (playerStatusController.IsPlayerDefeated)
                return false;
        }

        return battleStateMachine != null &&
               !battleStateMachine.IsBattleFinished();
    }

    // =====================================
    // BEGIN ENEMY ACTION
    // =====================================

    private void BeginEnemyAction(bool isAttack)
    {
        if (!IsEnemyTurnValid())
        {
            isResolvingTurn = false;
            return;
        }

        if (enemyAttack == null ||
            enemyAttack != enemySpawner.GetCurrentEnemyAttack())
        {
            Debug.LogWarning(
                "EndButton: Enemy hiện tại không hợp lệ!"
            );

            CompletePlayerTurnTransition();
            return;
        }

        // Dùng chung một Coroutine cho
        // Normal Attack và Skill.
        StartCoroutine(
            ResolveEnemyAction(enemyAttack, isAttack)
        );
    }

    // =====================================
    // NORMAL ATTACK / SKILL
    // =====================================

    private IEnumerator ResolveEnemyAction(
        EnemyAttack actingEnemy,
        bool isAttack)
    {
        if (!CanAct(actingEnemy))
        {
            FinishTurn();
            yield break;
        }

        EnemyDashMovement movement =
            actingEnemy.GetComponentInParent<EnemyDashMovement>();

        bool allowDash = isAttack
            ? dashOnNormalAttack
            : dashOnSkill;

        bool useDash =
            allowDash &&
            movement != null &&
            playerTarget != null;

        // 1. Dash tới Player.
        if (useDash)
            yield return movement.DashTo(playerTarget);

        if (!CanAct(actingEnemy))
        {
            FinishTurn();
            yield break;
        }

        // 2. Thực hiện hành động.
        if (isAttack)
        {
            // EnemyAttackBase tự phát Animation
            // và xử lý Damage.
            actingEnemy.AttackWithoutTiming();

            Debug.Log("Enemy: Normal Attack!");
        }
        else
        {
            // EnemySkillBase quản lý
            // Animation và hiệu ứng Skill.
            actingEnemy.UseSkill();

            Debug.Log("Enemy: Use Skill!");
        }

        // 3. Đợi Animation.
        float duration = isAttack
            ? attackAnimationDuration
            : skillAnimationDuration;

        yield return new WaitForSeconds(
            Mathf.Max(0f, duration)
        );

        // 4. Quay về vị trí ban đầu.
        if (useDash && movement != null &&
            CanAct(actingEnemy))
        {
            yield return movement.ReturnToStart();
        }

        // 5. Kết thúc Enemy Turn.
        FinishTurn();
    }

    // =====================================
    // FINISH ENEMY TURN
    // =====================================

    public void FinishTurn()
    {
        if (!isResolvingTurn || isSkippingTurn)
            return;

        if (!IsEnemyTurnValid())
        {
            isResolvingTurn = false;
            return;
        }

        // Player bị Stun / Freeze.
        if (playerStatusController != null &&
            playerStatusController.ConsumeSkippedTurn())
        {
            StartCoroutine(SkipPlayerTurn());
            return;
        }

        CompletePlayerTurnTransition();
    }

    // =====================================
    // START NEXT PLAYER TURN
    // =====================================

    private void CompletePlayerTurnTransition()
    {
        if (!IsEnemyTurnValid())
        {
            isResolvingTurn = false;
            isSkippingTurn = false;
            return;
        }

        isResolvingTurn = false;
        isSkippingTurn = false;

        // Reset Deck.
        if (deckManager != null)
            deckManager.InitializeDeck();

        // Hồi Mana theo TurnManager.
        if (turnManager != null)
            turnManager.StartTurn();

        // Chuyển sang Player Turn.
        battleStateMachine.ChangeState(
            BattleStateMachine.BattleState.PlayerTurn
        );

        // Chuẩn bị Intent cho lượt tiếp theo.
        if (enemyIntentManager != null &&
            !enemyIntentManager.PrepareIntent())
        {
            Debug.LogWarning(
                "EndButton: Không chuẩn bị được Enemy Intent!"
            );
        }

        Debug.Log("========== PLAYER TURN ==========");
    }

    // =====================================
    // SKIP PLAYER TURN (STUN / FREEZE)
    // =====================================

    private IEnumerator SkipPlayerTurn()
    {
        isSkippingTurn = true;

        Debug.Log(
            "Player bị Stun / Freeze: Mất lượt!"
        );

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, skipTurnDelay)
        );

        if (!IsEnemyTurnValid())
        {
            isSkippingTurn = false;
            isResolvingTurn = false;
            yield break;
        }

        // DOT vẫn gây Damage khi Player mất lượt.
        if (!ProcessPlayerStatus())
        {
            isSkippingTurn = false;
            isResolvingTurn = false;
            yield break;
        }

        // Chuẩn bị hành động tiếp theo của Enemy.
        if (enemyIntentManager == null ||
            !enemyIntentManager.PrepareIntent())
        {
            isSkippingTurn = false;
            CompletePlayerTurnTransition();
            yield break;
        }

        if (!TryGetEnemyIntent(out bool isAttack))
        {
            isSkippingTurn = false;
            CompletePlayerTurnTransition();
            yield break;
        }

        isSkippingTurn = false;

        // Enemy tiếp tục hành động.
        BeginEnemyAction(isAttack);
    }

    // =====================================
    // VALIDATE ENEMY TURN
    // =====================================

    private bool IsEnemyTurnValid()
    {
        return isResolvingTurn &&
               battleStateMachine != null &&
               !battleStateMachine.IsBattleFinished() &&
               battleStateMachine.CurrentState ==
                   BattleStateMachine.BattleState.EnemyTurn;
    }

    private bool CanAct(EnemyAttack actingEnemy)
    {
        return IsEnemyTurnValid() &&
               actingEnemy != null &&
               enemySpawner != null &&
               actingEnemy ==
                   enemySpawner.GetCurrentEnemyAttack();
    }
}