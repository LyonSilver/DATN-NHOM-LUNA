using UnityEngine;

public class RewardSystem : MonoBehaviour
{
    [Header("Reward Settings")]
    [SerializeField] private int baseExp = 100;
    [SerializeField] private int baseGold = 50;

    public int RewardExp { get; private set; }
    public int RewardGold { get; private set; }
    public int RewardCardCount { get; private set; }

    public void CalculateReward(BattleResultData result)
    {
        // Reset phan thuong
        RewardExp = 0;
        RewardGold = 0;
        RewardCardCount = 0;

        if (result == null)
        {
            Debug.LogWarning("BattleResultData dang null!");
            return;
        }

        // 0 sao: khong co phan thuong

        if (result.star == 0)
        {
            return;
        }

        // 1 sao: chi nhan EXP

        if (result.star == 1)
        {
            RewardExp = baseExp;
        }

        // 2 sao: EXP + Gold + 1 Card

        else if (result.star == 2)
        {
            RewardExp = baseExp * 2;
            RewardGold = baseGold * 2;
            RewardCardCount = 1;
        }

        // 3 sao: EXP + Gold + 3 Card

        else if (result.star == 3)
        {
            RewardExp = baseExp * 3;
            RewardGold = baseGold * 3;
            RewardCardCount = 3;
        }

        Debug.Log(
            "EXP: " + RewardExp
            + " | Gold: " + RewardGold
            + " | Cards: " + RewardCardCount
        );
    }
}