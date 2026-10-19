
using UnityEngine;

public class ShieldSkill : EnemySkillBase
{
    [Header("Shield Settings")]
    [SerializeField] private int shieldAmount = 20;

    protected override void ApplyEffect(
        CardEffectManager effectManager)
    {
        effectManager.ApplyEnemyShield(shieldAmount);

        Debug.Log(
            $"{gameObject.name} sử dụng Shield Skill! " +
            $"+{shieldAmount} Shield."
        );
    }
}
