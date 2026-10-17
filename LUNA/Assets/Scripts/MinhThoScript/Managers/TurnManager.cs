
using System;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("Energy Settings")]
    [Min(1)]
    public int maxEnergy = 3;

    [Header("Energy Recovery")]
    [Range(0f, 100f)]
    [SerializeField] private float recoveryPercent = 40f;

    [Header("Runtime State")]
    public int currentEnergy;

    public int MaxEnergy => maxEnergy;
    public int CurrentEnergy => currentEnergy;

    public event Action OnEnergyChanged;

    // =====================================
    // AWAKE - START BATTLE
    // =====================================

    private void Awake()
    {
        maxEnergy = Mathf.Max(1, maxEnergy);

        // Bắt đầu trận với Mana đầy.
        currentEnergy = maxEnergy;
    }

    private void Start()
    {
        OnEnergyChanged?.Invoke();

        Debug.Log(
            $"TurnManager: Bắt đầu trận | " +
            $"Mana: {currentEnergy}/{maxEnergy}"
        );
    }

    // =====================================
    // START NEW PLAYER TURN
    // =====================================

    public void StartTurn()
    {
        // Hồi 40% Max Mana thay vì hồi đầy.
        int recoverAmount = Mathf.RoundToInt(
            maxEnergy * recoveryPercent / 100f
        );

        int oldEnergy = currentEnergy;

        currentEnergy = Mathf.Min(
            maxEnergy,
            currentEnergy + recoverAmount
        );

        OnEnergyChanged?.Invoke();

        Debug.Log(
            $"TurnManager: Bắt đầu Player Turn | " +
            $"Hồi {currentEnergy - oldEnergy} Mana | " +
            $"Mana: {currentEnergy}/{maxEnergy}"
        );
    }

    // =====================================
    // CHECK CARD COST
    // =====================================

    public bool CanAfford(CardData card)
    {
        if (card == null)
            return false;

        if (card.cost < 0)
            return false;

        return currentEnergy >= card.cost;
    }

    // =====================================
    // SPEND ENERGY
    // =====================================

    public bool SpendEnergy(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning(
                "TurnManager: Mana Cost không được âm!"
            );
            return false;
        }

        if (amount > currentEnergy)
        {
            Debug.LogWarning(
                $"TurnManager: Không đủ Mana! " +
                $"Cần {amount}, hiện có {currentEnergy}."
            );

            return false;
        }

        currentEnergy -= amount;

        OnEnergyChanged?.Invoke();

        Debug.Log(
            $"TurnManager: Dùng {amount} Mana | " +
            $"Còn lại: {currentEnergy}/{maxEnergy}"
        );

        return true;
    }

    // =====================================
    // RECOVER ENERGY
    // =====================================

    public void RecoverEnergy(int amount)
    {
        if (amount <= 0)
            return;

        int oldEnergy = currentEnergy;

        currentEnergy = Mathf.Min(
            maxEnergy,
            currentEnergy + amount
        );

        OnEnergyChanged?.Invoke();

        Debug.Log(
            $"TurnManager: Hồi {currentEnergy - oldEnergy} Mana | " +
            $"{currentEnergy}/{maxEnergy}"
        );
    }

    // =====================================
    // TEST FUNCTIONS
    // =====================================

    [ContextMenu("TEST - Spend 1 Energy")]
    private void TestSpend1()
    {
        SpendEnergy(1);
    }

    [ContextMenu("TEST - Spend 2 Energy")]
    private void TestSpend2()
    {
        SpendEnergy(2);
    }

    [ContextMenu("TEST - Spend All Energy")]
    private void TestSpendAll()
    {
        SpendEnergy(currentEnergy);
    }

    [ContextMenu("TEST - Start New Turn (Recover Mana)")]
    private void TestStartTurn()
    {
        StartTurn();
    }

    [ContextMenu("TEST - Reset Full Mana")]
    private void TestResetFullMana()
    {
        currentEnergy = maxEnergy;
        OnEnergyChanged?.Invoke();
    }
}