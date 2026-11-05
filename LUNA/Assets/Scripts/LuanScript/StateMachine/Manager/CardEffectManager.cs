
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

    // effectType mới (Khớp thứ tự enum CardEffectType trong CardData)
    private const int APPLY_STATUS = 7;
    private const int DRAW_CARD = 8;
    private const int CLEANSE = 9;
    private const int POISON_STATUS = 1;
    private const int BREAK_STATUS = 2;

    public enum TimingGrade
    {
        None,
        Miss,
        Good,
        Great,
        Perfect
    }

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
    
    [Header("Ngũ hành & trạng thái (Tùy chọn, để trống = Hành vi cũ)")]
    [SerializeField] private CardCombatExtension combatExtension;
    // =====================================
    // CARD SETTINGS
    // =====================================

    [Header("Card Settings")]
    [SerializeField] private int slashGoodBonus = 2;
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

    private int temporaryGuardShield;
    private int enemyPoisonTurns;
    private int enemyBreakTurns;

    private int focusBonus;
    private int pendingDrawCards;

    // Thẻ đang xử lý, để tính tương khắc trong DealEnemyDamage
    private CardData activeCard;

    private TimingGrade firstTiming = TimingGrade.None;
    private TimingGrade secondTiming = TimingGrade.None;

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
        playerHP = Mathf.Clamp(playerHP, 0, maxPlayerHP);

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

        if (combatExtension != null)
            combatExtension.ClearAll();

        enemyDefeated = enemy == null || enemy.IsDead;

        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.OnHealthChanged += HandleEnemyHealthChanged;
            currentEnemyHealth.OnDied += HandleEnemyDied;
        }

        UpdateUI();
    }

    private void UnsubscribeEnemy()
    {
        if (currentEnemyHealth == null)
            return;

        currentEnemyHealth.OnHealthChanged -= HandleEnemyHealthChanged;
        currentEnemyHealth.OnDied -= HandleEnemyDied;
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
            effect == MULTI_HIT ||
            effect == APPLY_STATUS;

        if (isAttackCard && !CanDamageEnemy())
            return;

        // Lấy Timing cho lần sử dụng Card này.
        TimingGrade timing1 = firstTiming;
        TimingGrade timing2 = secondTiming;

        firstTiming = TimingGrade.None;
        secondTiming = TimingGrade.None;

        // Focus áp dụng cho Card tiếp theo.
        if (effect != FOCUS)
            focusBonus = 0;

        // =================================
        // PLAY SKILL VFX
        // =================================

        // Spawn VFX trước khi gây Damage.
        // Tránh mất Enemy Transform nếu Enemy
        // chết ngay sau khi nhận đòn.
        if (cardSkillVFX != null)
            cardSkillVFX.PlayCardVFX(card);

        // =================================
        // APPLY CARD EFFECT
        // =================================
        activeCard = card;

        switch (effect)
        {
            case DAMAGE:
                ApplyDamageWithTiming(card, timing1);
                break;

            case SHIELD:
                ApplyShield(card);
                break;

            case HEAL:
                ApplyHeal(card);
                break;

            case POISON:
                ApplyPoison(card, timing1);
                break;

            case FOCUS:
                ApplyFocus(card);
                break;

            case MULTI_HIT:
                ApplyMultiHit(card, timing1, timing2);
                break;

            case DRAW:
                ApplyChaosDraw(card);
                break;

            case APPLY_STATUS:
                ApplyStatusCard(card, timing1);
                break;

            case DRAW_CARD:
                pendingDrawCards += Mathf.Max(0, card.extraValue);
                break;

            case CLEANSE:
                Debug.LogWarning("Cleanse chưa hỗ trợ (Enemy chưa gắn trạng thái lên Player).");
                break;

            default:
                Debug.LogWarning(
                    "Card Effect chưa được hỗ trợ: " + effect
                );
                break;
        }
        activeCard = null;
    }

    // =====================================
    // NORMAL DAMAGE / HEAVY HIT
    // =====================================

    public void ApplyDamage(CardData card)
    {
        ApplyDamageWithTiming(card, TimingGrade.None);
    }

    private void ApplyDamageWithTiming(
        CardData card,
        TimingGrade timing)
    {
        if (card == null || !CanDamageEnemy())
            return;

        int damage = Mathf.Max(0, card.baseDamage);

        // SLASH: GOOD trở lên +2 Damage.
        if (card.cardID == "slash" &&
            IsGoodOrBetter(timing))
        {
            damage += slashGoodBonus;
        }

        DealEnemyDamage(
            CalculateTimingDamage(damage, timing)
        );

        // HEAVY HIT: PERFECT gây Break.
        if (card.cardID == "heavy_hit" &&
            timing == TimingGrade.Perfect &&
            (int)card.statusEffect == BREAK_STATUS &&
            CanDamageEnemy())
        {
            ApplyBreak(
                Mathf.Max(1, card.statusDuration)
            );
        }
    }

    // =====================================
    // DOUBLE STRIKE
    // =====================================

    private void ApplyMultiHit(
        CardData card,
        TimingGrade timing1,
        TimingGrade timing2)
    {
        if (card == null || !CanDamageEnemy())
            return;

        int hitCount = Mathf.Max(1, card.extraValue);

        for (int i = 0; i < hitCount; i++)
        {
            if (!CanDamageEnemy())
                break;

            TimingGrade timing = TimingGrade.None;

            if (i == 0)
                timing = timing1;
            else if (i == 1)
                timing = timing2;

            int damage = CalculateTimingDamage(
                card.baseDamage,
                timing
            );

            DealEnemyDamage(damage);
        }
    }

    // =====================================
    // POISON DART
    // =====================================

    private void ApplyPoison(
        CardData card,
        TimingGrade timing)
    {
        if (card == null || !CanDamageEnemy())
            return;

        int damage = CalculateTimingDamage(
            card.baseDamage,
            timing
        );

        DealEnemyDamage(damage);

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

    // =====================================
    // APPLY STATUS (Thẻ nhóm Effect)
    // =====================================

    private void ApplyStatusCard(CardData card, TimingGrade timing)
    {
        if (card == null || !CanDamageEnemy())
            return;

        // 1) Sát thương trước (có Timing và tương khắc)
        int damage = CalculateTimingDamage(
            Mathf.Max(0, card.baseDamage),
            timing
        );

        DealEnemyDamage(damage);

        // Enemy đã chết thì không gắn trạng thái nữa.
        if (!CanDamageEnemy())
            return;

        // 2) Rồi mới gắn trạng thái
        int duration = Mathf.Max(1, card.statusDuration);

        switch (card.statusEffect)
        {
            case StatusEffectType.Poison:
                enemyPoisonTurns = Mathf.Max(enemyPoisonTurns, duration);
                break;

            case StatusEffectType.Break:
                ApplyBreak(duration);
                break;

            case StatusEffectType.Burn:
            case StatusEffectType.Weak:
            case StatusEffectType.Stun:
                if (combatExtension != null)
                    combatExtension.ApplyStatus(card.statusEffect, duration);
                else
                    Debug.LogWarning("Chưa gán CombatExtension, không thể gắn " + card.statusEffect);
                break;
        }
    }

    // Poison gây Damage đầu Enemy Turn.
    public void ProcessEnemyTurnStatuses()
    {
        // Burn gây sát thương, bỏ qua Shield như Poison
        if (combatExtension != null)
        {
            int burn = combatExtension.GetBurnTickDamage();

            if (burn > 0)
                DamageEnemyHP(burn);
        }

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
    // PLAYER SHIELD / HEAL
    // =====================================

    public void ApplyShield(CardData card)
    {
        if (card == null || playerDefeated)
            return;

        int amount = Mathf.Max(0, card.shieldValue);

        playerShield += amount;
        temporaryGuardShield += amount;

        UpdateUI();
    }

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

    public void ApplyEnemyShield(int amount)
    {
        if (!CanDamageEnemy() || amount <= 0)
            return;

        enemyShield += amount;
        UpdateUI();
    }

    // Guard hết hạn ở đầu lượt Player kế tiếp.
    public void OnPlayerTurnStarted()
    {
        if (combatExtension != null)
            combatExtension.OnPlayerTurnStart();

        if (temporaryGuardShield <= 0)
            return;

        playerShield = Mathf.Max(
            0,
            playerShield - temporaryGuardShield
        );

        temporaryGuardShield = 0;
        UpdateUI();
    }

    // =====================================
    // FOCUS
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
        pendingDrawCards += Mathf.Max(0, card.extraValue);

        float chance = UnityEngine.Random.Range(0f, 100f);

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
            OnDrawCardsRequested.Invoke(amount);
        else
            Debug.LogWarning(
                "Chưa kết nối DeckManager để rút Card!"
            );
    }

    // =====================================
    // TIMING
    // =====================================

    public void SetNextCardTiming(
        TimingGrade first,
        TimingGrade second = TimingGrade.None)
    {
        firstTiming = first;
        secondTiming = second;
    }

    private bool IsGoodOrBetter(TimingGrade timing)
    {
        return timing == TimingGrade.Good ||
               timing == TimingGrade.Great ||
               timing == TimingGrade.Perfect;
    }

    private int CalculateTimingDamage(
        int damage,
        TimingGrade timing)
    {
        float multiplier = 1f;

        switch (timing)
        {
            case TimingGrade.Miss:
                multiplier = 0.5f;
                break;

            case TimingGrade.Great:
                multiplier = 1.25f;
                break;

            case TimingGrade.Perfect:
                multiplier = 1.5f;
                break;
        }

        return Mathf.Max(
            0,
            Mathf.RoundToInt(damage * multiplier)
        );
    }

    // =====================================
    // DAMAGE ENEMY
    // =====================================

    private void DealEnemyDamage(int damage)
    {
        if (!CanDamageEnemy() || damage <= 0)
            return;
        
        if (combatExtension != null && activeCard != null)
            damage = combatExtension.ModifyCardDamage(activeCard, damage);

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
            battleStats.AddDamageDealt(actualDamage);

        currentEnemyHealth.TakeDamage(damage);
        UpdateUI();
    }

    // =====================================
    // DAMAGE PLAYER
    // =====================================

    public void TakeDamage(int damage)
    {
        if (combatExtension != null)
            damage = combatExtension.ModifyOutgoingDamage(damage);
        
        if (damage <= 0 ||
            playerDefeated ||
            enemyDefeated)
        {
            return;
        }

        // Shield nhận Damage trước.
        int blocked = Mathf.Min(damage, playerShield);

        playerShield -= blocked;

        temporaryGuardShield = Mathf.Max(
            0,
            temporaryGuardShield - blocked
        );

        UpdateUI();

        DamagePlayerHP(damage - blocked);
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

        playerHP = Mathf.Max(0, playerHP - damage);

        // Ghi Damage Received.
        if (battleStats != null)
            battleStats.AddDamageReceived(actualDamage);

        UpdateUI();

        if (playerHP <= 0)
            CheckPlayerDead();
        else if (actualDamage > 0)
            playerAnimation?.PlayHurt();
    }

    // =====================================
    // PLAYER DEATH
    // =====================================

    private void CheckPlayerDead()
    {
        if (playerHP > 0 || playerDefeated)
            return;

        playerDefeated = true;

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
            roundManager.PlayerDefeated();
    }

    // =====================================
    // UI
    // =====================================

    private void UpdateUI()
    {
        if (playerHPBar != null)
            playerHPBar.SetHP(playerHP, maxPlayerHP);

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

    private void OnDestroy()
    {
        UnsubscribeEnemy();
    }
}