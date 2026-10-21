
using UnityEngine;

public class EnemyAttack : EnemyAttackBase
{
    [Header("Enemy Skills")]
    [SerializeField] private EnemySkillBase[] skills;

    [Range(0f, 100f)]
    [SerializeField] private float skillChance = 50f;

    private EnemySkillBase selectedSkill;

    public EnemySkillBase SelectedSkill => selectedSkill;

    // =========================================
    // RANDOM ACTION
    // =========================================

    public virtual bool RandomAction()
    {
        selectedSkill = null;

        // Nếu Enemy không có Skill thì đánh thường.
        if (skills == null || skills.Length == 0)
        {
            return true;
        }

        float randomValue = Random.Range(0f, 100f);

        // true = Normal Attack
        if (randomValue >= skillChance)
        {
            Debug.Log(
                $"{gameObject.name} chọn Normal Attack."
            );

            return true;
        }

        // Chọn một Skill hợp lệ trong danh sách.
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

        // Không có Skill hợp lệ thì đánh thường.
        if (selectedSkill == null)
            return true;

        Debug.Log(
            $"{gameObject.name} chọn Skill: " +
            selectedSkill.GetType().Name
        );

        return false;
    }

    // =========================================
    // USE SELECTED SKILL
    // =========================================

    public virtual void UseSkill()
    {
        if (selectedSkill == null)
        {
            Debug.LogWarning(
                "EnemyAttack: Chưa chọn Skill!"
            );
            return;
        }

        EnemySkillBase skill = selectedSkill;

        // Xóa lựa chọn sau khi lấy Skill,
        // tránh sử dụng lại ngoài ý muốn.
        selectedSkill = null;

        skill.Execute(
            cardEffectManager,
            animator
        );
    }
}
