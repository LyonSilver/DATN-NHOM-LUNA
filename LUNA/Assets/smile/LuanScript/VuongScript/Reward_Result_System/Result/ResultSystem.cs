using UnityEngine;

public class ResultSystem : MonoBehaviour
{
    [Header("Score Settings")]

    [SerializeField] private int minScore = 100;
    [SerializeField] private int maxScore = 600;


    [Header("Star Conditions")]

    [SerializeField] private int twoStarScore = 200;
    [SerializeField] private int threeStarScore = 400;


    public BattleResultData CurrentResult { get; private set; }


    public void CalculateResult(
        int damageDealt,
        int damageReceived,
        int killCount,
        int goldEarned)
    {
        // 1. TẠO DATA KẾT QUẢ

        BattleResultData result = new BattleResultData();


        // 2. LƯU DỮ LIỆU TRẬN ĐẤU

        result.damageDealt = damageDealt;

        result.damageReceived = damageReceived;

        result.killCount = killCount;

        result.goldEarned = goldEarned;


        // 3. TÍNH SCORE

        int rawScore =
            damageDealt
            + (killCount * 20)
            + (goldEarned * 2)
            - damageReceived;


        // 4. GIỚI HẠN SCORE

        result.score = Mathf.Clamp(
            rawScore,
            minScore,
            maxScore
        );


        // 5. TÍNH SỐ SAO

        if (result.score >= threeStarScore)
        {
            // 400 - 600
            result.star = 3;
        }
        else if (result.score >= twoStarScore)
        {
            // 200 - 399
            result.star = 2;
        }
        else
        {
            // 100 - 199
            result.star = 1;
        }


        // 6. XÁC ĐỊNH THẮNG / THUA

        if (result.star >= 2)
        {
            result.victory = true;
        }
        else
        {
            result.victory = false;
        }


        // 7. LƯU KẾT QUẢ HIỆN TẠI

        CurrentResult = result;


        // 8. DEBUG

        Debug.Log(
            "Damage Dealt: " + result.damageDealt +
            " | Damage Received: " + result.damageReceived +
            " | Kill: " + result.killCount +
            " | Gold: " + result.goldEarned +
            " | Score: " + result.score +
            " | Star: " + result.star +
            " | Victory: " + result.victory
        );
    }
}