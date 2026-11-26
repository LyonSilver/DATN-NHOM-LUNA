
using System;
using System.Collections;
using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    // =====================================
    // CARD EFFECT TYPES
    // =====================================

    private const int DAMAGE = 0;
    private const int SHIELD = 1;
    private const int HEAL = 2;
    private const int POISON = 3;
    private const int FOCUS = 4;
    private const int MULTI_HIT = 5;
    private const int DRAW = 6;

    private const int POISON_STATUS = 1;

    // =====================================
    // PLAYER STATUS
    // =====================================

    [Header("Player HP")]
    [SerializeField] private int playerHP = 100;
    [SerializeField] private int maxPlayerHP = 100;

    [Header("Shield")]
    [SerializeField] private int playerShield;
    [SerializeField] private int enemyShield;

    // =====================================
    // REFERENCES
    // =====================================

    [Header("Battle Systems")]
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private BattleStateMachine battleStateMachine;
    [SerializeField] private BattleResultTracker battleStats;

    [Header("HP UI")]
    [SerializeField] private PlayerHPBar playerHPBar;
    [SerializeField] private EnemyHPBar enemyHPBar;

    [Header("Player Animation")]
    [SerializeField] private PlayerAnimationController playerAnimation;
    [SerializeField] private float deathAnimationDuration = 1f;

    [Header("Skill VFX")]
    [SerializeField] private CardSkillVFX cardSkillVFX;

    // =====================================
    // CARD SETTINGS
    // =====================================

    [Header("Card Settings")]
    [SerializeField] private int poisonDamagePerTurn = 2;

    [Range(0f, 100f)]
    [SerializeField] private float negativeEffectChance = 30f;

    [SerializeField] private int negativeEffectDamage = 3;

    // =====================================
    // RUNTIME
    // =====================================

    private EnemyHealth currentEnemyHealth;

    private bool enemyDefeated = true;
    private bool playerDefeated;

    private int enemyPoisonTurns;
    private int enemyBreakTurns;

    private int focusBonus;
    private int pendingDrawCards;

    // =====================================
    // EVENTS / PROPERTIES
    // =====================================

    public event Action<int> OnDrawCardsRequested;
    public event Action<int> OnFocusPrepared;

    public bool IsEnemyDefeated => enemyDefeated;
    public bool IsPlayerDefeated => playerDefeated;

    public int EnemyPoisonTurns => enemyPoisonTurns;
    public int EnemyBreakTurns => enemyBreakTurns;

    public int FocusBonus => focusBonus;

    // =====================================
    // INITIALIZATION
    // =====================================

    private void Start()
    {
        maxPlayerHP = Mathf.Max(1, maxPlayerHP);

        playerHP = Mathf.Clamp(
            playerHP,
            0,
            maxPlayerHP
        );

        playerShield = Mathf.Max(0, playerShield);
        enemyShield = Mathf.Max(0, enemyShield);

        UpdateUI();

        if (playerHP <= 0)
            CheckPlayerDead();
    }

    // =====================================
    // ENEMY MANAGEMENT
    // =====================================

    public void SetCurrentEnemy(EnemyHealth enemy)
    {
        UnsubscribeEnemy();

        currentEnemyHealth = enemy;

        enemyShield = 0;
        enemyPoisonTurns = 0;
        enemyBreakTurns = 0;

        enemyDefeated = enemy == null || enemy.IsDead;

        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.OnHealthChanged +=
                HandleEnemyHealthChanged;

            currentEnemyHealth.OnDied +=
                HandleEnemyDied;
        }

        UpdateUI();
    }

    private void UnsubscribeEnemy()
    {
        if (currentEnemyHealth == null)
            return;

        currentEnemyHealth.OnHealthChanged -=
            HandleEnemyHealthChanged;

        currentEnemyHealth.OnDied -=
            HandleEnemyDied;
    }

    private void HandleEnemyHealthChanged(EnemyHealth enemy)
    {
        if (enemy == currentEnemyHealth)
            UpdateUI();
    }

    private void HandleEnemyDied(EnemyHealth enemy)
    {
        if (enemy != currentEnemyHealth || enemyDefeated)
            return;

        enemyDefeated = true;

        enemyPoisonTurns = 0;
        enemyBreakTurns = 0;

        UpdateUI();

        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.CheckingBattle
            );
        }

        // RoundManager tự ghi Kill Count.
        if (roundManager != null)
            roundManager.EnemyDefeated();
        else
            Debug.LogError("Chưa gán RoundManager!");
    }

    private bool CanDamageEnemy()
    {
        return !playerDefeated &&
               !enemyDefeated &&
               currentEnemyHealth != null &&
               !currentEnemyHealth.IsDead;
    }

    // =====================================
    // EXECUTE CARD
    // =====================================

    public void ExecuteCardEffect(CardData card)
    {
        if (card == null || playerDefeated)
            return;

        int effect = (int)card.effectType;

        bool isAttackCard =
            effect == DAMAGE ||
            effect == POISON ||
            effect == MULTI_HIT;

        if (isAttackCard && !CanDamageEnemy())
            return;

        // Giữ tương thích với hệ thống Focus cũ.
        if (effect != FOCUS)
            focusBonus = 0;

        // =================================
        // PLAY SKILL VFX
        // =================================

        // VFX tấn công được tạo trước khi gây Damage.
        // Tránh mất Enemy Transform khi Enemy chết.

        // Riêng Shield sẽ tạo VFX sau khi cộng giáp.
        if (cardSkillVFX != null && effect != SHIELD)
        {
            cardSkillVFX.PlayCardVFX(card);
        }

        // =================================
        // APPLY CARD EFFECT
        // =================================

        switch (effect)
        {
            case DAMAGE:
                ApplyDamage(card);
                break;

            case SHIELD:
                ApplyShield(card);
                break;

            case HEAL:
                ApplyHeal(card);
                break;

            case POISON:
                ApplyPoison(card);
                break;

            case FOCUS:
                ApplyFocus(card);
                break;

            case MULTI_HIT:
                ApplyMultiHit(card);
                break;

            case DRAW:
                ApplyChaosDraw(card);
                break;

            default:
                Debug.LogWarning(
                    "Card Effect chưa được hỗ trợ: " + effect
                );
                break;
        }
    }

    // =====================================
    // NORMAL DAMAGE / HEAVY HIT
    // =====================================

    public void ApplyDamage(CardData card)
    {
        if (card == null || !CanDamageEnemy())
            return;

        DealEnemyDamage(
            Mathf.Max(0, card.baseDamage)
        );
    }

    // =====================================
    // DOUBLE STRIKE
    // =====================================

    private void ApplyMultiHit(CardData card)
    {
        if (card == null || !CanDamageEnemy())
            return;

        int hitCount = Mathf.Max(1, card.extraValue);

        for (int i = 0; i < hitCount; i++)
        {
            if (!CanDamageEnemy())
                break;

            DealEnemyDamage(
                Mathf.Max(0, card.baseDamage)
            );
        }
    }

    // =====================================
    // POISON DART
    // =====================================

    private void ApplyPoison(CardData card)
    {
        if (card == null || !CanDamageEnemy())
            return;

        DealEnemyDamage(
            Mathf.Max(0, card.baseDamage)
        );

        if (!CanDamageEnemy())
            return;

        if ((int)card.statusEffect == POISON_STATUS)
        {
            enemyPoisonTurns = Mathf.Max(
                enemyPoisonTurns,
                card.statusDuration
            );
        }
    }

    // Poison gây Damage đầu Enemy Turn.
    public void ProcessEnemyTurnStatuses()
    {
        if (!CanDamageEnemy() || enemyPoisonTurns <= 0)
            return;

        enemyPoisonTurns--;

        // Poison bỏ qua Shield.
        DamageEnemyHP(poisonDamagePerTurn);
    }

    // =====================================
    // BREAK
    // =====================================

    private void ApplyBreak(int duration)
    {
        if (!CanDamageEnemy())
            return;

        enemyShield = 0;

        enemyBreakTurns = Mathf.Max(
            enemyBreakTurns,
            duration
        );

        UpdateUI();
    }

    public bool TryConsumeEnemyBreak()
    {
        if (enemyDefeated || enemyBreakTurns <= 0)
            return false;

        enemyBreakTurns--;

        return true;
    }

    // =====================================
    // PLAYER SHIELD
    // =====================================

    public void ApplyShield(CardData card)
    {
        if (card == null || playerDefeated)
            return;

        int amount = Mathf.Max(0, card.shieldValue);

        if (amount <= 0)
            return;

        // Shield được cộng dồn.
        // Không còn cơ chế Shield tạm thời.
        playerShield += amount;

        UpdateUI();

        // Tạo VFX sau khi đã cộng chỉ số Shield.
        // CardSkillVFX sẽ tránh tạo VFX trùng.
        if (cardSkillVFX != null)
        {
            cardSkillVFX.PlayCardVFX(card);
        }
    }

    // =====================================
    // PLAYER HEAL
    // =====================================

    public void ApplyHeal(CardData card)
    {
        if (card == null || playerDefeated)
            return;

        playerHP = Mathf.Min(
            maxPlayerHP,
            playerHP + Mathf.Max(0, card.healValue)
        );

        UpdateUI();
    }

    // =====================================
    // ENEMY SHIELD
    // =====================================

    public void ApplyEnemyShield(int amount)
    {
        if (!CanDamageEnemy() || amount <= 0)
            return;

        enemyShield += amount;

        UpdateUI();
    }

    // =====================================
    // PLAYER TURN START
    // =====================================

    public void OnPlayerTurnStarted()
    {
        // Shield không bị reset khi sang lượt mới.
        // Giữ hàm để tương thích Battle System cũ.

        UpdateUI();
    }

    // =====================================
    // FOCUS - LEGACY COMPATIBILITY
    // =====================================

    private void ApplyFocus(CardData card)
    {
        focusBonus = Mathf.Max(0, card.extraValue);

        OnFocusPrepared?.Invoke(focusBonus);
    }

    public int PeekFocusBonusPercent()
    {
        return focusBonus;
    }

    // =====================================
    // CHAOS DRAW
    // =====================================

    private void ApplyChaosDraw(CardData card)
    {
        pendingDrawCards += Mathf.Max(
            0,
            card.extraValue
        );

        float chance = UnityEngine.Random.Range(
            0f,
            100f
        );

        if (chance < negativeEffectChance)
        {
            TakeStatusDamage(
                Mathf.Max(0, negativeEffectDamage)
            );
        }
    }

    public void FlushPendingCardDraws()
    {
        if (pendingDrawCards <= 0)
            return;

        int amount = pendingDrawCards;

        pendingDrawCards = 0;

        if (OnDrawCardsRequested != null)
        {
            OnDrawCardsRequested.Invoke(amount);
        }
        else
        {
            Debug.LogWarning(
                "Chưa kết nối DeckManager để rút Card!"
            );
        }
    }

    // =====================================
    // DAMAGE ENEMY
    // =====================================

    private void DealEnemyDamage(int damage)
    {
        if (!CanDamageEnemy() || damage <= 0)
            return;

        // Shield nhận Damage trước.
        int blocked = Mathf.Min(damage, enemyShield);

        enemyShield -= blocked;

        int hpDamage = damage - blocked;

        if (hpDamage > 0)
            DamageEnemyHP(hpDamage);

        UpdateUI();
    }

    // Dùng chung cho Damage và Poison.
    private void DamageEnemyHP(int damage)
    {
        if (!CanDamageEnemy() || damage <= 0)
            return;

        // Thống kê lượng HP thực tế bị trừ.
        int actualDamage = Mathf.Min(
            damage,
            Mathf.Max(0, currentEnemyHealth.CurrentHP)
        );

        // Ghi trước TakeDamage vì Enemy có thể chết.
        if (battleStats != null)
        {
            battleStats.AddDamageDealt(actualDamage);
        }

        currentEnemyHealth.TakeDamage(damage);

        UpdateUI();
    }

    // =====================================
    // DAMAGE PLAYER
    // =====================================

    public void TakeDamage(int damage)
    {
        if (damage <= 0 ||
            playerDefeated ||
            enemyDefeated)
        {
            return;
        }

        // =================================
        // SHIELD ABSORB DAMAGE
        // =================================

        int blocked = Mathf.Min(
            damage,
            playerShield
        );

        playerShield -= blocked;

        // Khi Shield bị đánh hết thì hủy VFX.
        if (playerShield <= 0)
        {
            playerShield = 0;

            if (cardSkillVFX != null)
            {
                cardSkillVFX.RemoveShieldVFX();
            }
        }

        UpdateUI();

        // =================================
        // REMAINING DAMAGE TO HP
        // =================================

        int remainingDamage = damage - blocked;

        if (remainingDamage > 0)
        {
            DamagePlayerHP(remainingDamage);
        }
    }

    // Poison / Burn bỏ qua Shield.
    public void TakeStatusDamage(int damage)
    {
        DamagePlayerHP(damage);
    }

    // Dùng chung cho Enemy Attack và Status.
    private void DamagePlayerHP(int damage)
    {
        if (damage <= 0 || playerDefeated)
            return;

        int actualDamage = Mathf.Min(
            damage,
            playerHP
        );

        playerHP = Mathf.Max(
            0,
            playerHP - damage
        );

        // Ghi Damage Received.
        if (battleStats != null)
        {
            battleStats.AddDamageReceived(actualDamage);
        }

        UpdateUI();

        if (playerHP <= 0)
        {
            CheckPlayerDead();
        }
        else if (actualDamage > 0)
        {
            playerAnimation?.PlayHurt();
        }
    }

    // =====================================
    // PLAYER DEATH
    // =====================================

    private void CheckPlayerDead()
    {
        if (playerHP > 0 || playerDefeated)
            return;

        playerDefeated = true;

        // Xóa Shield khi Player chết.
        playerShield = 0;

        // Hủy Shield VFX.
        if (cardSkillVFX != null)
        {
            cardSkillVFX.RemoveShieldVFX();
        }

        UpdateUI();

        playerAnimation?.PlayDeath();

        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.Defeat
            );
        }

        StartCoroutine(PlayerDeathCoroutine());
    }

    private IEnumerator PlayerDeathCoroutine()
    {
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, deathAnimationDuration)
        );

        if (roundManager != null)
        {
            roundManager.PlayerDefeated();
        }
    }

    // =====================================
    // UI
    // =====================================

    private void UpdateUI()
    {
        if (playerHPBar != null)
        {
            playerHPBar.SetHP(
                playerHP,
                maxPlayerHP
            );
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

    // =====================================
    // GETTERS / COMPATIBILITY
    // =====================================

    public int GetPlayerHP() => playerHP;

    public int GetMaxPlayerHP() => maxPlayerHP;

    public int GetPlayerShield() => playerShield;

    public int GetEnemyHP()
    {
        return currentEnemyHealth != null
            ? currentEnemyHealth.CurrentHP
            : 0;
    }

    public int GetEnemyShield() => enemyShield;

    // Giữ để script cũ không bị lỗi.
    public void NextRoundEnemyHP() { }

    // =====================================
    // CLEANUP
    // =====================================

    private void OnDestroy()
    {
        UnsubscribeEnemy();
    }
}