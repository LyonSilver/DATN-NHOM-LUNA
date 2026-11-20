
using UnityEngine;

public class RageManager : MonoBehaviour
{
    [Header("Rage Settings")]
    [SerializeField] private int maxRage = 100;
    [SerializeField] private int currentRage = 0;

    [Header("Card Rage")]
    [SerializeField] private int ragePerCard = 50;

    public int CurrentRage => currentRage;
    public int MaxRage => maxRage;

    public bool IsRageFull => currentRage >= maxRage;

    // =====================================
    // ADD RAGE
    // =====================================

    public void AddRage(int amount)
    {
        if (amount <= 0)
            return;

        currentRage = Mathf.Clamp(
            currentRage + amount,
            0,
            maxRage
        );
    }

    // =====================================
    // CARD USED
    // =====================================

    public void OnCardUsed()
    {
        AddRage(ragePerCard);

        Debug.Log("Rage: " + currentRage + "/" + maxRage);
    }

    // =====================================
    // USE RAGE
    // =====================================

    public bool UseRage()
    {
        if (!IsRageFull)
            return false;

        currentRage = 0;

        return true;
    }

    // =====================================
    // RESET RAGE
    // =====================================

    public void ResetRage()
    {
        currentRage = 0;
    }
}