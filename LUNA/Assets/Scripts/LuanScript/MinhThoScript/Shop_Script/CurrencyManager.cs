using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [Header("Player Currency (Coins = money)")]
    [SerializeField] private int money = 1000;
    [SerializeField] private int gem = 100;

    [Header("Giá trị mặc định khi Reset (để test)")]
    [SerializeField] private int defaultMoney = 1000;
    [SerializeField] private int defaultGem = 100;

    public int GetMoney()
    {
        return money;
    }

    public int GetGem()
    {
        return gem;
    }

    public bool HasEnoughMoney(int amount)
    {
        return money >= amount;
    }

    public bool HasEnoughGem(int amount)
    {
        return gem >= amount;
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
        }
    }

    public void SpendGem(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (gem >= amount)
        {
            gem -= amount;
        }
    }

    public void AddMoney(int amount)
    {
        if (amount > 0)
        {
            money += amount;
        }
    }

    public void AddGem(int amount)
    {
        if (amount > 0)
        {
            gem += amount;
        }
    }

    public void ResetToDefault()
    {
        money = defaultMoney;
        gem = defaultGem;
    }

    [ContextMenu("TEST - Reset về giá trị mặc định")]
    private void TestReset()
    {
        ResetToDefault();
        Debug.Log("CurrencyManager: Coins " + money + " | Gems " + gem);
    }
}