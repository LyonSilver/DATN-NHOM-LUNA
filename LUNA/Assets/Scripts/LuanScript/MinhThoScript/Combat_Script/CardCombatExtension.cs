using UnityEngine;

public class CardCombatExtension : MonoBehaviour
{
    [Header("Tham chiếu")]
    public _EnemyCombatState enemyCombat;

    [Header("Tương khắc Ngũ hành (thẻ có phương hệ)")]
    public float advantageMultiplier = 1.5f;
    public float disadvantageMultiplier = 0.75f;

    [Header("Chỉ để test")]
    public CardData testCard;
    public CardEffectManager testEffectManager;

    // Hook cho VFX/HUD: (quan hệ tương khắc, sát thương sau khi nhân)
    public event System.Action<ElementRelation, int> OnElementalHit;

    // =====================================================
    // CardEffectManager GỌI CÁC HÀM DƯỚI ĐÂY
    // =====================================================

    public int ModifyCardDamage(CardData card, int damage)
    {
        if (card == null || damage <= 0 || card.element == CardElement.None || enemyCombat == null)
        {
            return damage;
        }

        ElementRelation relation = ElementRules.GetRelation(card.element, enemyCombat.Element);
        float multiplier = 1f;

        if (relation == ElementRelation.Advantage)
        {
            multiplier = advantageMultiplier;
        }
        else if (relation == ElementRelation.Disadvantage)
        {
            multiplier = disadvantageMultiplier;
        }

        int final = Mathf.Approximately(multiplier, 1f)
            ? damage
            : Mathf.Max(1, Mathf.RoundToInt(damage * multiplier));

        Debug.Log("CardCombatExtension: " + card.cardName + " (" + ElementRules.GetNameVN(card.element) +
                  ") vs Enemy " + ElementRules.GetNameVN(enemyCombat.Element) +
                  ": " + damage + " -> " + final + " | " + relation);

        OnElementalHit?.Invoke(relation, final);
        return final;
    }

    public void ApplyStatus(StatusEffectType status, int turns)
    {
        if (enemyCombat != null)
        {
            enemyCombat.ApplyStatus(status, turns);
        }
    }

    public int GetBurnTickDamage()
    {
        return enemyCombat != null ? enemyCombat.GetBurnTickDamage() : 0;
    }

    public int ModifyOutgoingDamage(int damage)
    {
        return enemyCombat != null ? enemyCombat.ModifyOutgoingDamage(damage) : damage;
    }

    public void OnPlayerTurnStart()
    {
        if (enemyCombat != null)
        {
            enemyCombat.OnPlayerTurnStart();
        }
    }

    public void ClearAll()
    {
        if (enemyCombat != null)
        {
            enemyCombat.ClearAll();
        }
    }

    // Đồng đội gọi ở đầu lượt Enemy.
    public bool ShouldSkipTurn()
    {
        return enemyCombat != null && enemyCombat.ShouldSkipTurn();
    }

    // =====================================================
    // TEST (cần đang trong trận có Enemy thật)
    // =====================================================

    [ContextMenu("TEST - Bảng hệ số vs Enemy hiện tại")]
    private void TestMultiplierTable()
    {
        if (enemyCombat == null)
        {
            Debug.LogWarning("CardCombatExtension TEST: Chưa gán EnemyCombatState.");
            return;
        }

        foreach (CardElement e in ElementRules.AllElements)
        {
            ElementRelation relation = ElementRules.GetRelation(e, enemyCombat.Element);
            float multiplier = relation == ElementRelation.Advantage ? advantageMultiplier
                             : relation == ElementRelation.Disadvantage ? disadvantageMultiplier
                             : 1f;

            Debug.Log("Thẻ hệ " + ElementRules.GetNameVN(e) + " đánh Enemy hệ " +
                      ElementRules.GetNameVN(enemyCombat.Element) + " -> " + relation + " (x" + multiplier + ")");
        }
    }

    [ContextMenu("TEST - Chơi testCard (ExecuteCardEffect)")]
    private void TestPlayCard()
    {
        if (testCard == null || testEffectManager == null)
        {
            Debug.LogWarning("CardCombatExtension TEST: Cần gán Test Card và Test Effect Manager.");
            return;
        }

        testEffectManager.ExecuteCardEffect(testCard);
        testEffectManager.FlushPendingCardDraws();
    }

    [ContextMenu("TEST - Giả lập đầu lượt Enemy (Burn/Poison gây sát thương)")]
    private void TestEnemyTurnStart()
    {
        if (testEffectManager == null)
        {
            Debug.LogWarning("CardCombatExtension TEST: Chưa gán Test Effect Manager.");
            return;
        }

        testEffectManager.ProcessEnemyTurnStatuses();
        Debug.Log("CardCombatExtension TEST: Enemy bị Stun (bỏ lượt)? " + ShouldSkipTurn());
    }

    [ContextMenu("TEST - Giả lập đầu lượt Player (trừ 1 lượt trạng thái)")]
    private void TestPlayerTurnStart()
    {
        if (testEffectManager == null)
        {
            Debug.LogWarning("CardCombatExtension TEST: Chưa gán Test Effect Manager.");
            return;
        }

        testEffectManager.OnPlayerTurnStarted();
    }

    [ContextMenu("TEST - Thử Weak với đòn Enemy 10")]
    private void TestWeak()
    {
        Debug.Log("CardCombatExtension TEST: Đòn Enemy 10 -> " + ModifyOutgoingDamage(10));
    }
}