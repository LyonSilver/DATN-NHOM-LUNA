
using UnityEngine;

[RequireComponent(typeof(CardEffectManager))]
public class PlayerStatusController : MonoBehaviour
{
    private CardEffectManager cardEffectManager;

    [Header("Poison")]
    [SerializeField] private int poisonDamage;
    [SerializeField] private int poisonTurns;

    [Header("Burn")]
    [SerializeField] private int burnDamage;
    [SerializeField] private int burnTurns;

    [Header("Stun")]
    [SerializeField] private int stunTurns;

    [Header("Freeze")]
    [SerializeField] private int freezeTurns;

    public bool IsPlayerDefeated =>
        cardEffectManager != null &&
        cardEffectManager.IsPlayerDefeated;

    private void Awake()
    {
        cardEffectManager = GetComponent<CardEffectManager>();
    }

    // =========================================
    // POISON
    // =========================================

    public void ApplyPoison(int damage, int duration)
    {
        if (damage <= 0 || duration <= 0 || IsPlayerDefeated)
            return;

        poisonDamage = Mathf.Max(poisonDamage, damage);
        poisonTurns = Mathf.Max(poisonTurns, duration);

        Debug.Log(
            $"Player bị Poison: {poisonDamage} Damage " +
            $"trong {poisonTurns} lượt."
        );
    }

    // =========================================
    // BURN
    // =========================================

    public void ApplyBurn(int damage, int duration)
    {
        if (damage <= 0 || duration <= 0 || IsPlayerDefeated)
            return;

        burnDamage = Mathf.Max(burnDamage, damage);
        burnTurns = Mathf.Max(burnTurns, duration);

        Debug.Log(
            $"Player bị Burn: {burnDamage} Damage " +
            $"trong {burnTurns} lượt."
        );
    }

    // =========================================
    // STUN
    // =========================================

    public void ApplyStun(int duration)
    {
        if (duration <= 0 || IsPlayerDefeated)
            return;

        stunTurns = Mathf.Max(stunTurns, duration);

        Debug.Log(
            $"Player bị Stun {stunTurns} lượt."
        );
    }

    // =========================================
    // FREEZE
    // =========================================

    public void ApplyFreeze(int duration)
    {
        if (duration <= 0 || IsPlayerDefeated)
            return;

        freezeTurns = Mathf.Max(freezeTurns, duration);

        Debug.Log(
            $"Player bị Freeze {freezeTurns} lượt."
        );
    }

    // =========================================
    // PROCESS DOT
    // =========================================

    public void ProcessTurnEndEffects()
    {
        if (cardEffectManager == null || IsPlayerDefeated)
            return;

        // Poison Damage
        if (poisonTurns > 0)
        {
            poisonTurns--;

            cardEffectManager.TakeStatusDamage(poisonDamage);

            Debug.Log(
                $"Poison Damage: {poisonDamage} | " +
                $"Còn {poisonTurns} lượt."
            );

            if (poisonTurns == 0)
                poisonDamage = 0;
        }

        if (IsPlayerDefeated)
            return;

        // Burn Damage
        if (burnTurns > 0)
        {
            burnTurns--;

            cardEffectManager.TakeStatusDamage(burnDamage);

            Debug.Log(
                $"Burn Damage: {burnDamage} | " +
                $"Còn {burnTurns} lượt."
            );

            if (burnTurns == 0)
                burnDamage = 0;
        }
    }

    // =========================================
    // CHECK SKIPPED TURN
    // =========================================

    public bool ConsumeSkippedTurn()
    {
        if (IsPlayerDefeated)
            return false;

        bool skipTurn = stunTurns > 0 || freezeTurns > 0;

        if (stunTurns > 0)
            stunTurns--;

        if (freezeTurns > 0)
            freezeTurns--;

        if (skipTurn)
        {
            Debug.Log(
                "Player mất lượt vì Stun hoặc Freeze!"
            );
        }

        return skipTurn;
    }
}
