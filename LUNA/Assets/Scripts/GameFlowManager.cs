using System;
using UnityEngine;

public enum GameState
{
    MainMenu,
    StageSelect,
    Battle,
    Event,
    MiniGame,
    Result
}

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }

    public event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;

        Debug.Log("Game State: " + CurrentState);

        OnGameStateChanged?.Invoke(CurrentState);
    }
}