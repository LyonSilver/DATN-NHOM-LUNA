using System;
using UnityEngine;

public abstract class EnemyAttackBase : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] protected int damage = 10;

    [Header("Animation")]
    [SerializeField] protected Animator animator;

    // Event gửi Damage đến hệ thống xử lý HP
    public event Action<int> OnDamageCalculated;

    // Tấn công bình thường, không có Timing
    public virtual void AttackWithoutTiming()
    {
        PlayAttackAnimation();

        DealDamage(damage);
    }

    // Tấn công có Timing
    public virtual void ApplyTimingDamage(float multiplier)
    {
        PlayAttackAnimation();

        int finalDamage = Mathf.RoundToInt(damage * multiplier);

        Debug.Log(
            $"Base Damage: {damage} | " +
            $"Multiplier: {multiplier} | " +
            $"Final Damage: {finalDamage}"
        );

        DealDamage(finalDamage);
    }

    // Xử lý Damage
    protected virtual void DealDamage(int finalDamage)
    {
        OnDamageCalculated?.Invoke(finalDamage);

        Debug.Log($"{gameObject.name} dealt {finalDamage} damage");
    }

    // Animation tấn công
    protected virtual void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }
}