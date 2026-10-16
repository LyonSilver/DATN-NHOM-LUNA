
using UnityEngine;

public abstract class EnemySkillBase : MonoBehaviour
{
    [Header("Skill Animation")]
    [SerializeField] protected string skillTrigger = "Skill";

    public void Execute(
        CardEffectManager effectManager,
        Animator animator)
    {
        if (effectManager == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Chưa có CardEffectManager!"
            );
            return;
        }

        if (animator != null &&
            !string.IsNullOrEmpty(skillTrigger))
        {
            animator.SetTrigger(skillTrigger);
        }

        ApplyEffect(effectManager);
    }

    protected PlayerStatusController GetPlayerStatus(
        CardEffectManager effectManager)
    {
        PlayerStatusController status =
            effectManager.GetComponent<PlayerStatusController>();

        if (status == null)
        {
            Debug.LogError(
                "Chưa gắn PlayerStatusController " +
                "lên GameObject chứa CardEffectManager!"
            );
        }

        return status;
    }

    protected abstract void ApplyEffect(
        CardEffectManager effectManager
    );
}
