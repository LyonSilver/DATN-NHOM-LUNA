
using System;
using UnityEngine;

public enum MissionCardType
{
    Attack,
    Guard,
    Support,
    Heal,
    Other
}

[Serializable]
public class MissionInfo
{
    public string title;
    [TextArea] public string description;
    public int target = 1;
    [NonSerialized] public int progress;
    [NonSerialized] public bool completed;
}

[Serializable]
public class MissionRewardData
{
    public int stars;
    public int exp;
    public int gold;
    public int cardCount;
    public bool victory;
}

public class MissionRewardManager : MonoBehaviour
{
    [Header("Mission UI")]
    [SerializeField] private MissionProgressUI progressUI;
    [SerializeField] private MissionResultUI resultUI;

    [Header("Missions")]
    [SerializeField]
    private MissionInfo mission1 = new MissionInfo
    {
        title = "Nhiệm vụ 1",
        description = "Đánh bại kẻ địch ở Round 1",
        target = 1
    };

    [SerializeField]
    private MissionInfo mission2 = new MissionInfo
    {
        title = "Nhiệm vụ 2",
        description = "Sử dụng ít nhất 1 thẻ Attack và 1 thẻ Guard hoặc Support",
        target = 2
    };

    [SerializeField]
    private MissionInfo mission3 = new MissionInfo
    {
        title = "Nhiệm vụ 3",
        description = "Đánh bại kẻ địch Round 2 và chiến thắng",
        target = 1
    };

    [Header("Reward - 1 Star")]
    [SerializeField] private int oneStarExp = 100;
    [SerializeField] private int oneStarGoldMin = 5;
    [SerializeField] private int oneStarGoldMax = 15;

    [Header("Reward - 2 Stars")]
    [SerializeField] private int twoStarExp = 200;
    [SerializeField] private int twoStarGoldMin = 30;
    [SerializeField] private int twoStarGoldMax = 60;
    [SerializeField] private int twoStarCards = 1;

    [Header("Reward - 3 Stars")]
    [SerializeField] private int threeStarExp = 300;
    [SerializeField] private int threeStarGoldMin = 80;
    [SerializeField] private int threeStarGoldMax = 150;
    [SerializeField] private int threeStarCards = 3;

    private bool usedAttack;
    private bool usedGuardOrSupport;
    private bool battleEnded;

    public MissionRewardData FinalReward { get; private set; }

    public MissionInfo Mission1 => mission1;
    public MissionInfo Mission2 => mission2;
    public MissionInfo Mission3 => mission3;

    private void Start()
    {
        ResetMissions();
    }

    public void ResetMissions()
    {
        ResetMission(mission1);
        ResetMission(mission2);
        ResetMission(mission3);

        usedAttack = false;
        usedGuardOrSupport = false;
        battleEnded = false;
        FinalReward = null;

        RefreshProgressUI();
    }

    private void ResetMission(MissionInfo mission)
    {
        mission.progress = 0;
        mission.completed = false;
    }

    // Gọi khi Enemy của một Round đã bị hạ.
    public void RegisterEnemyDefeated(int roundNumber)
    {
        if (battleEnded)
            return;

        if (roundNumber == 1)
            CompleteMission(mission1);

        RefreshProgressUI();
    }

    // Chỉ gọi sau khi thẻ đã được sử dụng thành công.

    public void RegisterCardUsed(MissionCardType type)
    {
        if (battleEnded || mission2.completed)
            return;

        if (type == MissionCardType.Attack)
            usedAttack = true;

        if (type == MissionCardType.Guard ||
            type == MissionCardType.Support)
        {
            usedGuardOrSupport = true;
        }

        mission2.progress =
            (usedAttack ? 1 : 0) +
            (usedGuardOrSupport ? 1 : 0);

        mission2.completed =
            usedAttack && usedGuardOrSupport;

        RefreshProgressUI();

        Debug.Log(
            $"Mission 2: Attack={usedAttack}, " +
            $"Guard/Support={usedGuardOrSupport}, " +
            $"Progress={mission2.progress}/2");
    }


    // Gọi khi trận đã kết thúc, truyền kết quả thắng/thua.
    public void RegisterBattleEnded(bool victory, int finalRound)
    {
        if (battleEnded)
            return;

        battleEnded = true;

        if (victory && finalRound >= 2)
            CompleteMission(mission3);

        int stars = 0;

        if (victory)
        {
            if (mission1.completed) stars++;
            if (mission2.completed) stars++;
            if (mission3.completed) stars++;
        }

        FinalReward = CalculateReward(victory, stars);

        RefreshProgressUI();

        if (resultUI != null)
            resultUI.ShowResult(mission1, mission2, mission3, FinalReward);
        else
            Debug.LogWarning("MissionResultUI chưa được gán.");
    }

    private void CompleteMission(MissionInfo mission)
    {
        mission.progress = mission.target;
        mission.completed = true;
    }

    private MissionRewardData CalculateReward(bool victory, int stars)
    {
        MissionRewardData reward = new MissionRewardData
        {
            victory = victory,
            stars = stars,
            exp = 0,
            gold = 0,
            cardCount = 0
        };

        if (!victory || stars == 0)
            return reward;

        if (stars == 1)
        {
            reward.exp = oneStarExp;
            reward.gold = UnityEngine.Random.Range(
                oneStarGoldMin, oneStarGoldMax + 1);
        }
        else if (stars == 2)
        {
            reward.exp = twoStarExp;
            reward.gold = UnityEngine.Random.Range(
                twoStarGoldMin, twoStarGoldMax + 1);
            reward.cardCount = twoStarCards;
        }
        else if (stars >= 3)
        {
            reward.exp = threeStarExp;
            reward.gold = UnityEngine.Random.Range(
                threeStarGoldMin, threeStarGoldMax + 1);
            reward.cardCount = threeStarCards;
        }

        return reward;
    }

    private void RefreshProgressUI()
    {
        if (progressUI != null)
            progressUI.Refresh(mission1, mission2, mission3);
    }
}
