using System;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Damage")]
    [SerializeField] private int damage = 10;

    [Header("Card Effect Manager")]
    [SerializeField] private CardEffectManager cardEffectManager;

    public event Action<int> OnDamageCalculated;

    // =========================================================
    // RANDOM ENEMY ACTION
    // =========================================================

    public bool RandomAction()
    {
        int randomAction = UnityEngine.Random.Range(0, 2);

        if (randomAction == 0)
        {
            // Enemy Attack
            return true;
        }
        else
        {
            // Enemy Shield
            UseShield();
            return false;
        }
    }

    // =========================================================
    // SHIELD
    // =========================================================

    public void UseShield()
    {
        if (cardEffectManager != null)
        {
            cardEffectManager.ApplyEnemyShield(10);
        }
        else
        {
            Debug.LogWarning(
                "EnemyAttack: Chưa gán CardEffectManager."
            );
        }
    }

    // =========================================================
    // ATTACK
    // =========================================================

    public void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    public void AttackWithoutTiming()
    {
        Debug.Log(
            "Enemy Attack | Không có Timing | Damage: " +
            damage
        );

        DealDamage(damage);
    }

    // =========================================================
    // TIMING DAMAGE
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