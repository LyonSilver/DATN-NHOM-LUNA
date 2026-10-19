
using System;
using UnityEngine;

public abstract class EnemyAttackBase : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] protected int damage = 10;

    [Header("Animation")]
    [SerializeField] protected Animator animator;

    [SerializeField] protected string attackTrigger = "Attack";
    [SerializeField] protected string shieldTrigger = "Shield";

    [Header("Card Effect Manager")]
    [SerializeField] protected CardEffectManager cardEffectManager;

    public event Action<int> OnDamageCalculated;

    protected virtual void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }
    }

    // =========================================
    // SET MANAGER
    // =========================================

    public void SetCardEffectManager(CardEffectManager manager)
    {
        cardEffectManager = manager;
    }

    // =========================================
    // ATTACK ANIMATION
    // =========================================

    public virtual void PlayAttackAnimation()
    {
        if (animator != null &&
            !string.IsNullOrEmpty(attackTrigger))
        {
            animator.SetTrigger(attackTrigger);
        }
    }

    // =========================================
    // SHIELD ANIMATION
    // =========================================

    public virtual void PlayShieldAnimation()
    {
        if (animator != null &&
            !string.IsNullOrEmpty(shieldTrigger))
        {
            animator.SetTrigger(shieldTrigger);
        }
    }

    // =========================================
    // ATTACK WITHOUT TIMING
    // =========================================

    public virtual void AttackWithoutTiming()
    {
        Debug.Log(
            $"{gameObject.name} Attack | " +
            $"Không có Timing | Damage: {damage}"
        );

        DealDamage(damage);
    }

    // =========================================
    // ATTACK WITH TIMING
    // =========================================

    public virtual void ApplyTimingDamage(float multiplier)
    {
        int finalDamage = Mathf.Max(
            0,
            Mathf.RoundToInt(damage * multiplier)
        );

        Debug.Log(
            $"{gameObject.name} Attack | " +
            $"Base Damage: {damage} | " +
            $"Multiplier: {multiplier} | " +
            $"Final Damage: {finalDamage}"
        );

        DealDamage(finalDamage);
    }

    // =========================================
    // DEAL DAMAGE
    // =========================================

    protected virtual void DealDamage(int finalDamage)
    {
        if (cardEffectManager == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Chưa gán CardEffectManager!"
            );

            return;
        }

        cardEffectManager.TakeDamage(finalDamage);

        OnDamageCalculated?.Invoke(finalDamage);
    }

    // =========================================
    // GET DAMAGE
    // =========================================

    public int GetDamage()
    {
        return damage;
    }
}