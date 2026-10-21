using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    [Header("Tham chiếu")]
    public BattleManager battleManager;
    public DeckManager deckManager;
    public TurnManager turnManager;

    [Header("Focus - Card kế tiếp có Timing sẽ được mở rộng (CardManager đọc rồi reset về 0)")]
    public float pendingTimingWindowBonusPercent;

    [Header("Timing - Hệ số Damage khi MISS (= Hiệu quả thấp hơn bình thường)")]
    [Range(0f, 1f)]
    public float missDamageMultiplier = 0.5f;

    public void ResolveCard(CardData card, TimingResult result)
    {
        ResolveCard(card, new TimingResult[] { result });
    }

    public void ResolveCard(CardData card, TimingResult[] results)
    {
        if (card == null)
        {
            Debug.LogWarning("CardEffectManager.ResolveCard: CardData null.");
            return;
        }

        if (results == null || results.Length == 0)
        {
            results = new TimingResult[] { TimingResult.Good };
        }

        TimingResult firstResult = results[0];

        switch (card.effectType)
        {
            case CardEffectType.Damage:
                ResolveDamage(card, firstResult);
                break;

            case CardEffectType.Shield:
                ResolveShield(card);
                break;

            case CardEffectType.Heal:
                ResolveHeal(card);
                break;

            case CardEffectType.Poison:
                ResolvePoison(card, firstResult);
                break;

            case CardEffectType.FocusTiming:
                ResolveFocusTiming(card);
                break;

            case CardEffectType.DoubleHit:
                ResolveDoubleHit(card, results);
                break;

            case CardEffectType.ChaosDraw:
                ResolveChaosDraw(card);
                break;

            default:
                Debug.LogWarning("CardEffectManager: Chưa hỗ trợ effectType " + card.effectType);
                break;
        }
    }

    private int CalculateDamage(int baseDamage, int goodBonus, TimingResult result)
    {
        if (result == TimingResult.Miss)
        {
            int reduced = Mathf.CeilToInt(baseDamage * missDamageMultiplier);
            return baseDamage > 0 ? Mathf.Max(1, reduced) : 0;
        }

        return baseDamage + goodBonus;
    }

    private void ResolveDamage(CardData card, TimingResult result)
    {
        int damage = CalculateDamage(card.baseDamage, 2, result);

        battleManager.DamageEnemy(damage);

        if (card.cardID == "heavy_hit" && result == TimingResult.Perfect)
        {
            battleManager.BreakEnemy();
            Debug.Log("CardEffectManager: Heavy Hit PERFECT -> Enemy bị Break.");
        }

        Debug.Log("CardEffectManager: " + card.cardName + " gây " + damage + " Damage (Timing: " + result + ").");
    }

    private void ResolveShield(CardData card)
    {
        battleManager.ShieldPlayer(card.shieldValue);
        Debug.Log("CardEffectManager: " + card.cardName + " tạo " + card.shieldValue + " Shield.");
    }

    private void ResolveHeal(CardData card)
    {
        battleManager.HealPlayer(card.healValue);
        Debug.Log("CardEffectManager: " + card.cardName + " hồi " + card.healValue + " HP.");
    }

    private void ResolvePoison(CardData card, TimingResult result)
    {
        int damage = CalculateDamage(card.baseDamage, 0, result);

        battleManager.DamageEnemy(damage);
        battleManager.PoisonEnemy(1, card.statusDuration);

        Debug.Log("CardEffectManager: " + card.cardName + " gây " + damage +
                   " Damage (Timing: " + result + ") và áp dụng Poison trong " + card.statusDuration + " lượt.");
    }

    private void ResolveFocusTiming(CardData card)
    {
        pendingTimingWindowBonusPercent = card.extraValue;
        Debug.Log("CardEffectManager: Focus kích hoạt - Card kế tiếp có Timing sẽ được mở rộng " +
                   pendingTimingWindowBonusPercent + "%.");
    }

    private void ResolveDoubleHit(CardData card, TimingResult[] results)
    {
        int hitCount = Mathf.Max(1, card.extraValue);

        for (int i = 0; i < hitCount; i++)
        {
            TimingResult hitResult = results[Mathf.Min(i, results.Length - 1)];
            int damage = CalculateDamage(card.baseDamage, 1, hitResult);

            battleManager.DamageEnemy(damage);
            Debug.Log("CardEffectManager: " + card.cardName + " - Hit " + (i + 1) + "/" + hitCount +
                       " gây " + damage + " Damage (Timing: " + hitResult + ").");
        }
    }

    private void ResolveChaosDraw(CardData card)
    {
        int drawCount = Mathf.Max(1, card.extraValue);

        for (int i = 0; i < drawCount; i++)
        {
            deckManager.DrawCard();
        }

        Debug.Log("CardEffectManager: Chaos Draw - Đã rút thêm " + drawCount + " Card.");

        float negativeChance = 0.3f;
        if (Random.value < negativeChance)
        {
            turnManager.SpendEnergy(Mathf.Min(1, turnManager.currentEnergy));
            Debug.Log("CardEffectManager: Chaos Draw kích hoạt hiệu ứng bất lợi - Mất thêm 1 Energy!");
        }
    }
}