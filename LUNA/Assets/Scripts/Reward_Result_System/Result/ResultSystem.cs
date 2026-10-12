using UnityEngine;

public class ResultSystem : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private int maxScore = 600;

    [Header("Star Conditions")]
    [SerializeField] private int oneStarScore = 200;
    [SerializeField] private int twoStarScore = 400;
    [SerializeField] private int threeStarScore = 600;

    public BattleResultData CurrentResult { get; private set; }

    public void CalculateResult(
        int damageDealt,
        int damageReceived,
        int killCount,
        int goldEarned)
    {
        BattleResultData result = new BattleResultData();

        // Luu du lieu tran dau
        result.damageDealt = damageDealt;
        result.damageReceived = damageReceived;
        result.killCount = killCount;
        result.goldEarned = goldEarned;

        // Tinh diem
        int rawScore =
            damageDealt
            + killCount * 20
            + goldEarned * 2
            - damageReceived;

        // Gioi han diem tu 0 den 600
        result.score = Mathf.Clamp(rawScore, 0, maxScore);

        // Tinh sao
        if (result.score >= threeStarScore)
        {
            result.star = 3;
        }
        else if (result.score >= twoStarScore)
        {
            result.star = 2;
        }
        else if (result.score >= oneStarScore)
        {
            result.star = 1;
        }
        else
        {
            result.star = 0;
        }

        // Duoi 200 diem la that bai
        result.victory = result.star >= 2;

        CurrentResult = result;

        Debug.Log(
            "Score: " + result.score
            + " | Star: " + result.star
            + " | Victory: " + result.victory
        );
    }
}