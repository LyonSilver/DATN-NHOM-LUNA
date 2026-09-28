using System;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;

    [Header("End Button")]
    [SerializeField] private EndButton endButton;

    public event Action<int> OnDamageCalculated;


    // Có Timing
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


    // Không có Timing
    public void AttackWithoutTiming()
    {
        Debug.Log(
            "Enemy Attack | Không có Timing | Damage: " +
            damage
        );

        DealDamage(damage);
    }


    private void DealDamage(int finalDamage)
    {
        if (endButton != null)
        {
            endButton.TakeDamage(finalDamage);
        }
        else
        {
            Debug.LogWarning(
                "EnemyAttack: Chưa gán EndButton."
            );
        }

        OnDamageCalculated?.Invoke(finalDamage);
    }


    public int GetDamage()
    {
        return damage;
    }
}