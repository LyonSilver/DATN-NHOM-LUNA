using System;
using System.Collections.Generic;
using UnityEngine;

public class _EnemyCombatState : MonoBehaviour
{
    [Header("Phương hệ của Enemy (đồng đội gán bằng SetElement khi tải Enemy)")]
    public CardElement element = CardElement.None;

    [Header("Số liệu Burn/Weak/Stun (Poison và Break dùng cơ chế sẵn có của CardEffectManager)")]
    public int burnDamagePerTurn = 4;
    [Range(0.1f, 1f)]
    public float weakOutgoingMultiplier = 0.75f;

    private readonly Dictionary<StatusEffectType, int> statuses = new Dictionary<StatusEffectType, int>();

    public event Action OnStateChanged;

    public CardElement Element
    {
        get { return element; }
    }

    // =====================================================
    // PHƯƠNG HỆ
    // =====================================================

    public void SetElement(CardElement newElement)
    {
        element = newElement;
        OnStateChanged?.Invoke();
    }

    // =====================================================
    // TRẠNG THÁI
    // =====================================================

    public void ApplyStatus(StatusEffectType status, int turns)
    {
        if (turns <= 0)
        {
            return;
        }

        if (status != StatusEffectType.Burn &&
            status != StatusEffectType.Weak &&
            status != StatusEffectType.Stun)
        {
            Debug.LogWarning("EnemyCombatState: " + status +
                             " không do script này quản lý (Poison/Break dùng CardEffectManager).");
            return;
        }

        int current;
        statuses.TryGetValue(status, out current);
        statuses[status] = Mathf.Max(current, turns);

        Debug.Log("EnemyCombatState: Gắn " + status + " " + statuses[status] + " lượt.");
        OnStateChanged?.Invoke();
    }

    public bool HasStatus(StatusEffectType status)
    {
        return statuses.ContainsKey(status);
    }

    public int GetTurnsLeft(StatusEffectType status)
    {
        int turns;
        return statuses.TryGetValue(status, out turns) ? turns : 0;
    }

    public void ClearAll()
    {
        statuses.Clear();
        OnStateChanged?.Invoke();
    }

    public void AppendStatusParts(List<string> parts)
    {
        foreach (KeyValuePair<StatusEffectType, int> pair in statuses)
        {
            parts.Add(pair.Key + " " + pair.Value);
        }
    }

    // =====================================================
    // HIỆU LỰC (CardCombatExtension gọi)
    // =====================================================

    public int GetBurnTickDamage()
    {
        return HasStatus(StatusEffectType.Burn) ? Mathf.Max(0, burnDamagePerTurn) : 0;
    }

    public int ModifyOutgoingDamage(int damage)
    {
        if (damage <= 0 || !HasStatus(StatusEffectType.Weak))
        {
            return damage;
        }

        return Mathf.Max(1, Mathf.RoundToInt(damage * weakOutgoingMultiplier));
    }

    // Gọi bao nhiêu lần cũng được. Stun tự hết ở đầu lượt Player kế tiếp.
    public bool ShouldSkipTurn()
    {
        return HasStatus(StatusEffectType.Stun);
    }

    // Đầu lượt Player: trừ 1 lượt mọi trạng thái.
    public void OnPlayerTurnStart()
    {
        if (statuses.Count == 0)
        {
            return;
        }

        List<StatusEffectType> keys = new List<StatusEffectType>(statuses.Keys);

        foreach (StatusEffectType key in keys)
        {
            statuses[key] = statuses[key] - 1;

            if (statuses[key] <= 0)
            {
                statuses.Remove(key);
            }
        }

        OnStateChanged?.Invoke();
    }

    // =====================================================
    // TEST
    // =====================================================

    [ContextMenu("TEST - Phương hệ kế tiếp")]
    private void TestNextElement()
    {
        int count = Enum.GetValues(typeof(CardElement)).Length;
        SetElement((CardElement)(((int)element + 1) % count));
        Debug.Log("EnemyCombatState TEST: Enemy hệ " + ElementRules.GetNameVN(element));
    }

    [ContextMenu("TEST - Xóa mọi trạng thái")]
    private void TestClear()
    {
        ClearAll();
    }
}