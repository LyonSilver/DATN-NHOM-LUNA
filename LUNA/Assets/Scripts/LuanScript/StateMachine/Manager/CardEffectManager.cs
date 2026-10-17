
using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    [Header("Player HP")]
    [SerializeField] private int playerHP = 100;
    [SerializeField] private int maxPlayerHP = 100;

    [Header("Player Shield")]
    [SerializeField] private int playerShield = 0;

    [Header("Enemy Shield")]
    [SerializeField] private int enemyShield = 0;

    [Header("Managers")]
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private BattleStateMachine battleStateMachine;

    [Header("Player HP UI")]
    [SerializeField] private PlayerHPBar playerHPBar;

    [Header("Enemy HP UI")]
    [SerializeField] private EnemyHPBar enemyHPBar;

    // Enemy hiện tại do EnemySpawner cung cấp
    private EnemyHealth currentEnemyHealth;

    private bool enemyDefeated = true;
    private bool playerDefeated = false;

    public bool IsEnemyDefeated => enemyDefeated;
    public bool IsPlayerDefeated => playerDefeated;

    // =========================================
    // START
    // =========================================

    private void Start()
    {
        maxPlayerHP = Mathf.Max(1, maxPlayerHP);

        playerHP = Mathf.Clamp(
            playerHP, 0, maxPlayerHP
        );

        playerShield = Mathf.Max(0, playerShield);

        UpdateUI();

        if (playerHP <= 0)
        {
            CheckPlayerDead();
        }
    }

    // =========================================
    // SET CURRENT ENEMY
    // =========================================

    public void SetCurrentEnemy(EnemyHealth enemy)
    {
        // Hủy đăng ký Enemy cũ
        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.OnHealthChanged -=
                HandleEnemyHealthChanged;

            currentEnemyHealth.OnDied -=
                HandleEnemyDied;
        }

        // Cập nhật Enemy mới
        currentEnemyHealth = enemy;

        // Reset Shield của Enemy
        enemyShield = 0;

        enemyDefeated =
            enemy == null || enemy.IsDead;

        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.OnHealthChanged +=
                HandleEnemyHealthChanged;

            currentEnemyHealth.OnDied +=
                HandleEnemyDied;
        }

        UpdateUI();
    }

    // =========================================
    // ENEMY HEALTH CHANGED
    // =========================================

    private void HandleEnemyHealthChanged(
        EnemyHealth enemy)
    {
        if (enemy != currentEnemyHealth)
            return;

        UpdateUI();
    }

    // =========================================
    // ENEMY DEAD
    // =========================================

    private void HandleEnemyDied(
        EnemyHealth enemy)
    {
        if (enemy != currentEnemyHealth ||
            enemyDefeated)
        {
            return;
        }

        enemyDefeated = true;

        UpdateUI();

        Debug.Log(
            "CardEffectManager: Enemy đã bị hạ!"
        );

        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.CheckingBattle
            );
        }

        if (roundManager != null)
        {
            roundManager.EnemyDefeated();
        }
        else
        {
            Debug.LogError(
                "CardEffectManager: Chưa gán RoundManager!"
            );
        }
    }

    // =========================================
    // CARD DAMAGE TO ENEMY
    // =========================================

    public void ApplyDamage(CardData card)
    {
        if (card == null ||
            enemyDefeated ||
            playerDefeated)
        {
            return;
        }

        if (currentEnemyHealth == null)
        {
            Debug.LogWarning(
                "CardEffectManager: Chưa có Enemy!"
            );
            return;
        }

        int damage = Mathf.Max(
            0, card.baseDamage
        );

        int originalDamage = damage;

        // Enemy Shield nhận Damage trước
        int damageToShield = Mathf.Min(
            damage, enemyShield
        );

        enemyShield -= damageToShield;
        damage -= damageToShield;

        // Damage còn lại trừ HP Enemy
        if (damage > 0)
        {
            currentEnemyHealth.TakeDamage(damage);
        }

        UpdateUI();

        Debug.Log(
            $"Card Damage: {originalDamage} | " +
            $"Damage to Shield: {damageToShield}"
        );
    }

    // =========================================
    // PLAYER SHIELD
    // =========================================

    public void ApplyShield(CardData card)
    {
        if (card == null || playerDefeated)
            return;

        int shield = Mathf.Max(
            0, card.shieldValue
        );

        playerShield += shield;

        UpdateUI();

        Debug.Log(
            $"Player nhận {shield} Shield. " +
            $"Tổng Shield: {playerShield}"
        );
    }

    // =========================================
    // ENEMY SHIELD
    // =========================================

    public void ApplyEnemyShield(int shieldAmount)
    {
        if (shieldAmount <= 0 ||
            enemyDefeated ||
            playerDefeated)
        {
            return;
        }

        enemyShield += shieldAmount;

        UpdateUI();

        Debug.Log(
            $"Enemy nhận {shieldAmount} Shield. " +
            $"Tổng Shield: {enemyShield}"
        );
    }

    // =========================================
    // PLAYER HEAL
    // =========================================

    public void ApplyHeal(CardData card)
    {
        if (card == null || playerDefeated)
            return;

        int heal = Mathf.Max(
            0, card.healValue
        );

        playerHP = Mathf.Min(
            maxPlayerHP,
            playerHP + heal
        );

        UpdateUI();

        Debug.Log(
            $"Player Heal: +{heal} | " +
            $"HP: {playerHP}/{maxPlayerHP}"
        );
    }

    // =========================================
    // EXECUTE CARD EFFECT
    // =========================================

    public void ExecuteCardEffect(CardData card)
    {
        if (card == null ||
            playerDefeated ||
            enemyDefeated)
        {
            return;
        }

        switch (card.effectType)
        {
            case CardEffectType.Damage:
                ApplyDamage(card);
                break;

            case CardEffectType.Shield:
                ApplyShield(card);
                break;

            case CardEffectType.Heal:
                ApplyHeal(card);
                break;

            default:
                Debug.LogWarning(
                    $"Chưa xử lý Card Effect: " +
                    $"{card.effectType}"
                );
                break;
        }
    }

    // =========================================
    // ENEMY DAMAGE TO PLAYER
    // =========================================

    public void TakeDamage(int damage)
    {
        if (damage <= 0 ||
            playerDefeated ||
            enemyDefeated)
        {
            return;
        }

        int originalDamage = damage;

        // Player Shield nhận Damage trước
        int damageToShield = Mathf.Min(
            damage, playerShield
        );

        playerShield -= damageToShield;
        damage -= damageToShield;

        // Trừ HP còn lại
        playerHP = Mathf.Max(
            0, playerHP - damage
        );

        UpdateUI();

        Debug.Log(
            $"Enemy Damage: {originalDamage} | " +
            $"Player HP: {playerHP}/{maxPlayerHP} | " +
            $"Shield: {playerShield}"
        );

        CheckPlayerDead();
    }

    // =========================================
    // STATUS DAMAGE (POISON / BURN)
    // =========================================

    public void TakeStatusDamage(int damage)
    {
        if (damage <= 0 || playerDefeated)
            return;

        // DoT bỏ qua Shield
        playerHP = Mathf.Max(
            0, playerHP - damage
        );

        UpdateUI();

        Debug.Log(
            $"Player nhận {damage} DoT Damage. " +
            $"HP: {playerHP}/{maxPlayerHP}"
        );

        CheckPlayerDead();
    }

    // =========================================
    // PLAYER DEAD
    // =========================================

    private void CheckPlayerDead()
    {
        if (playerHP > 0 || playerDefeated)
            return;

        playerDefeated = true;

        Debug.Log(
            "CardEffectManager: Player đã bị hạ!"
        );

        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.Defeat
            );
        }

        if (roundManager != null)
        {
            roundManager.PlayerDefeated();
        }
        else
        {
            Debug.LogError(
                "CardEffectManager: Chưa gán RoundManager!"
            );
        }
    }

    // =========================================
    // NEXT ROUND (LEGACY)
    // =========================================

    public void NextRoundEnemyHP()
    {
        // HP Enemy mới lấy từ EnemyHealth
        // của Prefab được EnemySpawner tạo.
        // Giữ hàm để tương thích code cũ.
        Debug.Log(
            "Enemy HP được quản lý bởi EnemyHealth."
        );
    }

    // =========================================
    // UPDATE UI
    // =========================================

    private void UpdateUI()
    {
        // Player HP Image
        if (playerHPBar != null)
        {
            playerHPBar.SetHP(
                playerHP,
                maxPlayerHP
            );
        }

        // Enemy HP Image
        if (enemyHPBar != null)
        {
            if (currentEnemyHealth != null)
            {
                enemyHPBar.SetHP(
                    currentEnemyHealth.CurrentHP,
                    currentEnemyHealth.MaxHP
                );
            }
            else
            {
                enemyHPBar.SetHP(0, 1);
            }
        }
    }

    // =========================================
    // GETTERS
    // =========================================

    public int GetPlayerHP()
    {
        return playerHP;
    }

    public int GetMaxPlayerHP()
    {
        return maxPlayerHP;
    }

    public int GetPlayerShield()
    {
        return playerShield;
    }

    public int GetEnemyHP()
    {
        if (currentEnemyHealth == null)
            return 0;

        return currentEnemyHealth.CurrentHP;
    }

    public int GetEnemyShield()
    {
        return enemyShield;
    }

    // =========================================
    // CLEANUP EVENTS
    // =========================================

    private void OnDestroy()
    {
        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.OnHealthChanged -=
                HandleEnemyHealthChanged;

            currentEnemyHealth.OnDied -=
                HandleEnemyDied;
        }
    }
}
