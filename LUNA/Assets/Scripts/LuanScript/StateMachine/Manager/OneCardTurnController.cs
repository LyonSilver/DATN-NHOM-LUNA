
using System.Collections;
using UnityEngine;

public class OneCardTurnController : MonoBehaviour
{
    [Header("Battle Systems")]
    [SerializeField] private BattleStateMachine battleStateMachine;
    [SerializeField] private EndButton endButton;

    [Header("Card System")]
    [SerializeField] private CardManager cardManager;

    [Header("NPC Ultimate")]
    [SerializeField]
    private NalaUltimateController nalaUltimateController;

    private bool actionUsedThisTurn;
    private bool enteredEnemyTurn;

    private Coroutine endTurnCoroutine;

    public bool ActionUsedThisTurn => actionUsedThisTurn;

    // =====================================
    // CHECK ACTION LIMIT
    // =====================================

    public bool CanUseCard()
    {
        return !actionUsedThisTurn &&
               battleStateMachine != null &&
               battleStateMachine.IsPlayerTurn() &&
               !battleStateMachine.IsBattleFinished();
    }

    public bool CanUseUltimate()
    {
        return CanUseCard() &&
               cardManager != null &&
               !cardManager.IsUsingCard;
    }

    // =====================================
    // CARD USED
    // =====================================

    public void NotifyCardStarted()
    {
        if (!CanUseCard())
            return;

        BeginPlayerAction();

        Debug.Log(
            "[TURN] Da su dung Card, khoa luot!"
        );
    }

    // =====================================
    // ULTIMATE USED
    // =====================================

    public void NotifyUltimateStarted()
    {
        if (!CanUseUltimate())
            return;

        BeginPlayerAction();

        Debug.Log(
            "[TURN] Da su dung Ultimate, khoa luot!"
        );
    }

    // =====================================
    // BEGIN PLAYER ACTION
    // =====================================

    private void BeginPlayerAction()
    {
        actionUsedThisTurn = true;
        enteredEnemyTurn = false;

        if (endTurnCoroutine != null)
            StopCoroutine(endTurnCoroutine);

        endTurnCoroutine = StartCoroutine(
            WaitForActionAndEndTurn()
        );
    }

    // =====================================
    // AUTO END TURN
    // =====================================

    private IEnumerator WaitForActionAndEndTurn()
    {
        yield return null;

        // Cho Card hoac Ultimate hoan thanh.
        while (IsPlayerActionRunning())
        {
            if (battleStateMachine == null ||
                battleStateMachine.IsBattleFinished() ||
                !battleStateMachine.IsPlayerTurn())
            {
                endTurnCoroutine = null;
                yield break;
            }

            yield return null;
        }

        // Neu game dang Pause thi cho tiep.
        while (Time.timeScale <= 0f)
        {
            if (battleStateMachine == null ||
                battleStateMachine.IsBattleFinished() ||
                !battleStateMachine.IsPlayerTurn())
            {
                endTurnCoroutine = null;
                yield break;
            }

            yield return null;
        }

        // Neu Enemy chet va chuyen Round thi dung.
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
                "[TURN] Chua gan EndButton!"
            );

            endTurnCoroutine = null;
            yield break;
        }

        Debug.Log(
            "[TURN] Player hoan thanh hanh dong!"
        );

        // Dung chung logic chuyen luot.
        endButton.EndTurn();

        if (battleStateMachine.IsEnemyTurn())
        {
            enteredEnemyTurn = true;

            Debug.Log(
                "[TURN] Chuyen sang Enemy Turn!"
            );
        }
        else
        {
            Debug.LogWarning(
                "[TURN] EndTurn chua chuyen sang Enemy Turn!"
            );
        }

        endTurnCoroutine = null;
    }

    // =====================================
    // CHECK RUNNING ACTION
    // =====================================

    private bool IsPlayerActionRunning()
    {
        bool isUsingCard =
            cardManager != null &&
            cardManager.IsUsingCard;

        bool isUsingUltimate =
            nalaUltimateController != null &&
            nalaUltimateController.IsUsingUltimate;

        return isUsingCard || isUsingUltimate;
    }

    // =====================================
    // RESET NEXT TURN
    // =====================================

    private void Update()
    {
        if (!actionUsedThisTurn ||
            battleStateMachine == null)
            return;

        var state = battleStateMachine.CurrentState;

        // Enemy chet va chuyen Round.
        if (state == BattleStateMachine.BattleState.NextRound)
        {
            ResetActionLimit();
            return;
        }

        // Danh dau da vao Enemy Turn.
        if (state == BattleStateMachine.BattleState.EnemyTurn)
        {
            enteredEnemyTurn = true;
        }

        // Enemy hoan thanh hanh dong.
        if (enteredEnemyTurn &&
            battleStateMachine.IsPlayerTurn())
        {
            ResetActionLimit();
        }
    }

    // =====================================
    // RESET ACTION
    // =====================================

    private void ResetActionLimit()
    {
        if (endTurnCoroutine != null)
        {
            StopCoroutine(endTurnCoroutine);
            endTurnCoroutine = null;
        }

        actionUsedThisTurn = false;
        enteredEnemyTurn = false;

        Debug.Log(
            "[TURN] Player duoc su dung hanh dong moi!"
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