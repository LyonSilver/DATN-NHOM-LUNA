using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public event System.Action OnCurrencyChanged;

    [Header("Player Currency")]
    [SerializeField] private int money = 1000;
    [SerializeField] private int energy = 100;

    public int GetMoney()
    {
        return money;
    }

    public int GetEnergy()
    {
        return energy;
    }

    public bool HasEnoughMoney(int amount)
    {
        return money >= amount;
    }

    public bool HasEnoughEnergy(int amount)
    {
        return energy >= amount;
    }

    public void SpendMoney(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (money >= amount)
        {
            money -= amount;
            OnCurrencyChanged?.Invoke();
        }
    }

    public void SpendEnergy(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (energy >= amount)
        {
            energy -= amount;
            OnCurrencyChanged?.Invoke();
        }
    }

    public void AddMoney(int amount)
    {
        if (amount > 0)
        {
            money += amount;
            OnCurrencyChanged?.Invoke();
        }
    }

    public void AddEnergy(int amount)
    {
        if (amount > 0)
        {
            energy += amount;
            OnCurrencyChanged?.Invoke();
        }
    }
}