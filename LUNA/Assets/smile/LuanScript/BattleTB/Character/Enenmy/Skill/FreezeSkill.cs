
using UnityEngine;

public class FreezeSkill : EnemySkillBase
{
    [Header("Freeze Settings")]
    [SerializeField] private int freezeDuration = 1;

    protected override void ApplyEffect(
        CardEffectManager effectManager)
    {
        PlayerStatusController status =
            GetPlayerStatus(effectManager);

        if (status == null)
            return;

        status.ApplyFreeze(freezeDuration);

        Debug.Log(
            $"{gameObject.name} sử dụng Freeze Skill! " +
            $"Player bị đóng băng {freezeDuration} lượt."
        );
    }
}
