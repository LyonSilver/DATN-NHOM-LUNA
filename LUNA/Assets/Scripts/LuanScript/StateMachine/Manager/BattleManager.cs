using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int enemyMaxHP = 50;
    public int enemyHP;
    public int enemyShield;
    public bool enemyBroken;
    public int enemyPoisonTurns;

    [Header("Player Stats")]
    public int playerMaxHP = 50;
    public int playerHP;
    public int playerShield;

    // Sự kiện thông báo cho HUD và các hệ thống khác khi chỉ số thay đổi
    public event System.Action OnBattleStateChanged;

    private void Start()
    {
        ResetBattle();
    }

    public void ResetBattle()
    {
        enemyHP = enemyMaxHP;
        enemyShield = 0;
        enemyBroken = false;
        enemyPoisonTurns = 0;

        playerHP = playerMaxHP;
        playerShield = 0;

        OnBattleStateChanged?.Invoke();
    }

    public void DamageEnemy(int amount)
    {
        int remaining = amount;

        if (enemyShield > 0)
        {
            int absorbed = Mathf.Min(enemyShield, remaining);
            enemyShield -= absorbed;
            remaining -= absorbed;
        }

        enemyHP = Mathf.Max(0, enemyHP - remaining);
        OnBattleStateChanged?.Invoke();
    }

    public void ShieldPlayer(int amount)
    {
        playerShield += amount;
        OnBattleStateChanged?.Invoke();
    }

    public void HealPlayer(int amount)
    {
        playerHP = Mathf.Min(playerMaxHP, playerHP + amount);
        OnBattleStateChanged?.Invoke();
    }

    public void PoisonEnemy(int stacks, int turns)
    {
        enemyPoisonTurns = Mathf.Max(enemyPoisonTurns, turns);
        OnBattleStateChanged?.Invoke();
    }

    public void BreakEnemy()
    {
        enemyBroken = true;
        OnBattleStateChanged?.Invoke();
    }

    [ContextMenu("TEST - Reset Battle State")]
    private void TestReset()
    {
        ResetBattle();
        Debug.Log("BattleManager TEST: Đã reset Enemy/Player về trạng thái ban đầu.");
    }
}