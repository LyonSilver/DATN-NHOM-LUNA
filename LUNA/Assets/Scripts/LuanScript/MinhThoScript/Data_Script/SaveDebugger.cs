using UnityEngine;

public class SaveDebugger : MonoBehaviour
{
    [Header("Thông số giả lập")]
    [SerializeField] string stageId = "stage_01";
    [SerializeField, Range(0, 3)] int stars = 2;
    [SerializeField] int score = 1000;
    [SerializeField, Min(1)] int playerLevel = 1;

    [ContextMenu("1. Print Save")]
    void PrintSave()
    {
        _SaveData d = _SaveSystem.ReloadFromDisk();
        Debug.Log("File save: " + _SaveSystem.FilePath);
        Debug.Log("Level " + d.playerLevel + " | EXP " + d.exp + " | Coin " + d.coin);
        foreach (StageProgress p in d.stages)
            Debug.Log(p.stageId + ": Cleared =" + p.cleared + ", sao =" + p.bestStars + "/3, điểm cao =" + p.highScore);
        if (d.stages.Count == 0) Debug.Log("Chưa có Stage nào được lưu.");
    }

    [ContextMenu("2. Report Stage Result")]
    void ReportResult()
    {
        _SaveSystem.ReportStageResult(stageId, stars, score);
        Debug.Log("Đã lưu kết quả " + stageId + " với " + stars + " sao, điểm " + score);
    }

    [ContextMenu("3. Set Player Level")]
    void SetLevel()
    {
        _SaveData d = _SaveSystem.Load();
        d.playerLevel = playerLevel;
        _SaveSystem.Save();
        Debug.Log("Đã đặt Player Level = " + playerLevel);
    }

    [ContextMenu("4. Reset Save")]
    void ResetSave()
    {
        _SaveSystem.Reset();
        Debug.Log("Đã xóa toàn bộ tiến trình.");
    }
}