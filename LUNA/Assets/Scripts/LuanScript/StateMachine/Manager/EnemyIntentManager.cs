
using UnityEngine;
using UnityEngine.UI;

public class EnemyIntentManager : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Intent UI")]
    [SerializeField] private Image intentIconImage;

    [Header("Default Icons")]
    [SerializeField] private Sprite normalAttackIcon;
    [SerializeField] private Sprite defaultSkillIcon;

    private EnemyAttack preparedEnemy;
    private bool hasPreparedIntent;
    private bool preparedIsAttack;

    private void Awake()
    {
        HideIntent();
    }

    // =========================================
    // CHỌN TRƯỚC HÀNH ĐỘNG
    // =========================================

    public bool PrepareIntent()
    {
        ClearIntent();

        if (enemySpawner == null)
        {
            Debug.LogError(
                "EnemyIntentManager: Chưa gán EnemySpawner!"
            );
            return false;
        }

        EnemyAttack enemy =
            enemySpawner.GetCurrentEnemyAttack();

        if (enemy == null)
        {
            Debug.LogWarning(
                "EnemyIntentManager: Không có Enemy hiện tại!"
            );
            return false;
        }

        // Random chỉ một lần trước Player Turn.
        bool isAttack = enemy.RandomAction();

        if (!isAttack && enemy.SelectedSkill == null)
        {
            Debug.LogError(
                "EnemyIntentManager: Enemy chọn Skill " +
                "nhưng không tìm thấy Skill!"
            );
            return false;
        }

        preparedEnemy = enemy;
        preparedIsAttack = isAttack;
        hasPreparedIntent = true;

        Sprite icon;

        if (isAttack)
        {
            icon = normalAttackIcon;

            Debug.Log(
                "Enemy Intent: NORMAL ATTACK"
            );
        }
        else
        {
            EnemySkillBase skill = enemy.SelectedSkill;

            icon = skill.IntentIcon != null
                ? skill.IntentIcon
                : defaultSkillIcon;

            Debug.Log(
                $"Enemy Intent: {skill.GetType().Name}"
            );
        }

        if (intentIconImage != null)
        {
            intentIconImage.sprite = icon;
            intentIconImage.enabled = icon != null;
        }

        return true;
    }

    // =========================================
    // LẤY HÀNH ĐỘNG ĐÃ CHỌN
    // =========================================

    public bool TryConsumeIntent(
        out EnemyAttack enemy,
        out bool isAttack)
    {
        enemy = null;
        isAttack = true;

        if (!hasPreparedIntent || preparedEnemy == null)
        {
            Debug.LogWarning(
                "EnemyIntentManager: Chưa chuẩn bị Intent!"
            );
            return false;
        }

        // Đảm bảo đây vẫn là Enemy đang chiến đấu.
        if (enemySpawner == null ||
            preparedEnemy !=
            enemySpawner.GetCurrentEnemyAttack())
        {
            ClearIntent();

            Debug.LogWarning(
                "EnemyIntentManager: Enemy đã thay đổi!"
            );
            return false;
        }

        enemy = preparedEnemy;
        isAttack = preparedIsAttack;

        // Chỉ xóa Intent trên UI.
        // Skill đã chọn vẫn nằm trong EnemyAttack
        // cho tới khi UseSkill() thực hiện.
        ClearIntent();

        return true;
    }

    // =========================================
    // CLEAR INTENT
    // =========================================

    public void ClearIntent()
    {
        preparedEnemy = null;
        hasPreparedIntent = false;
        preparedIsAttack = false;

        HideIntent();
    }

    private void HideIntent()
    {
        if (intentIconImage != null)
        {
            intentIconImage.enabled = false;
        }
    }
}
