
using System.Collections;
using UnityEngine;

public class OneCardTurnController : MonoBehaviour
{
    [Header("Battle Systems")]
    [SerializeField]
    private BattleStateMachine battleStateMachine;

    [SerializeField] private EndButton endButton;

    [Header("Card System")]
    [SerializeField] private CardManager cardManager;

    private bool cardUsedThisTurn;
    private bool enteredEnemyTurn;
    private Coroutine endTurnCoroutine;

    // =====================================
    // CHECK CARD LIMIT
    // =====================================

    public bool CanUseCard()
    {
        return !cardUsedThisTurn &&
               battleStateMachine != null &&
               battleStateMachine.IsPlayerTurn() &&
               !battleStateMachine.IsBattleFinished();
    }

    // =====================================
    // CARD USED
    // =====================================

    public void NotifyCardStarted()
    {
        if (!CanUseCard())
            return;

        cardUsedThisTurn = true;
        enteredEnemyTurn = false;

        endTurnCoroutine = StartCoroutine(
            WaitForCardAndEndTurn()
        );

        Debug.Log(
            "OneCardTurn: Đã dùng 1 Card, khóa lượt!"
        );
    }

    // =====================================
    // AUTO END TURN
    // =====================================

    private IEnumerator WaitForCardAndEndTurn()
    {
        yield return null;

        // Chờ CardManager xử lý xong:
        // Animation, Damage, VFX và Hand.
        while (cardManager != null &&
               cardManager.IsUsingCard)
        {
            if (battleStateMachine == null ||
                battleStateMachine.IsBattleFinished())
            {
                endTurnCoroutine = null;
                yield break;
            }

            yield return null;
        }

        // Nếu game đang Pause thì chưa chuyển lượt.
        while (Time.timeScale <= 0f)
        {
            if (battleStateMachine == null ||
                battleStateMachine.IsBattleFinished())
            {
                endTurnCoroutine = null;
                yield break;
            }

            yield return null;
        }

        // Nếu Card giết Enemy và hệ thống đã
        // chuyển sang NextRound/Victory thì dừng.
        if (battleStateMachine == null ||
            !battleStateMachine.IsPlayerTurn() ||
            battleStateMachine.IsBattleFinished())
        {
            endTurnCoroutine = null;
            yield break;
        }

        if (endButton == null)
        {
            Debug.LogError(
                "OneCardTurn: Chưa gán EndButton!"
            );
            endTurnCoroutine = null;
            yield break;
        }

        // Gọi logic Enemy Turn hiện có.
        endButton.EndTurn();

        if (battleStateMachine.CurrentState ==
            BattleStateMachine.BattleState.EnemyTurn)
        {
            enteredEnemyTurn = true;
        }

        endTurnCoroutine = null;
    }

    // =====================================
    // RESET NEXT TURN
    // =====================================

    private void Update()
    {
        if (!cardUsedThisTurn ||
            battleStateMachine == null)
            return;

        var state = battleStateMachine.CurrentState;

        // Enemy đã bị hạ, game chuyển Round mới.
        if (state == BattleStateMachine.BattleState.NextRound)
        {
            ResetCardLimit();
            return;
        }

        // Đánh dấu đã vào Enemy Turn.
        if (state == BattleStateMachine.BattleState.EnemyTurn)
        {
            enteredEnemyTurn = true;
        }

        // Chỉ mở khóa khi Enemy Turn đã kết thúc.
        if (enteredEnemyTurn &&
            battleStateMachine.IsPlayerTurn())
        {
            ResetCardLimit();
        }
    }

    private void ResetCardLimit()
    {
        if (endTurnCoroutine != null)
        {
            StopCoroutine(endTurnCoroutine);
            endTurnCoroutine = null;
        }

        cardUsedThisTurn = false;
        enteredEnemyTurn = false;

        Debug.Log(
            "OneCardTurn: Player được dùng Card mới!"
        );
    }

    private void OnDisable()
    {
        if (endTurnCoroutine != null)
        {
            StopCoroutine(endTurnCoroutine);
            endTurnCoroutine = null;
        }
    }
}