
using UnityEngine;

public class EnemyAttack : EnemyAttackBase
{
    [Header("Enemy Skills")]
    [SerializeField] private EnemySkillBase[] skills;

    [Range(0f, 100f)]
    [SerializeField] private float skillChance = 50f;

    private EnemySkillBase selectedSkill;

    // =========================================
    // RANDOM ACTION
    // =========================================

    public virtual bool RandomAction()
    {
        selectedSkill = null;

        if (skills == null || skills.Length == 0)
            return true;

        float randomValue = Random.Range(0f, 100f);

        // true = Normal Attack
        if (randomValue >= skillChance)
        {
            Debug.Log("Enemy chọn Normal Attack.");
            return true;
        }

        // false = Skill
        int startIndex = Random.Range(0, skills.Length);

        for (int i = 0; i < skills.Length; i++)
        {
            int index = (startIndex + i) % skills.Length;

            if (skills[index] != null)
            {
                selectedSkill = skills[index];
                break;
            }
        }

        if (selectedSkill == null)
            return true;

        Debug.Log(
            $"Enemy chọn Skill: {selectedSkill.GetType().Name}"
        );

        return false;
    }

    // =========================================
    // USE SKILL
    // =========================================

    public virtual void UseSkill()
    {
        if (selectedSkill == null)
        {
            Debug.LogWarning(
                "Enemy chưa chọn Skill!"
            );
            return;
        }

        selectedSkill.Execute(
            cardEffectManager,
            animator
        );

        selectedSkill = null;
    }
}
