using System;
using UnityEngine;
using UnityEngine.UI;

public class UnlockDemoController : MonoBehaviour
{
    [Header("Demo Data")]
    public int playerLevel = 1;

    [Header("UI References")]
    public Text statusText;
    public Text levelText;
    public UnlockableItem cardItem;
    public UnlockableItem stageItem;
    public UnlockableItem bossItem;
    public UnlockableItem levelItem;
    public UnlockableItem rewardItem;

    private void Start()
    {
        UpdateLevelUI();
        UpdateStatus("Demo System Ready.");
    }

    public void DemoUnlockCard()
    {
        bool result = UnlockManager.Instance.Unlock(UnlockType.Card, "Card_FireDragon");
        UpdateStatus(result ? "Unlocked Card_FireDragon" : "Card_FireDragon is already unlocked");
    }

    public void DemoUnlockStage()
    {
        bool result = UnlockManager.Instance.Unlock(UnlockType.Stage, "Stage_Forest_01");
        UpdateStatus(result ? "Unlocked Stage_Forest_01" : "Stage_Forest_01 is already unlocked");
    }

    public void DemoUnlockBoss()
    {
        bool result = UnlockManager.Instance.Unlock(UnlockType.Boss, "Boss_DemonKing");
        UpdateStatus(result ? "Unlocked Boss_DemonKing" : "Boss_DemonKing is already unlocked");
    }

    public void DemoLevelUp()
    {
        playerLevel += 3;
        UpdateLevelUI();

        if (levelItem != null)
        {
            levelItem.EvaluateLevelCondition(playerLevel);
        }

        UpdateStatus("Player Level Up -> " + playerLevel);
    }

    public void DemoUnlockReward()
    {
        bool result = UnlockManager.Instance.Unlock(UnlockType.Reward, "Reward_GoldChest");
        UpdateStatus(result ? "Unlocked Reward_GoldChest" : "Reward_GoldChest is already unlocked");
    }

    public void DemoResetData()
    {
        if (UnlockManager.Instance == null) return;
        UnlockManager.Instance.ResetAllData();

        if (cardItem != null) cardItem.RefreshUI();
        if (stageItem != null) stageItem.RefreshUI();
        if (bossItem != null) bossItem.RefreshUI();
        if (levelItem != null) levelItem.RefreshUI();
        if (rewardItem != null) rewardItem.RefreshUI();

        playerLevel = 1;
        UpdateLevelUI();
        UpdateStatus("Reset all data successfully");
    }

    private void UpdateLevelUI()
    {
        if (levelText != null)
        {
            levelText.text = "Current Level: " + playerLevel;
        }
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = "Status: " + message;
        }
    }
}

public class UnlockableItem
{
    public UnlockableItem()
    {
    }

    internal void EvaluateLevelCondition(int playerLevel)
    {
        throw new NotImplementedException();
    }

    internal void RefreshUI()
    {
        throw new NotImplementedException();
    }
}