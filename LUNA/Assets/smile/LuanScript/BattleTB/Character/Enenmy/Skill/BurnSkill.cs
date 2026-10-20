
using UnityEngine;

public class BurnSkill : EnemySkillBase
{
    [Header("Burn Settings")]
    [SerializeField] private int burnDamage = 8;
    [SerializeField] private int burnDuration = 2;

    protected override void ApplyEffect(
        CardEffectManager effectManager)
    {
        PlayerStatusController status =
            GetPlayerStatus(effectManager);

        if (status == null)
            return;

        status.ApplyBurn(
            burnDamage,
            burnDuration
        );

        Debug.Log(
            $"{gameObject.name} sử dụng Burn Skill!"
        );
    }
}
