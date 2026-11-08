
using UnityEngine;

public class BattleResultTracker : MonoBehaviour
{
    [Header("Battle Statistics")]
    [SerializeField] private int damageDealt;
    [SerializeField] private int damageReceived;
    [SerializeField] private int killCount;

    public int DamageDealt => damageDealt;
    public int DamageReceived => damageReceived;
    public int KillCount => killCount;

    private void Awake()
    {
        ResetStats();
    }

    // Reset khi bắt đầu trận mới.
    public void ResetStats()
    {
        damageDealt = 0;
        damageReceived = 0;
        killCount = 0;
    }

    // Damage thật sự gây vào HP của Enemy.
    public void AddDamageDealt(int damage)
    {
        if (damage <= 0)
            return;

        damageDealt += damage;

        Debug.Log(
            $"Battle Stats | Damage Dealt: {damageDealt}"
        );
    }

    // Damage thật sự trừ vào HP của Player.
    public void AddDamageReceived(int damage)
    {
        if (damage <= 0)
            return;

        damageReceived += damage;

        Debug.Log(
            $"Battle Stats | Damage Received: {damageReceived}"
        );
    }

    // Ghi nhận Enemy đã bị tiêu diệt.
    public void AddKill()
    {
        killCount++;

        Debug.Log(
            $"Battle Stats | Kill Count: {killCount}"
        );
    }
}