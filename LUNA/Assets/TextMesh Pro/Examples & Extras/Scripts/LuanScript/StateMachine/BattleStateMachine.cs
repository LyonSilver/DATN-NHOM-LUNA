using UnityEngine;

public class BattleStateMachine : MonoBehaviour
{
    public enum BattleState
    {
        PlayerTurn,
        EnemyTurn,
        CheckingBattle,
        NextRound,
        Victory,
        Defeat
    }

    [Header("Current State")]
    [SerializeField] private BattleState currentState = BattleState.PlayerTurn;

    public BattleState CurrentState => currentState;

    private void Start()
    {
        ChangeState(BattleState.PlayerTurn);
    }

    public void ChangeState(BattleState newState)
    {
        // Không cho thay đổi trạng thái sau khi trận đấu kết thúc.
        if (IsBattleFinished())
            return;

        currentState = newState;

        Debug.Log("Battle State: " + currentState);
    }

    public bool IsPlayerTurn()
    {
        return currentState == BattleState.PlayerTurn;
    }

    public bool IsEnemyTurn()
    {
        return currentState == BattleState.EnemyTurn;
    }

    public bool IsBattleFinished()
    {
        return currentState == BattleState.Victory ||
               currentState == BattleState.Defeat;
    }

    public void StartPlayerTurn()
    {
        if (IsBattleFinished())
            return;

        ChangeState(BattleState.PlayerTurn);
    }

    public void StartEnemyTurn()
    {
        if (IsBattleFinished())
            return;

        ChangeState(BattleState.EnemyTurn);
    }
}