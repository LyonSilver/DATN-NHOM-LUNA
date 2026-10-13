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
    [SerializeField] private BattleState currentState;

    public BattleState CurrentState => currentState;

    private void Start()
    {
        ChangeState(BattleState.PlayerTurn);
    }

    public void ChangeState(BattleState newState)
    {
        if (currentState == BattleState.Victory ||
            currentState == BattleState.Defeat)
        {
            return;
        }

        currentState = newState;

        Debug.Log("Battle State: " + currentState);
    }

    public bool IsPlayerTurn()
    {
        return currentState == BattleState.PlayerTurn;
    }

    public bool IsBattleFinished()
    {
        return currentState == BattleState.Victory ||
               currentState == BattleState.Defeat;
    }
}