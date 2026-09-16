using System;
using UnityEngine;

public enum TurnState
{
    PlayerTurn,
    EnemyTurn,
    Evaluating
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public TurnState CurrentTurnState { get; private set; }

    [Header("Energy")]
    public int MaxEnergy = 3;
    public int CurrentEnergy { get; private set; }

    public event Action OnPlayerTurnStart;
    public event Action OnEnemyTurnStart;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        CurrentTurnState = TurnState.PlayerTurn;

        // Khôi phục Energy
        CurrentEnergy = MaxEnergy;

        Debug.Log("Player Turn - Energy: " + CurrentEnergy);

        // Báo cho các hệ thống khác
        OnPlayerTurnStart?.Invoke();
    }

    public void EndPlayerTurn()
    {
        if (CurrentTurnState != TurnState.PlayerTurn)
            return;

        CurrentTurnState = TurnState.EnemyTurn;

        Debug.Log("Enemy Turn");

        OnEnemyTurnStart?.Invoke();
    }

    public void StartEvaluating()
    {
        CurrentTurnState = TurnState.Evaluating;

        Debug.Log("Evaluating...");
    }

    public void UseEnergy(int amount)
    {
        if (amount <= 0)
            return;

        if (CurrentEnergy < amount)
        {
            Debug.Log("Không đủ Energy!");
            return;
        }

        CurrentEnergy -= amount;

        Debug.Log("Current Energy: " + CurrentEnergy);
    }
}