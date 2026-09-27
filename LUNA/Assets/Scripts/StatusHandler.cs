using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusHandler : MonoBehaviour
{
    public List<StatusInstance> activeStatuses = new List<StatusInstance>();
    public event Action OnStatusUpdated; // Báo cho UI làm mới

    // 1. Thêm & Cộng dồn Status
    public void ApplyStatus(StatusType type, int duration, int stacks, StackType stackType)
    {
        var existing = activeStatuses.Find(s => s.type == type);
        if (existing != null)
        {
            if (stackType == StackType.Duration)
                existing.duration += duration;
            else if (stackType == StackType.Intensity)
                existing.stacks += stacks;
        }
        else
        {
            activeStatuses.Add(new StatusInstance(type, duration, stacks, stackType));
        }
        OnStatusUpdated?.Invoke();
    }

    // 2. Chạy đòn đánh DoT & Trừ lượt ở đầu/cuối Turn
    public void TickTurnStart(Action<int> takeDamageCallback)
    {
        for (int i = activeStatuses.Count - 1; i >= 0; i--)
        {
            var status = activeStatuses[i];

            // Trừ HP qua Poison / Burn
            if (status.type == StatusType.Poison || status.type == StatusType.Burn)
            {
                int damage = status.stacks * 5;
                takeDamageCallback?.Invoke(damage);
            }

            // Trừ 1 Duration & Tự xóa khi hết lượt
            status.duration--;
            if (status.duration <= 0)
                activeStatuses.RemoveAt(i);
        }
        OnStatusUpdated?.Invoke();
    }

    // 3. Xử lý Stun & Weak
    public bool IsStunned() => activeStatuses.Exists(s => s.type == StatusType.Stun);

    public float GetDamageMultiplier()
    {
        var weak = activeStatuses.Find(s => s.type == StatusType.Weak);
        return weak != null ? 0.7f : 1.0f; // Giảm 30% dame khi bị Weak
    }

    // 4. Cleanse (Xóa debuff)
    public void Cleanse(bool clearAll = true, StatusType targetType = StatusType.Poison)
    {
        if (clearAll)
            activeStatuses.Clear();
        else
            activeStatuses.RemoveAll(s => s.type == targetType);

        OnStatusUpdated?.Invoke();
    }
}