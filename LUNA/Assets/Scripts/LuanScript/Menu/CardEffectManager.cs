
using UnityEngine;
using TMPro;

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

    [Header("Player UI")]
    [SerializeField] private TMP_Text playerHPText;
    [SerializeField] private TMP_Text shieldText;

    [Header("Enemy UI")]
    [SerializeField] private EnemyHPBar enemyHPBar;

    private EnemyHealth currentEnemyHealth;

    private bool enemyDefeated;
    private bool playerDefeated;

    public bool IsEnemyDefeated => enemyDefeated;
    public bool IsPlayerDefeated => playerDefeated;

    private void Start()
    {
        maxPlayerHP = Mathf.Max(1, maxPlayerHP);
        playerHP = Mathf.Clamp(playerHP, 0, maxPlayerHP);

        UpdateUI();

        if (playerHP <= 0)
            CheckPlayerDead();
    }

    // =========================================
    // SET CURRENT ENEMY
    // =========================================

    public void SetCurrentEnemy(EnemyHealth enemy)
    {
        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.OnHealthChanged -= HandleEnemyHealthChanged;
            currentEnemyHealth.OnDied -= HandleEnemyDied;
        }

        currentEnemyHealth = enemy;

        enemyShield = 0;
        enemyDefeated = enemy == null || enemy.IsDead;

        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.OnHealthChanged += HandleEnemyHealthChanged;
            currentEnemyHealth.OnDied += HandleEnemyDied;
        }

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.OnHealthChanged -= HandleEnemyHealthChanged;
            currentEnemyHealth.OnDied -= HandleEnemyDied;
        }
    }

    private void HandleEnemyHealthChanged(EnemyHealth enemy)
    {
        UpdateUI();
    }

    private void HandleEnemyDied(EnemyHealth enemy)
    {
        if (enemy != currentEnemyHealth || enemyDefeated)
            return;

        enemyDefeated = true;

        Debug.Log("CardEffectManager: Enemy đã bị hạ.");

        UpdateUI();

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
    // CARD DAMAGE
    // =========================================

    public void ApplyDamage(CardData card)
    {
        if (card == null || enemyDefeated || playerDefeated)
            return;

        if (currentEnemyHealth == null)
        {
            Debug.LogWarning("Chưa có Enemy để nhận Damage!");
            return;
        }

        int damage = Mathf.Max(0, card.baseDamage);
        int originalDamage = damage;

        // Enemy Shield nhận sát thương trước.
        int damageToShield = Mathf.Min(damage, enemyShield);

        enemyShield -= damageToShield;
        damage -= damageToShield;

        // Sát thương còn dư gây lên EnemyHealth.
        if (damage > 0)
        {
            currentEnemyHealth.TakeDamage(damage);
        }

        UpdateUI();

        Debug.Log(
            $"Card Damage: {originalDamage} | " +
            $"Enemy Shield: {enemyShield}"
        );
    }

    // =========================================
    // PLAYER SHIELD
    // =========================================

    public void ApplyShield(CardData card)
    {
        if (card == null || playerDefeated)
            return;

        int shield = Mathf.Max(0, card.shieldValue);

        playerShield += shield;

        UpdateUI();

        Debug.Log(
            $"Player nhận {shield} Shield. " +
            $"Shield hiện tại: {playerShield}"
        );
    }

    // =========================================
    // ENEMY SHIELD
    // =========================================

    public void ApplyEnemyShield(int shieldAmount)
    {
        if (shieldAmount <= 0 || enemyDefeated || playerDefeated)
            return;

        enemyShield += shieldAmount;

        UpdateUI();

        Debug.Log(
            $"Enemy nhận {shieldAmount} Shield. " +
            $"Enemy Shield: {enemyShield}"
        );
    }

    // =========================================
    // PLAYER HEAL
    // =========================================

    public void ApplyHeal(CardData card)
    {
        if (card == null || playerDefeated)
            return;

        int heal = Mathf.Max(0, card.healValue);

        playerHP = Mathf.Min(maxPlayerHP, playerHP + heal);

        UpdateUI();

        Debug.Log(
            $"Player hồi {heal} HP. " +
            $"Player HP: {playerHP}/{maxPlayerHP}"
        );
    }

    // =========================================
    // EXECUTE CARD EFFECT
    // =========================================

    public void ExecuteCardEffect(CardData card)
    {
        if (card == null || playerDefeated || enemyDefeated)
            return;

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
                    $"Chưa xử lý hiệu ứng: {card.effectType}"
                );
                break;
        }
    }

    // =========================================
    // ENEMY DAMAGE TO PLAYER
    // =========================================

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || playerDefeated || enemyDefeated)
            return;

        int originalDamage = damage;

        int damageToShield = Mathf.Min(damage, playerShield);

        playerShield -= damageToShield;
        damage -= damageToShield;

        playerHP = Mathf.Max(0, playerHP - damage);
        playerShield = Mathf.Max(0, playerShield);

        UpdateUI();

        Debug.Log(
            $"Enemy Damage: {originalDamage} | " +
            $"Player HP: {playerHP}/{maxPlayerHP} | " +
            $"Player Shield: {playerShield}"
        );

        CheckPlayerDead();
    }

    // =========================================
    // CHECK PLAYER DEAD
    // =========================================

    private void CheckPlayerDead()
    {
        if (playerHP > 0 || playerDefeated)
            return;

        playerDefeated = true;

        Debug.Log("CardEffectManager: Player đã bị hạ.");

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
        // Giữ phương thức để RoundManager cũ
        // không bị lỗi compile.
        // HP Enemy mới được khởi tạo từ Prefab
        // bởi EnemySpawner.
        Debug.Log(
            "Enemy HP sẽ được lấy từ EnemyHealth của Prefab mới."
        );
    }

    // =========================================
    // UPDATE UI
    // =========================================

    private void UpdateUI()
    {
        if (playerHPText != null)
        {
            playerHPText.text =
                $"HP: {playerHP}/{maxPlayerHP}";
        }

        if (shieldText != null)
        {
            shieldText.text =
                $"Shield: {playerShield}";
        }

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

    public int GetPlayerShield()
    {
        return playerShield;
    }

    public int GetEnemyHP()
    {
        return currentEnemyHealth != null
            ? currentEnemyHealth.CurrentHP
            : 0;
    }

    public int GetEnemyShield()
    {
        return enemyShield;
    }


    public void TakeStatusDamage(int damage)
    {
        if (damage <= 0 || playerDefeated)
            return;

        // Damage từ trạng thái bỏ qua Shield
        playerHP = Mathf.Max(0, playerHP - damage);

        UpdateUI();

        Debug.Log(
            $"Player nhận {damage} Status Damage. " +
            $"HP: {playerHP}/{maxPlayerHP}"
        );

        CheckPlayerDead();
    }

}
