
using UnityEngine;

[CreateAssetMenu(
    fileName = "Stage_01",
    menuName = "Level Stage/Stage"
)]
public class StageData : ScriptableObject
{
    [Header("Nhận diện")]
    public string stageId = "stage_01";
    public string displayName = "Tên Stage";
    public int order = 1;

    [Header("Điều kiện mở khóa")]
    public int requiredPlayerLevel = 1;

    [Header("Loại Stage")]
    public bool isBossStage;

    [Header("Scene chiến đấu")]
    public string sceneToLoad = "MapBT";

    [Header("Enemy theo từng Round")]
    [Tooltip(
        "Mỗi phần tử là vị trí Enemy trong " +
        "mảng EnemySpawner.EnemyPrefabs. " +
        "Số phần tử chính là số Round."
    )]
    public int[] enemyIndices = { 0, 1 };

    [Header("Phần thưởng cơ bản (Easy)")]
    [Min(0)] public int rewardCoin = 100;
    [Min(0)] public int rewardExp = 50;

    [Header("Mô tả")]
    [TextArea(2, 4)]
    public string description;

    // Số Round của Stage.
    public int RoundCount
    {
        get
        {
            return enemyIndices != null
                ? enemyIndices.Length
                : 0;
        }
    }

    // Enemy cần Spawn tại Round được chỉ định.
    // round bắt đầu từ 1.
    public int GetEnemyIndex(int round)
    {
        if (enemyIndices == null ||
            round < 1 ||
            round > enemyIndices.Length)
        {
            return -1;
        }

        return enemyIndices[round - 1];
    }

    // Hệ số thưởng giống StagePanel.
    public float GetDifficultyMultiplier(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return 1f;

            case Difficulty.Normal:
                return 1.5f;

            case Difficulty.Hard:
                return 2f;

            default:
                return 1f;
        }
    }

    public int GetRewardCoin(Difficulty difficulty)
    {
        return Mathf.RoundToInt(
            rewardCoin * GetDifficultyMultiplier(difficulty)
        );
    }

    public int GetRewardExp(Difficulty difficulty)
    {
        return Mathf.RoundToInt(
            rewardExp * GetDifficultyMultiplier(difficulty)
        );
    }
}
