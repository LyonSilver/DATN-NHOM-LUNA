using System;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;

    [Header("Card Effect Manager")]
    [SerializeField] private CardEffectManager cardEffectManager;

    public event Action<int> OnDamageCalculated;


    // =========================================================
    // CÓ TIMING
    // =========================================================

    public void ApplyTimingDamage(float multiplier)
    {
        int finalDamage = Mathf.RoundToInt(
            damage * multiplier
        );

        Debug.Log(
            "Enemy Attack | Timing Damage: " +
            finalDamage +
            " | Base Damage: " +
            damage +
            " | Multiplier: " +
            multiplier
        );

        DealDamage(finalDamage);
    }


    // =========================================================
    // KHÔNG CÓ TIMING
    // =========================================================

    public void AttackWithoutTiming()
    {
        Debug.Log(
            "Enemy Attack | Không có Timing | Damage: " +
            damage
        );

        DealDamage(damage);
    }


    // =========================================================
    // DEAL DAMAGE
    // =========================================================

    private void DealDamage(int finalDamage)
    {
        if (cardEffectManager != null)
        {
            cardEffectManager.TakeDamage(finalDamage);
        }
        else
        {
            Debug.LogWarning(
                "EnemyAttack: Chưa gán CardEffectManager."
            );
        }

        OnDamageCalculated?.Invoke(finalDamage);
    }


    // =========================================================
    // GET DAMAGE
    // =========================================================

    public int GetDamage()
    {
        return damage;
    }
}