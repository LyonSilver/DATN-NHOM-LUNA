using UnityEngine;

public class ResultSystem : MonoBehaviour
{
    [Header("Star Conditions")]
    [SerializeField] private int oneStarScore = 300;
    [SerializeField] private int twoStarScore = 600;
    [SerializeField] private int threeStarScore = 1000;

    public BattleResultData CurrentResult { get; private set; }

    public void CalculateResult(
        int damageDealt,
        int damageReceived,
        int killCount,
        int goldEarned)
    {
        // Tạo dữ liệu kết quả
        BattleResultData result = new BattleResultData();

        // Lưu dữ liệu trận
        result.damageDealt = damageDealt;
        result.damageReceived = damageReceived;
        result.killCount = killCount;
        result.goldEarned = goldEarned;

        // 1. TÍNH SCORE

        result.score =
            damageDealt
            + (killCount * 100)
            + (goldEarned * 10)
            - damageReceived;

        // 2. TÍNH SỐ SAO

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

        
        // 3. XÁC ĐỊNH THẮNG / THUA

        if (result.star >= 2)
        {
            result.victory = true;
        }
        else
        {
            result.victory = false;
        }

        // Lưu kết quả hiện tại
        CurrentResult = result;

        // Debug để kiểm tra
        Debug.Log(
            "Score: " + result.score +
            " | Star: " + result.star +
            " | Victory: " + result.victory
        );
    }
}