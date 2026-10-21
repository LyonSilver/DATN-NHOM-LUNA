using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [Header("Player Currency")]
    [SerializeField] private int money = 1000;
    [SerializeField] private int energy = 10000;

    // Thong bao khi Coin hoac Gem thay doi.
    public event Action OnCurrencyChanged;

    private int lastMoney;
    private int lastEnergy;

    private void Awake()
    {
        lastMoney = money;
        lastEnergy = energy;
    }

    private void Update()
    {
        // Phat hien khi ban sua Coin/Gem truc tiep
        // trong Inspector luc dang Play Mode.
        if (money != lastMoney || energy != lastEnergy)
        {
            NotifyCurrencyChanged();
        }
    }
    private void NotifyCurrencyChanged()
    {
        lastMoney = money;
        lastEnergy = energy;

        if (OnCurrencyChanged != null)
        {
            OnCurrencyChanged.Invoke();
        }
    }

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
        if (amount <= 0 || money < amount)
        {
            return;
        }

        money -= amount;
        NotifyCurrencyChanged();
    }

    public void SpendEnergy(int amount)
    {
        if (amount <= 0 || energy < amount)
        {
            return;
        }

        energy -= amount;
        NotifyCurrencyChanged();
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        money += amount;
        NotifyCurrencyChanged();
    }

    public void AddEnergy(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        energy += amount;
        NotifyCurrencyChanged();
    }
}