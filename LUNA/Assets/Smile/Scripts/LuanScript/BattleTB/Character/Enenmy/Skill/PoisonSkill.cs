
using UnityEngine;

public class PoisonSkill : EnemySkillBase
{
    [Header("Poison Settings")]
    [SerializeField] private int poisonDamage = 5;
    [SerializeField] private int poisonDuration = 3;

    protected override void ApplyEffect(
        CardEffectManager effectManager)
    {
        PlayerStatusController status =
            GetPlayerStatus(effectManager);

        if (status == null)
            return;

        status.ApplyPoison(
            poisonDamage,
            poisonDuration
        );

        Debug.Log(
            $"{gameObject.name} sử dụng Poison Skill!"
        );
    }
}
