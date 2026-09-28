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
        int star = result.star;

        // EXP
        RewardExp = baseExp * star;

        // Gold
        RewardGold = baseGold * star;

        // Card
        RewardCardCount = star;

        Debug.Log(
            "Reward -> EXP: " + RewardExp +
            " | Gold: " + RewardGold +
            " | Cards: " + RewardCardCount
        );
    }
}