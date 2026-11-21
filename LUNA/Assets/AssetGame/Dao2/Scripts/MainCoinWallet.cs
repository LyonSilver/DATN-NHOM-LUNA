
using System;
using UnityEngine;

public class MainCoinWallet : MonoBehaviour
{
    public static MainCoinWallet Instance { get; private set; }

    private const string COIN_SAVE_KEY = "MAIN_GAME_COINS";

    public int Balance { get; private set; }

    public event Action<int> OnBalanceChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        LoadCoins();
    }

    private void LoadCoins()
    {
        Balance = PlayerPrefs.GetInt(COIN_SAVE_KEY, 0);

        OnBalanceChanged?.Invoke(Balance);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Coin amount must be greater than 0.");
            return;
        }

        Balance = (int)Math.Min(
            (long)Balance + amount,
            int.MaxValue
        );

        SaveCoins();

        OnBalanceChanged?.Invoke(Balance);

        Debug.Log($"Received {amount} Coins. Balance: {Balance}");
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0)
            return false;

        if (Balance < amount)
        {
            Debug.Log("Not enough Coins.");
            return false;
        }

        Balance -= amount;

        SaveCoins();

        OnBalanceChanged?.Invoke(Balance);

        Debug.Log($"Spent {amount} Coins. Balance: {Balance}");

        return true;
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(COIN_SAVE_KEY, Balance);
        PlayerPrefs.Save();
    }
}
