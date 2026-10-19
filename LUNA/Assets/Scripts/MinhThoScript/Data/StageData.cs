using UnityEngine;

[CreateAssetMenu(fileName = "Stage_01", menuName = "Level Stage/Stage")]
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
    public string sceneToLoad = "Battle";

    [Header("Phần thưởng cơ bản (Độ khó dễ)")]
    public int rewardCoin = 100;
    public int rewardExp = 50;

    [Header("Mô tả")]
    [TextArea(2, 4)]
    public string description;
}