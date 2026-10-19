
using UnityEngine;

public abstract class EnemySkillBase : MonoBehaviour
{
    [Header("Intent UI")]
    [SerializeField] private Sprite intentIcon;

    [Header("Skill Animation")]
    [SerializeField] protected string skillTrigger = "Skill";

    public Sprite IntentIcon => intentIcon;

    // =========================================
    // EXECUTE SKILL
    // =========================================

    public void Execute(
        CardEffectManager effectManager,
        Animator animator)
    {
        if (effectManager == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Thiếu CardEffectManager!"
            );
            return;
        }

        // Phát Animation Skill
        if (animator != null &&
            !string.IsNullOrEmpty(skillTrigger))
        {
            animator.SetTrigger(skillTrigger);
        }

        // Áp dụng hiệu ứng
        ApplyEffect(effectManager);
    }

    // =========================================
    // PLAYER STATUS
    // =========================================

    protected PlayerStatusController GetPlayerStatus(
        CardEffectManager effectManager)
    {
        PlayerStatusController status =
            effectManager.GetComponent<PlayerStatusController>();

        if (status == null)
        {
            Debug.LogError(
                "Chưa gắn PlayerStatusController " +
                "vào EffectManager!"
            );
        }

        return status;
    }

    // =========================================
    // SKILL EFFECT
    // =========================================

    protected abstract void ApplyEffect(
        CardEffectManager effectManager
    );
}
