
using System;
using System.Collections;
using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    // =========================================
    // CARD EFFECT TYPES
    // =========================================

    private const int DAMAGE = 0;
    private const int SHIELD = 1;
    private const int HEAL = 2;
    private const int POISON = 3;
    private const int FOCUS = 4;
    private const int MULTI_HIT = 5;
    private const int DRAW = 6;

    private const int POISON_STATUS = 1;
    private const int BREAK_STATUS = 2;

    // =========================================
    // OPTIONAL TIMING
    // =========================================

    public enum TimingGrade
    {
        None,
        Miss,
        Good,
        Great,
        Perfect
    }

    // =========================================
    // PLAYER HP
    // =========================================

    [Header("Player HP")]
    [SerializeField] private int playerHP = 100;
    [SerializeField] private int maxPlayerHP = 100;

    [Header("Player Shield")]
    [SerializeField] private int playerShield = 0;

    [Header("Enemy Shield")]
    [SerializeField] private int enemyShield = 0;

    // =========================================
    // MANAGERS
    // =========================================

    [Header("Managers")]
    [SerializeField] private RoundManager roundManager;

    [SerializeField]
    private BattleStateMachine battleStateMachine;

    // =========================================
    // BATTLE STATISTICS - NEW
    // =========================================

    [Header("Battle Statistics")]
    [SerializeField]
    private BattleResultTracker battleStats;

    // =========================================
    // HP UI
    // =========================================

    [Header("Player HP UI")]
    [SerializeField] private PlayerHPBar playerHPBar;

    [Header("Enemy HP UI")]
    [SerializeField] private EnemyHPBar enemyHPBar;

    // =========================================
    // PLAYER ANIMATION
    // =========================================

    [Header("Player Animation")]
    [SerializeField]
    private PlayerAnimationController playerAnimation;

    [Header("Player Death")]
    [SerializeField]
    private float deathAnimationDuration = 1f;

    // =========================================
    // CARD SETTINGS
    // =========================================

    [Header("Slash Settings")]
    [SerializeField] private int slashGoodBonus = 2;

    [Header("Poison Settings")]
    [SerializeField] private int poisonDamagePerTurn = 2;

    [Header("Chaos Draw Settings")]
    [Range(0f, 100f)]
    [SerializeField]
    private float negativeEffectChance = 30f;

    [SerializeField]
    private int negativeEffectDamage = 3;

    // =========================================
    // RUNTIME DATA
    // =========================================

    private EnemyHealth currentEnemyHealth;

    private bool enemyDefeated = true;
    private bool playerDefeated = false;

    // Shield của Guard.
    private int temporaryGuardShield = 0;

    // Status của Enemy.
    private int enemyPoisonTurns = 0;
    private int enemyBreakTurns = 0;

    // Focus.
    private int focusBonus = 0;

    // Chaos Draw.
    private int pendingDrawCards = 0;

    // Timing.
    private TimingGrade firstTiming = TimingGrade.None;
    private TimingGrade secondTiming = TimingGrade.None;

    // =========================================
    // EVENTS
    // =========================================

    public event Action<int> OnDrawCardsRequested;
    public event Action<int> OnFocusPrepared;

    // =========================================
    // PROPERTIES
    // =========================================

    public bool IsEnemyDefeated => enemyDefeated;
    public bool IsPlayerDefeated => playerDefeated;

    public int EnemyPoisonTurns => enemyPoisonTurns;
    public int EnemyBreakTurns => enemyBreakTurns;
    public int FocusBonus => focusBonus;

    // =========================================
    // START
    // =========================================

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
        UnsubscribeEnemy();

        // Gán Enemy mới.
        currentEnemyHealth = enemy;

        // Reset trạng thái khi sang Round mới.
        enemyShield = 0;
        enemyPoisonTurns = 0;
        enemyBreakTurns = 0;

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

    private void UnsubscribeEnemy()
    {
        if (currentEnemyHealth == null)
            return;

        currentEnemyHealth.OnHealthChanged -=
            HandleEnemyHealthChanged;

        currentEnemyHealth.OnDied -=
            HandleEnemyDied;
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

        enemyPoisonTurns = 0;
        enemyBreakTurns = 0;

        UpdateUI();

        Debug.Log(
            "CardEffectManager: Enemy đã bị hạ!"
        );

        // RoundManager sẽ ghi KillCount,
        // tránh ghi trùng khi OnDied gọi nhiều lần.

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
    // EXECUTE CARD EFFECT
    // =========================================

    public void ExecuteCardEffect(CardData card)
    {
        if (card == null || playerDefeated)
            return;

        int effectType = (int)card.effectType;

        // Những Card tấn công cần Enemy sống.
        if (effectType == DAMAGE ||
            effectType == POISON ||
            effectType == MULTI_HIT)
        {
            if (!CanDamageEnemy())
            {
                Debug.LogWarning(
                    "CardEffectManager: Không có Enemy hợp lệ!"
                );

                return;
            }
        }

        // Lấy Timing của Card.
        TimingGrade timing1 = firstTiming;
        TimingGrade timing2 = secondTiming;

        firstTiming = TimingGrade.None;
        secondTiming = TimingGrade.None;

        // Focus chỉ áp dụng cho Card tiếp theo.
        if (effectType != FOCUS)
        {
            focusBonus = 0;
        }

        Debug.Log(
            "CardEffectManager: Sử dụng " +
            card.cardName
        );

        switch (effectType)
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

            default:
                Debug.LogWarning(
                    "CardEffectManager: Chưa hỗ trợ Effect: " +
                    effectType
                );
                break;
        }
    }

    // =========================================
    // CHECK ENEMY
    // =========================================

    private bool CanDamageEnemy()
    {
        return
            !playerDefeated &&
            !enemyDefeated &&
            currentEnemyHealth != null &&
            !currentEnemyHealth.IsDead;
    }

    // =========================================
    // DAMAGE CARD: SLASH / HEAVY HIT
    // =========================================

    public void ApplyDamage(CardData card)
    {
        ApplyDamageWithTiming(
            card,
            TimingGrade.None
        );
    }

    private void ApplyDamageWithTiming(
        CardData card,
        TimingGrade timing)
    {
        if (card == null || !CanDamageEnemy())
            return;

        int damage = Mathf.Max(
            0,
            card.baseDamage
        );

        // SLASH: GOOD trở lên cộng Damage.
        if (card.cardID == "slash" &&
            IsGoodOrBetter(timing))
        {
            damage += slashGoodBonus;
        }

        // Áp dụng Timing.
        damage = ApplyTimingMultiplier(
            damage,
            timing
        );

        if (damage <= 0)
            return;

        playerAnimation?.PlayAttack();

        // Hàm này tự ghi Damage Dealt.
        DealEnemyDamage(damage);

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

    // =========================================
    // DOUBLE STRIKE
    // =========================================

    private void ApplyMultiHit(
        CardData card,
        TimingGrade timing1,
        TimingGrade timing2)
    {
        if (card == null || !CanDamageEnemy())
            return;

        int hitCount = Mathf.Max(
            1,
            card.extraValue
        );

        int damagePerHit = Mathf.Max(
            0,
            card.baseDamage
        );

        playerAnimation?.PlayAttack();

        for (int i = 0; i < hitCount; i++)
        {
            if (!CanDamageEnemy())
                break;

            TimingGrade timing = TimingGrade.None;

            if (i == 0)
                timing = timing1;
            else if (i == 1)
                timing = timing2;

            int finalDamage = ApplyTimingMultiplier(
                damagePerHit,
                timing
            );

            // Mỗi Hit ghi Damage riêng.
            DealEnemyDamage(finalDamage);

            Debug.Log(
                $"DOUBLE STRIKE | Hit {i + 1}/{hitCount}" +
                $" | Damage: {finalDamage}"
            );
        }
    }

    // =========================================
    // POISON DART
    // =========================================

    private void ApplyPoison(
        CardData card,
        TimingGrade timing)
    {
        if (card == null || !CanDamageEnemy())
            return;

        int damage = ApplyTimingMultiplier(
            Mathf.Max(0, card.baseDamage),
            timing
        );

        playerAnimation?.PlayAttack();

        // Damage ban đầu được ghi thống kê.
        DealEnemyDamage(damage);

        if (!CanDamageEnemy())
            return;

        if ((int)card.statusEffect != POISON_STATUS)
            return;

        int duration = Mathf.Max(
            0,
            card.statusDuration
        );

        enemyPoisonTurns = Mathf.Max(
            enemyPoisonTurns,
            duration
        );

        Debug.Log(
            $"POISON DART: Enemy bị Poison " +
            $"{enemyPoisonTurns} lượt!"
        );
    }

    // =========================================
    // ENEMY POISON PER TURN
    // =========================================

    public void ProcessEnemyTurnStatuses()
    {
        if (!CanDamageEnemy())
            return;

        if (enemyPoisonTurns <= 0)
            return;

        enemyPoisonTurns--;

        int damage = Mathf.Max(
            0,
            poisonDamagePerTurn
        );

        if (damage <= 0)
            return;

        // =====================================
        // BATTLE STATS: POISON DAMAGE
        // =====================================

        // Damage thực tế không vượt quá HP Enemy.
        int actualDamage = Mathf.Min(
            damage,
            Mathf.Max(0, currentEnemyHealth.CurrentHP)
        );

        // Ghi trước TakeDamage để bảo đảm
        // thống kê cập nhật trước sự kiện OnDied.
        if (battleStats != null)
        {
            battleStats.AddDamageDealt(actualDamage);
        }

        // Poison bỏ qua Enemy Shield.
        currentEnemyHealth.TakeDamage(damage);

        UpdateUI();

        Debug.Log(
            $"Enemy nhận {actualDamage} Poison Damage." +
            $" Còn {enemyPoisonTurns} lượt."
        );
    }

    // =========================================
    // HEAVY HIT: BREAK
    // =========================================

    private void ApplyBreak(int duration)
    {
        if (!CanDamageEnemy())
            return;

        // Break phá Shield Enemy.
        enemyShield = 0;

        enemyBreakTurns = Mathf.Max(
            enemyBreakTurns,
            duration
        );

        UpdateUI();

        Debug.Log(
            $"HEAVY HIT: Enemy bị Break " +
            $"{enemyBreakTurns} lượt!"
        );
    }

    public bool TryConsumeEnemyBreak()
    {
        if (enemyDefeated ||
            enemyBreakTurns <= 0)
        {
            return false;
        }

        enemyBreakTurns--;

        Debug.Log(
            "Enemy bị Break, bỏ qua hành động!"
        );

        return true;
    }

    // =========================================
    // GUARD: PLAYER SHIELD
    // =========================================

    public void ApplyShield(CardData card)
    {
        if (card == null || playerDefeated)
            return;

        int amount = Mathf.Max(
            0,
            card.shieldValue
        );

        playerShield += amount;
        temporaryGuardShield += amount;

        UpdateUI();

        Debug.Log(
            $"GUARD: Player nhận {amount} Shield." +
            $" Tổng Shield: {playerShield}"
        );
    }

    // =========================================
    // RESET GUARD AT PLAYER TURN START
    // =========================================

    public void OnPlayerTurnStarted()
    {
        if (temporaryGuardShield <= 0)
            return;

        playerShield = Mathf.Max(
            0,
            playerShield - temporaryGuardShield
        );

        temporaryGuardShield = 0;

        UpdateUI();

        Debug.Log(
            "GUARD: Shield còn lại đã hết hiệu lực."
        );
    }

    // =========================================
    // ENEMY SHIELD
    // =========================================

    public void ApplyEnemyShield(int shieldAmount)
    {
        if (shieldAmount <= 0 ||
            !CanDamageEnemy())
        {
            return;
        }

        enemyShield += shieldAmount;

        UpdateUI();

        Debug.Log(
            $"Enemy nhận {shieldAmount} Shield!"
        );
    }

    // =========================================
    // QUICK HEAL
    // =========================================

    public void ApplyHeal(CardData card)
    {
        if (card == null || playerDefeated)
            return;

        int amount = Mathf.Max(
            0,
            card.healValue
        );

        int oldHP = playerHP;

        playerHP = Mathf.Min(
            maxPlayerHP,
            playerHP + amount
        );

        int actualHeal = playerHP - oldHP;

        UpdateUI();

        Debug.Log(
            $"QUICK HEAL: Player hồi {actualHeal} HP."
        );
    }

    // =========================================
    // FOCUS
    // =========================================

    private void ApplyFocus(CardData card)
    {
        if (card == null || playerDefeated)
            return;

        focusBonus = Mathf.Max(
            0,
            card.extraValue
        );

        Debug.Log(
            $"FOCUS: Card kế tiếp nhận +" +
            $"{focusBonus} Timing Zone."
        );

        OnFocusPrepared?.Invoke(focusBonus);
    }

    public int PeekFocusBonusPercent()
    {
        return focusBonus;
    }

    // =========================================
    // CHAOS DRAW
    // =========================================

    private void ApplyChaosDraw(CardData card)
    {
        if (card == null || playerDefeated)
            return;

        int drawCount = Mathf.Max(
            0,
            card.extraValue
        );

        // Chờ CardManager bỏ Card đã dùng
        // ra khỏi Hand trước khi rút bài.
        pendingDrawCards += drawCount;

        Debug.Log(
            $"CHAOS DRAW: Yêu cầu rút {drawCount} Card."
        );

        float randomValue = UnityEngine.Random.Range(
            0f,
            100f
        );

        if (randomValue < negativeEffectChance)
        {
            Debug.Log(
                "CHAOS DRAW: Kích hoạt hiệu ứng bất lợi!"
            );

            // Damage này được ghi Damage Received
            // thông qua TakeStatusDamage.
            TakeStatusDamage(
                Mathf.Max(0, negativeEffectDamage)
            );
        }
    }

    // =========================================
    // DRAW CARD EVENT
    // =========================================

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
                "CardEffectManager: Chưa kết nối " +
                "DeckManager để rút Card!"
            );
        }
    }

    // =========================================
    // TIMING SUPPORT
    // =========================================

    public void SetNextCardTiming(
        TimingGrade first,
        TimingGrade second = TimingGrade.None)
    {
        firstTiming = first;
        secondTiming = second;
    }

    private bool IsGoodOrBetter(TimingGrade timing)
    {
        return
            timing == TimingGrade.Good ||
            timing == TimingGrade.Great ||
            timing == TimingGrade.Perfect;
    }

    private int ApplyTimingMultiplier(
        int damage,
        TimingGrade timing)
    {
        float multiplier = 1f;

        switch (timing)
        {
            case TimingGrade.Miss:
                multiplier = 0.5f;
                break;

            case TimingGrade.Good:
                multiplier = 1f;
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

    // =========================================
    // DEAL DAMAGE TO ENEMY
    // =========================================

    private void DealEnemyDamage(int damage)
    {
        if (damage <= 0 || !CanDamageEnemy())
            return;

        int originalDamage = damage;

        // =====================================
        // 1. ENEMY SHIELD
        // =====================================

        int damageToShield = Mathf.Min(
            damage,
            enemyShield
        );

        enemyShield -= damageToShield;

        int hpDamage = damage - damageToShield;

        // =====================================
        // 2. ENEMY HP + BATTLE STATS
        // =====================================

        if (hpDamage > 0 &&
            currentEnemyHealth != null)
        {
            int enemyHPBefore =
                Mathf.Max(0, currentEnemyHealth.CurrentHP);

            // Không tính Damage vượt quá HP.
            int actualDamage = Mathf.Min(
                hpDamage,
                enemyHPBefore
            );

            // =====================================
            // NEW: ADD DAMAGE DEALT
            // =====================================

            // Ghi trước TakeDamage vì TakeDamage
            // có thể gọi OnDied và kết thúc Round.
            if (battleStats != null)
            {
                battleStats.AddDamageDealt(actualDamage);
            }

            currentEnemyHealth.TakeDamage(hpDamage);
        }

        UpdateUI();

        Debug.Log(
            $"Card Damage: {originalDamage} | " +
            $"Shield: {damageToShield} | " +
            $"HP Damage: {hpDamage}"
        );
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
        // 1. PLAYER SHIELD
        // =====================================

        int damageToShield = Mathf.Min(
            damage,
            playerShield
        );

        playerShield -= damageToShield;

        // Guard Shield đã bị tiêu hao.
        int guardConsumed = Mathf.Min(
            damageToShield,
            temporaryGuardShield
        );

        temporaryGuardShield -= guardConsumed;

        int hpDamage = damage - damageToShield;

        // =====================================
        // 2. PLAYER HP
        // =====================================

        int oldHP = playerHP;

        if (hpDamage > 0)
        {
            playerHP = Mathf.Max(
                0,
                playerHP - hpDamage
            );
        }

        // Damage thực tế nhận vào HP.
        int actualDamage = oldHP - playerHP;

        // =====================================
        // NEW: ADD DAMAGE RECEIVED
        // =====================================

        if (battleStats != null)
        {
            battleStats.AddDamageReceived(actualDamage);
        }

        UpdateUI();

        Debug.Log(
            $"Enemy Damage: {originalDamage} | " +
            $"Shield: {damageToShield} | " +
            $"Damage Received: {actualDamage} | " +
            $"Player HP: {playerHP}/{maxPlayerHP}"
        );

        // =====================================
        // 3. PLAYER ANIMATION
        // =====================================

        if (playerHP <= 0)
        {
            CheckPlayerDead();
        }
        else if (actualDamage > 0)
        {
            playerAnimation?.PlayHurt();
        }
    }

    // =========================================
    // STATUS DAMAGE TO PLAYER
    // =========================================

    public void TakeStatusDamage(int damage)
    {
        if (damage <= 0 || playerDefeated)
            return;

        // =====================================
        // 1. PLAYER HP
        // =====================================

        int oldHP = playerHP;

        // Poison / Burn bỏ qua Shield.
        playerHP = Mathf.Max(
            0,
            playerHP - damage
        );

        int actualDamage = oldHP - playerHP;

        // =====================================
        // NEW: ADD DAMAGE RECEIVED
        // =====================================

        if (battleStats != null)
        {
            battleStats.AddDamageReceived(actualDamage);
        }

        UpdateUI();

        Debug.Log(
            $"Player nhận {actualDamage} Status Damage." +
            $" HP: {playerHP}/{maxPlayerHP}"
        );

        if (playerHP <= 0)
        {
            CheckPlayerDead();
        }
        else if (actualDamage > 0)
        {
            playerAnimation?.PlayHurt();
        }
    }

    // =========================================
    // PLAYER DEFEATED
    // =========================================

    private void CheckPlayerDead()
    {
        if (playerHP > 0 || playerDefeated)
            return;

        playerDefeated = true;

        Debug.Log(
            "CardEffectManager: Player đã bị hạ!"
        );

        playerAnimation?.PlayDeath();

        if (battleStateMachine != null)
        {
            battleStateMachine.ChangeState(
                BattleStateMachine.BattleState.Defeat
            );
        }

        // Chờ Death Animation trước khi hiện Result.
        StartCoroutine(
            PlayerDeathCoroutine()
        );
    }

    // =========================================
    // PLAYER DEATH COROUTINE
    // =========================================

    private IEnumerator PlayerDeathCoroutine()
    {
        float duration = Mathf.Max(
            0f,
            deathAnimationDuration
        );

        if (duration > 0f)
        {
            yield return new WaitForSecondsRealtime(
                duration
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
    // LEGACY METHOD
    // =========================================

    public void NextRoundEnemyHP()
    {
        // EnemyHP được EnemyHealth quản lý.
        // Giữ hàm để tương thích code cũ.
    }

    // =========================================
    // UPDATE UI
    // =========================================

    private void UpdateUI()
    {
        // Player HP Bar.
        if (playerHPBar != null)
        {
            playerHPBar.SetHP(
                playerHP,
                maxPlayerHP
            );
        }

        // Enemy HP Bar.
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
    // CLEANUP
    // =========================================

    private void OnDestroy()
    {
        UnsubscribeEnemy();
    }
}