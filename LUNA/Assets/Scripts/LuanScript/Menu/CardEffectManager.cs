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

    [Header("Round")]
    [SerializeField] private RoundManager roundManager;

    [Header("UI")]
    [SerializeField] private TMP_Text playerHPText;
    [SerializeField] private TMP_Text shieldText;
    [SerializeField] private TMP_Text enemyHPText;
    [SerializeField] private TMP_Text enemyShieldText;


    // =========================================================
    // CARD DAMAGE
    // Card đánh Enemy
    // =========================================================

    public void ApplyDamage(CardData card)
    {
        if (card == null)
            return;

        int damage = card.baseDamage;
        int originalDamage = damage;

        // =========================
        // Shield của Enemy nhận damage trước
        // =========================
        if (enemyShield > 0)
        {
            int damageToShield = Mathf.Min(
                damage,
                enemyShield
            );

            enemyShield -= damageToShield;
            damage -= damageToShield;
        }

        // =========================
        // Damage còn dư mới vào HP
        // =========================
        if (damage > 0)
        {
            enemyHP -= damage;
        }

        // Không cho âm
        enemyHP = Mathf.Max(enemyHP, 0);
        enemyShield = Mathf.Max(enemyShield, 0);

        UpdateUI();

        Debug.Log(
            "Card Damage: " +
            originalDamage +
            " | Enemy HP: " +
            enemyHP +
            "/" +
            maxEnemyHP +
            " | Enemy Shield: " +
            enemyShield
        );

        CheckEnemyDead();
    }

    private void CheckEnemyDead()
    {
        if (enemyHP > 0)
            return;

        Debug.Log("CardEffectManager: Enemy HP = 0!");

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


    // =========================================================
    // CARD SHIELD
    // Card cộng Shield cho Player
    // =========================================================

    public void ApplyShield(CardData card)
    {
        if (card == null)
            return;

        int shield = card.shieldValue;

        playerShield += shield;

        playerShield = Mathf.Max(playerShield, 0);

        UpdateUI();

        Debug.Log(
            "Card Shield: Player nhận " +
            shield +
            " Shield. Shield hiện tại: " +
            playerShield
        );
    }

    // =========================================================
    // ENEMY SHIELD
    // Enemy tạo Shield
    // =========================================================

    public void ApplyEnemyShield(int shieldAmount)
    {
        if (shieldAmount <= 0)
            return;

        enemyShield += shieldAmount;
        enemyShield = Mathf.Max(enemyShield, 0);

        UpdateUI();

        Debug.Log(
            "Enemy Shield: +" +
            shieldAmount +
            " | Enemy Shield hiện tại: " +
            enemyShield
        );
    }


    // =========================================================
    // CARD HEAL
    // Card hồi HP cho Player
    // =========================================================

    public void ApplyHeal(CardData card)
    {
        if (card == null)
            return;

        int heal = card.healValue;

        playerHP += heal;

        playerHP = Mathf.Min(playerHP, maxPlayerHP);

        UpdateUI();

        Debug.Log(
            "Card Heal: Player hồi " +
            heal +
            " HP. Player HP: " +
            playerHP +
            "/" +
            maxPlayerHP
        );
    }


    // =========================================================
    // EXECUTE CARD EFFECT
    // =========================================================

    public void ExecuteCardEffect(CardData card)
    {
        if (card == null)
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
        }
    }


    // =========================================================
    // ENEMY DAMAGE
    // Enemy đánh Player
    // Shield nhận trước
    // Damage dư mới vào HP
    // =========================================================

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
            return;

        int originalDamage = damage;

        // -------------------------
        // Shield nhận damage trước
        // -------------------------

        if (playerShield > 0)
        {
            int damageToShield = Mathf.Min(
                damage,
                playerShield
            );

            playerShield -= damageToShield;

            damage -= damageToShield;
        }


        // -------------------------
        // Damage còn dư vào HP
        // -------------------------

        if (damage > 0)
        {
            playerHP -= damage;
        }


        // Không cho âm
        playerHP = Mathf.Max(playerHP, 0);
        playerShield = Mathf.Max(playerShield, 0);

        UpdateUI();

        Debug.Log(
            "Enemy Damage: " +
            originalDamage +
            " | Player HP: " +
            playerHP +
            "/" +
            maxPlayerHP +
            " | Shield: " +
            playerShield
        );
    }


    // =========================================================
    // UPDATE UI
    // =========================================================

    private void UpdateUI()
    {
        if (playerHPText != null)
        {
            playerHPText.text =
                "HP: " +
                playerHP +
                "/" +
                maxPlayerHP;
        }

        if (shieldText != null)
        {
            shieldText.text =
                "Shield: " +
                playerShield;
        }

        if (enemyHPText != null)
        {
            enemyHPText.text =
                "HP: " +
                enemyHP +
                "/" +
                maxEnemyHP;
        }

        if (enemyShieldText != null)
        {
            enemyShieldText.text =
                "Shield: " +
                enemyShield;
        }
    }


    // =========================================================
    // GETTERS
    // =========================================================

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

    public void NextRoundEnemyHP()
    {
        maxEnemyHP *= 2;

        enemyHP = maxEnemyHP;

        UpdateUI();

        Debug.Log(
            "Enemy sang Round mới: HP = " +
            enemyHP +
            "/" +
            maxEnemyHP
        );
    }
}