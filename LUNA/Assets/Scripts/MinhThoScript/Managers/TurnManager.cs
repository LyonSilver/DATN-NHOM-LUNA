using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("Cấu hình Energy (Mỗi lượt bắt đầu với 3 Energy)")]
    public int maxEnergy = 3;

    [Header("Runtime State (chỉ để xem, không tự sửa tay)")]
    public int currentEnergy;

    public event System.Action OnEnergyChanged;

    private void Start()
    {
        StartTurn();
    }

    public void StartTurn()
    {
        currentEnergy = maxEnergy;
        OnEnergyChanged?.Invoke();
    }

    public bool CanAfford(CardData card)
    {
        if (card == null) return false;
        return currentEnergy >= card.cost;
    }

    public bool SpendEnergy(int amount)
    {
        if (amount > currentEnergy)
        {
            Debug.Log("TurnManager: Không đủ Energy để trừ (" + amount + "), hiện có " + currentEnergy + ".");
            return false;
        }

        currentEnergy -= amount;
        OnEnergyChanged?.Invoke();
        return true;
    }

    [ContextMenu("TEST - Spend 1 Energy")]
    private void TestSpend1()
    {
        SpendEnergy(1);
        Debug.Log("TurnManager TEST: Còn lại " + currentEnergy + "/" + maxEnergy + " Energy.");
    }

    [ContextMenu("TEST - Spend 2 Energy")]
    private void TestSpend2()
    {
        SpendEnergy(2);
        Debug.Log("TurnManager TEST: Còn lại " + currentEnergy + "/" + maxEnergy + " Energy.");
    }

    [ContextMenu("TEST - Spend All Energy")]
    private void TestSpendAll()
    {
        SpendEnergy(currentEnergy);
        Debug.Log("TurnManager TEST: Còn lại " + currentEnergy + "/" + maxEnergy + " Energy.");
    }

    [ContextMenu("TEST - Start New Turn (Reset Energy)")]
    private void TestStartTurn()
    {
        StartTurn();
        Debug.Log("TurnManager TEST: Đã reset Energy về " + currentEnergy + "/" + maxEnergy + ".");
    }
}