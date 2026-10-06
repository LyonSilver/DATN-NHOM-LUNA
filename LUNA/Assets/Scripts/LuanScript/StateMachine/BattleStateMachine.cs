using UnityEngine;

public class BattleStateMachine : MonoBehaviour
{
    public enum BattleState
    {
        Start,
        PlayerTurn,
        PlayerAction,
        EnemyTurn,
        EnemyAction,
        GameOver
    }

    public BattleState currentState;

    private void Start()
    {
        ChangeState(BattleState.Start);
    }

    private void Update()
    {
        switch (currentState)
        {
            case BattleState.Start:
                StartBattle();
                break;

            case BattleState.PlayerTurn:
                PlayerTurn();
                break;

            case BattleState.PlayerAction:
                PlayerAction();
                break;

            case BattleState.EnemyTurn:
                EnemyTurn();
                break;

            case BattleState.EnemyAction:
                EnemyAction();
                break;

            case BattleState.GameOver:
                GameOver();
                break;
        }
    }

    private void ChangeState(BattleState newState)
    {
        currentState = newState;

        Debug.Log("Current State: " + currentState);
    }

    private void StartBattle()
    {
        Debug.Log("Battle Start!");

        ChangeState(BattleState.PlayerTurn);
    }

    private void PlayerTurn()
    {
        Debug.Log("Player Turn");

        // Sau này:
        // Hiện UI Attack
        // Hiện UI Skill
        // Hiện UI Item
        // Chờ Player chọn

        ChangeState(BattleState.PlayerAction);
    }

    private void PlayerAction()
    {
        Debug.Log("Player Action");

        // Sau này thực hiện Attack / Skill...

        ChangeState(BattleState.EnemyTurn);
    }

    private void EnemyTurn()
    {
        Debug.Log("Enemy Turn");

        ChangeState(BattleState.EnemyAction);
    }

    private void EnemyAction()
    {
        Debug.Log("Enemy Action");

        // Enemy tấn công

        ChangeState(BattleState.PlayerTurn);
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
    }
}