using UnityEngine;
using TMPro;

public class CardEffectManager : MonoBehaviour
{
    [Header("Player HP")]
    [SerializeField] private int playerHP = 100;
    [SerializeField] private int maxPlayerHP = 100;

    [Header("Player Shield")]
    [SerializeField] private int playerShield = 0;

    [Header("Enemy HP")]
    [SerializeField] private int enemyHP = 50;
    [SerializeField] private int maxEnemyHP = 50;

    [Header("Enemy Shield")]
    [SerializeField] private int enemyShield = 0;

    [Header("Managers")]
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private BattleStateMachine battleStateMachine;

    [Header("UI")]
    [SerializeField] private TMP_Text playerHPText;
    [SerializeField] private TMP_Text shieldText;
    [SerializeField] private TMP_Text enemyHPText;
    [SerializeField] private TMP_Text enemyShieldText;

    private bool enemyDefeated;
    private bool playerDefeated;

    public bool IsEnemyDefeated => enemyDefeated;
    public bool IsPlayerDefeated => playerDefeated;

    private void Start()
    {
        playerHP = Mathf.Clamp(playerHP, 0, maxPlayerHP);
        enemyHP = Mathf.Clamp(enemyHP, 0, maxEnemyHP);

        UpdateUI();

        if (playerHP <= 0)
            CheckPlayerDead();

        if (enemyHP <= 0)
            CheckEnemyDead();
    }

    // =========================================
    // CARD DAMAGE
    // =========================================

    public void ApplyDamage(CardData card)
    {
        if (card == null || enemyDefeated || playerDefeated)
            return;

        int damage = Mathf.Max(0, card.baseDamage);
        int originalDamage = damage;

        // Enemy Shield nhận sát thương trước.
        int damageToShield = Mathf.Min(damage, enemyShield);

        enemyShield -= damageToShield;
        damage -= damageToShield;

        // Sát thương còn dư mới trừ HP.
        enemyHP -= damage;

        enemyHP = Mathf.Max(0, enemyHP);
        enemyShield = Mathf.Max(0, enemyShield);

        UpdateUI();

        Debug.Log(
            $"Card Damage: {originalDamage} | " +
            $"Enemy HP: {enemyHP}/{maxEnemyHP} | " +
            $"Enemy Shield: {enemyShield}"
        );

        CheckEnemyDead();
    }

    private void CheckEnemyDead()
    {
        if (enemyHP > 0 || enemyDefeated)
            return;

        enemyDefeated = true;

        Debug.Log("CardEffectManager: Enemy đã bị hạ.");

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

        // Player Shield nhận sát thương trước.
        int damageToShield = Mathf.Min(damage, playerShield);

        playerShield -= damageToShield;
        damage -= damageToShield;

        // Sát thương còn dư mới trừ HP.
        playerHP -= damage;

        playerHP = Mathf.Max(0, playerHP);
        playerShield = Mathf.Max(0, playerShield);

        UpdateUI();

        Debug.Log(
            $"Enemy Damage: {originalDamage} | " +
            $"Player HP: {playerHP}/{maxPlayerHP} | " +
            $"Player Shield: {playerShield}"
        );

        CheckPlayerDead();
    }

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
    // NEXT ROUND
    // =========================================

    public void NextRoundEnemyHP()
    {
        if (playerDefeated)
            return;

        maxEnemyHP = Mathf.Max(1, maxEnemyHP * 2);
        enemyHP = maxEnemyHP;

        // Reset trạng thái để kẻ địch Round mới hoạt động.
        enemyDefeated = false;
        enemyShield = 0;

        UpdateUI();

        Debug.Log(
            $"Enemy Round mới: HP = {enemyHP}/{maxEnemyHP}"
        );
    }

    // =========================================
    // UPDATE UI
    // =========================================

    private void UpdateUI()
    {
        if (playerHPText != null)
            playerHPText.text = $"HP: {playerHP}/{maxPlayerHP}";

        if (shieldText != null)
            shieldText.text = $"Shield: {playerShield}";

        if (enemyHPText != null)
            enemyHPText.text = $"HP: {enemyHP}/{maxEnemyHP}";

        if (enemyShieldText != null)
            enemyShieldText.text = $"Shield: {enemyShield}";
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
        return enemyHP;
    }

    public int GetEnemyShield()
    {
        return enemyShield;
    }
}