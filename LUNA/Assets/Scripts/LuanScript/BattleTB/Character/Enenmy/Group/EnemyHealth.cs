
using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy HP")]
    [SerializeField] private int maxHP = 50;

    private int currentHP;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public bool IsDead => currentHP <= 0;

    public event Action<EnemyHealth> OnHealthChanged;
    public event Action<EnemyHealth> OnDied;

    private void Awake()
    {
        maxHP = Mathf.Max(1, maxHP);
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead)
            return;

        currentHP = Mathf.Max(0, currentHP - damage);

        Debug.Log(
            $"{gameObject.name} nhận {damage} Damage. " +
            $"HP: {currentHP}/{maxHP}"
        );

        OnHealthChanged?.Invoke(this);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
            return;

        currentHP = Mathf.Min(maxHP, currentHP + amount);

        OnHealthChanged?.Invoke(this);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} đã chết!");

        OnDied?.Invoke(this);
    }
}
