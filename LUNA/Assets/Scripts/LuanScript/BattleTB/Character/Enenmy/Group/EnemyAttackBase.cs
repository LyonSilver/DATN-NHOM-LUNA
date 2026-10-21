
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

    // =====================================
    // AWAKE
    // =====================================

    protected virtual void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (animator == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Không tìm thấy Animator!"
            );
        }
    }

    // =====================================
    // SET MANAGER
    // =====================================

    public void SetCardEffectManager(CardEffectManager manager)
    {
        cardEffectManager = manager;
    }

    // =====================================
    // ATTACK ANIMATION
    // =====================================

    public virtual void PlayAttackAnimation()
    {
        if (animator == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Chưa gán Animator!"
            );
            return;
        }

        if (string.IsNullOrEmpty(attackTrigger))
            return;

        animator.SetTrigger(attackTrigger);
    }

    // =====================================
    // SHIELD ANIMATION
    // =====================================

    public virtual void PlayShieldAnimation()
    {
        if (animator != null &&
            !string.IsNullOrEmpty(shieldTrigger))
        {
            animator.SetTrigger(shieldTrigger);
        }
    }

    // =====================================
    // NORMAL ATTACK
    // =====================================

    public virtual void AttackWithoutTiming()
    {
        // 1. Phát Animation Attack.
        PlayAttackAnimation();

        // 2. Gây Damage.
        DealDamage(damage);

        Debug.Log(
            $"{gameObject.name} Normal Attack | " +
            $"Damage: {damage}"
        );
    }

    // =====================================
    // ATTACK WITH TIMING
    // =====================================

    public virtual void ApplyTimingDamage(float multiplier)
    {
        int finalDamage = Mathf.Max(
            0,
            Mathf.RoundToInt(damage * multiplier)
        );

        // Không gọi Animation ở đây vì
        // Timing có thể đã được phát sau Animation
        // bởi hệ thống Cinematic.

        DealDamage(finalDamage);

        Debug.Log(
            $"{gameObject.name} Timing Attack | " +
            $"Multiplier: {multiplier} | " +
            $"Damage: {finalDamage}"
        );
    }

    // =====================================
    // DEAL DAMAGE
    // =====================================

    protected virtual void DealDamage(int finalDamage)
    {
        if (cardEffectManager == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Chưa gán CardEffectManager!"
            );
            return;
        }

        if (finalDamage <= 0)
            return;

        cardEffectManager.TakeDamage(finalDamage);

        OnDamageCalculated?.Invoke(finalDamage);
    }

    // =====================================
    // GET DAMAGE
    // =====================================

    public int GetDamage()
    {
        return damage;
    }
}
