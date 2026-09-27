using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public void ClaimReward(int rewardCoin, string rewardCardID = null)
    {
        SaveData saveData = SaveSystem.Load();

        saveData.coin += rewardCoin;

        if (!string.IsNullOrEmpty(rewardCardID) && !saveData.unlockedCardIDs.Contains(rewardCardID))
        {
            saveData.unlockedCardIDs.Add(rewardCardID);
        }

        string rewardInfo = $"Nhận {rewardCoin} Coin" + (string.IsNullOrEmpty(rewardCardID) ? "" : $" & Lá bài {rewardCardID}");
        SaveSystem.AutoSave(saveData, rewardInfo);
    }
}