
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Animation Triggers")]
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string hurtTrigger = "Hurt";
    [SerializeField] private string deathTrigger = "Death";

    private bool isDead = false;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }
    }

    // =====================================
    // ATTACK ANIMATION
    // =====================================

    public void PlayAttack()
    {
        if (animator == null || isDead)
            return;

        animator.SetTrigger(attackTrigger);

        Debug.Log("Player: Attack Animation");
    }

    // =====================================
    // HURT ANIMATION
    // =====================================

    public void PlayHurt()
    {
        if (animator == null || isDead)
            return;

        animator.SetTrigger(hurtTrigger);

        Debug.Log("Player: Hurt Animation");
    }

    // =====================================
    // DEATH ANIMATION
    // =====================================

    public void PlayDeath()
    {
        if (animator == null || isDead)
            return;

        isDead = true;

        animator.ResetTrigger(attackTrigger);
        animator.ResetTrigger(hurtTrigger);

        animator.SetTrigger(deathTrigger);

        Debug.Log("Player: Death Animation");
    }
}
