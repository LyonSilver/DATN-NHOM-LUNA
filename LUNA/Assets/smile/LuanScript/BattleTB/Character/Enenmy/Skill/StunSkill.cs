
using UnityEngine;

public class StunSkill : EnemySkillBase
{
    [Header("Stun Settings")]
    [SerializeField] private int stunDuration = 1;

    protected override void ApplyEffect(
        CardEffectManager effectManager)
    {
        PlayerStatusController status =
            GetPlayerStatus(effectManager);

        if (status == null)
            return;

        status.ApplyStun(stunDuration);

        Debug.Log(
            $"{gameObject.name} sử dụng Stun Skill! " +
            $"Player bị choáng {stunDuration} lượt."
        );
    }
}
