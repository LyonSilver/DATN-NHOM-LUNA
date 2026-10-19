
using System.Collections;
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

    [Header("Player Animation")]
    [SerializeField] private PlayerAnimationController playerAnimation;

    [Header("Player Death Animation")]
    [SerializeField] private float deathAnimationDuration = 1f;

    // Enemy đang được EnemySpawner quản lý.
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
        // Hủy đăng ký Enemy cũ.
        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.OnHealthChanged -=
                HandleEnemyHealthChanged;

            currentEnemyHealth.OnDied -=
                HandleEnemyDied;
        }

        // Gán Enemy mới.
        currentEnemyHealth = enemy;

        // Reset Shield khi đổi Enemy.
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
    // ENEMY DIED
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

        if (currentEnemyHealth == null ||
            currentEnemyHealth.IsDead)
        {
            Debug.LogWarning(
                "CardEffectManager: Chưa có Enemy hợp lệ!"
            );
            return;
        }

        int damage = Mathf.Max(
            0, card.baseDamage
        );

        if (damage <= 0)
        {
            Debug.Log(
                "CardEffectManager: Card không gây Damage."
            );
            return;
        }

        int originalDamage = damage;

        // =====================================
        // PLAYER ATTACK ANIMATION
        // =====================================

        // Chỉ phát khi Card Damage được thực thi.
        if (playerAnimation != null)
        {
            playerAnimation.PlayAttack();
        }

        // =====================================
        // ENEMY SHIELD
        // =====================================

        int damageToShield = Mathf.Min(
            damage, enemyShield
        );

        enemyShield -= damageToShield;
        damage -= damageToShield;

        // =====================================
        // ENEMY HP DAMAGE
        // =====================================

        if (damage > 0)
        {
            currentEnemyHealth.TakeDamage(damage);
        }

        UpdateUI();

        Debug.Log(
            $"Card Damage: {originalDamage} | " +
            $"Damage to Shield: {damageToShield} | " +
            $"Damage to HP: {damage}"
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

        // Không phát Attack Animation.
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

        // Không phát Attack Animation.
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

        // =====================================
        // PLAYER SHIELD
        // =====================================

        int damageToShield = Mathf.Min(
            damage, playerShield
        );

        playerShield -= damageToShield;
        damage -= damageToShield;

        // =====================================
        // PLAYER HP
        // =====================================

        if (damage > 0)
        {
            playerHP = Mathf.Max(
                0, playerHP - damage
            );
        }

        UpdateUI();

        Debug.Log(
            $"Enemy Damage: {originalDamage} | " +
            $"Damage to Shield: {damageToShield} | " +
            $"Damage to HP: {damage} | " +
            $"Player HP: {playerHP}/{maxPlayerHP}"
        );

        // =====================================
        // PLAYER ANIMATION
        // =====================================

        if (playerHP <= 0)
        {
            // Death sẽ được phát trong CheckPlayerDead.
            CheckPlayerDead();
        }
        else if (damage > 0)
        {
            // Chỉ Hurt khi Player thực sự mất HP.
            playerAnimation?.PlayHurt();
        }
    }

    // =========================================
    // STATUS DAMAGE (POISON / BURN)
    // =========================================

    public void TakeStatusDamage(int damage)
    {
        if (damage <= 0 || playerDefeated)
            return;

        // DoT bỏ qua Shield.
        playerHP = Mathf.Max(
            0, playerHP - damage
        );

        UpdateUI();

        Debug.Log(
            $"Player nhận {damage} DoT Damage. " +
            $"HP: {playerHP}/{maxPlayerHP}"
        );

        if (playerHP <= 0)
        {
            CheckPlayerDead();
        }
        else
        {
            playerAnimation?.PlayHurt();
        }
    }

    // =========================================
    // CHECK PLAYER DEAD
    // =========================================

    private void CheckPlayerDead()
    {
        if (playerHP > 0 || playerDefeated)
            return;

        playerDefeated = true;

        Debug.Log(
            "CardEffectManager: Player đã bị hạ!"
        );

        // =====================================
        // PLAY DEATH ANIMATION
        // =====================================

        playerAnimation?.PlayDeath();

        // Đưa FSM vào trạng thái Defeat
        // để không tiếp tục lượt chơi.
        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.Defeat
            );
        }

        // Chờ Death Animation rồi mới
        // gọi RoundManager hiển thị Defeat.
        StartCoroutine(PlayerDeathCoroutine());
    }

    // =========================================
    // PLAYER DEATH COROUTINE
    // =========================================

    private IEnumerator PlayerDeathCoroutine()
    {
        float duration = Mathf.Max(
            0f, deathAnimationDuration
        );

        Debug.Log(
            $"Player Death Animation: {duration} giây."
        );

        if (duration > 0f)
        {
            yield return new WaitForSecondsRealtime(
                duration
            );
        }

        // =====================================
        // FINISH DEFEAT
        // =====================================

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
        // Giữ lại để tương thích code cũ.
        // HP của Enemy mới do EnemyHealth quản lý.

        Debug.Log(
            "Enemy HP được quản lý bởi EnemyHealth."
        );
    }

    // =========================================
    // UPDATE UI
    // =========================================

    private void UpdateUI()
    {
        // =====================================
        // PLAYER HP BAR
        // =====================================

        if (playerHPBar != null)
        {
            playerHPBar.SetHP(
                playerHP,
                maxPlayerHP
            );
        }

        // =====================================
        // ENEMY HP BAR
        // =====================================

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
