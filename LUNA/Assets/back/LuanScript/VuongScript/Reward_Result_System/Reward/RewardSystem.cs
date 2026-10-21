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

        // RESET PHẦN THƯỞNG

        RewardExp = 0;
        RewardGold = 0;
        RewardCardCount = 0;


        // 1 SAO
        // Chỉ nhận EXP

        if (star == 1)
        {
            RewardExp = baseExp;
            RewardGold = 0;
            RewardCardCount = 0;
        }


        // 2 SAO
        // EXP + GOLD + 2 CARD

        else if (star == 2)
        {
            RewardExp = baseExp * 2;
            RewardGold = baseGold * 2;
            RewardCardCount = 2;
        }


        // 3 SAO
        // EXP + GOLD + 3 CARD

        else if (star == 3)
        {
            RewardExp = baseExp * 3;
            RewardGold = baseGold * 3;
            RewardCardCount = 3;
        }


        // DEBUG

        Debug.Log(
            "Reward -> EXP: " + RewardExp +
            " | Gold: " + RewardGold +
            " | Cards: " + RewardCardCount
        );
    }
}