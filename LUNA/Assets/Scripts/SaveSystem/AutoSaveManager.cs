
using UnityEngine;

public class AutoSaveManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool enableAutoSave = true;

    [Header("References")]
    [SerializeField] private SaveLoadManager saveLoadManager;

    private SaveData currentProgress;
    private bool matchAlreadyEnded;

    public bool IsAutoSaveEnabled => enableAutoSave;

    private void Awake()
    {
        if (saveLoadManager == null)
            saveLoadManager = SaveLoadManager.Instance;

        if (saveLoadManager == null)
            saveLoadManager = FindFirstObjectByType<SaveLoadManager>();
    }

    // Gắn với UI Toggle
    public void SetAutoSave(bool enabled)
    {
        enableAutoSave = enabled;
    }

    public void EnableAutoSave() => SetAutoSave(true);
    public void DisableAutoSave() => SetAutoSave(false);

    // Gọi khi hệ thống game đã cập nhật dữ liệu Player
    public void SetCurrentProgress(SaveData data)
    {
        if (data == null)
            return;

        currentProgress = data;
    }

    // Gọi khi bắt đầu trận mới
    public void OnMatchStarted()
    {
        matchAlreadyEnded = false;
    }

    // Gọi đúng một lần khi trận kết thúc
    public void OnMatchEnded(string stageId, string result)
    {
        if (matchAlreadyEnded)
            return;

        matchAlreadyEnded = true;

        if (!enableAutoSave)
        {
            Debug.Log("[AutoSave] Đã tắt.");
            return;
        }

        SaveMatchResult(stageId, result);
    }

    private void SaveMatchResult(string stageId, string result)
    {
        if (saveLoadManager == null ||
            string.IsNullOrWhiteSpace(saveLoadManager.CurrentPlayerId))
        {
            Debug.LogError("[AutoSave] Chưa có Player ID!");
            return;
        }

        // Lấy dữ liệu mới nhất đã được hệ thống game cung cấp
        SaveData data = currentProgress;

        if (data == null)
            data = saveLoadManager.LoadGame(0);

        if (data == null)
            data = new SaveData();

        data.playerId = saveLoadManager.CurrentPlayerId;
        data.currentStageId = stageId;
        data.lastMatchResult = result;

        data.matchesPlayed++;

        if (result == "Win")
            data.matchesWon++;
        else if (result == "Lose")
            data.matchesLost++;

        saveLoadManager.SaveGame(data, 0);

        Debug.Log("[AutoSave] Đã lưu kết quả trận: " + result);
    }

    // Save thủ công, không phụ thuộc Auto Save bật hay tắt
    public void ManualSave()
    {
        if (saveLoadManager == null)
            return;

        SaveData data = currentProgress;

        if (data == null)
            data = saveLoadManager.LoadGame(0);

        if (data == null)
            data = new SaveData();

        saveLoadManager.SaveGame(data, 1);
    }
}
