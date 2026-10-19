
using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy HP")]
    [SerializeField] private int maxHP = 50;

    private int currentHP;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [SerializeField] private string hurtTrigger = "Hurt";
    [SerializeField] private string deathTrigger = "Death";

    [Header("Death Animation")]
    [SerializeField] private AnimationClip deathAnimationClip;
    [SerializeField] private float defaultDeathDuration = 1f;

    private bool deathTriggered = false;

    // =========================================
    // PROPERTIES
    // =========================================

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    public bool IsDead => currentHP <= 0;

    public float DeathAnimationDuration
    {
        get
        {
            if (deathAnimationClip != null)
            {
                return deathAnimationClip.length;
            }

            return Mathf.Max(0f, defaultDeathDuration);
        }
    }

    // =========================================
    // EVENTS
    // =========================================

    public event Action<EnemyHealth> OnHealthChanged;
    public event Action<EnemyHealth> OnDied;

    // =========================================
    // AWAKE
    // =========================================

    private void Awake()
    {
        maxHP = Mathf.Max(1, maxHP);
        currentHP = maxHP;

        deathTriggered = false;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }
    }

    // =========================================
    // TAKE DAMAGE
    // =========================================

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead)
            return;

        currentHP = Mathf.Max(
            0,
            currentHP - damage
        );

        Debug.Log(
            $"{gameObject.name} nhận {damage} Damage. " +
            $"HP: {currentHP}/{maxHP}"
        );

        // Cập nhật Enemy HP UI.
        OnHealthChanged?.Invoke(this);

        // Kiểm tra HP sau khi nhận Damage.
        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            PlayHurtAnimation();
        }
    }

    // =========================================
    // HEAL
    // =========================================

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
            return;

        currentHP = Mathf.Min(
            maxHP,
            currentHP + amount
        );

        OnHealthChanged?.Invoke(this);

        Debug.Log(
            $"{gameObject.name} hồi {amount} HP. " +
            $"HP: {currentHP}/{maxHP}"
        );
    }

    // =========================================
    // HURT ANIMATION
    // =========================================

    private void PlayHurtAnimation()
    {
        if (animator == null ||
            string.IsNullOrEmpty(hurtTrigger))
        {
            return;
        }

        animator.SetTrigger(hurtTrigger);

        Debug.Log(
            $"{gameObject.name} phát Hurt Animation!"
        );
    }

    // =========================================
    // DIE
    // =========================================

    private void Die()
    {
        if (deathTriggered)
            return;

        deathTriggered = true;

        Debug.Log(
            $"{gameObject.name} đã chết!"
        );

        // Phát Death Animation.
        PlayDeathAnimation();

        // Thông báo Enemy đã chết.
        // RoundManager sẽ chờ Animation hoàn thành
        // rồi mới xóa Enemy.
        OnDied?.Invoke(this);
    }

    // =========================================
    // DEATH ANIMATION
    // =========================================

    private void PlayDeathAnimation()
    {
        if (animator == null ||
            string.IsNullOrEmpty(deathTrigger))
        {
            return;
        }

        animator.SetTrigger(deathTrigger);

        Debug.Log(
            $"{gameObject.name} phát Death Animation!"
        );
    }
}