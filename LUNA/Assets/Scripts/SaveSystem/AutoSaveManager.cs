using UnityEngine;

public class AutoSaveManager : MonoBehaviour
{
    public static AutoSaveManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float minSecondsBetweenSaves = 3f;   // chống spam

    [Header("References")]
    [SerializeField] private SaveLoadManager saveLoadManager;

    private SaveData currentProgress;
    private bool matchAlreadyEnded;
    private float lastSaveTime = -999f;

    public bool IsAutoSaveEnabled => enableAutoSave;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (saveLoadManager == null)
            saveLoadManager = SaveLoadManager.Instance;

        if (saveLoadManager == null)
            saveLoadManager = FindFirstObjectByType<SaveLoadManager>();
    }

    public void SetAutoSave(bool enabled)
    {
        enableAutoSave = enabled;
        if (currentProgress != null)
            currentProgress.autoSaveEnabled = enabled;
    }

    public void EnableAutoSave() => SetAutoSave(true);
    public void DisableAutoSave() => SetAutoSave(false);

    /// <summary>
    /// Gọi mỗi khi gameplay thay đổi dữ liệu quan trọng (level up, nhận gold, unlock...)
    /// </summary>
    public void SetCurrentProgress(SaveData data)
    {
        if (data == null) return;
        currentProgress = data;
        currentProgress.autoSaveEnabled = enableAutoSave;
    }

    public void OnMatchStarted()
    {
        matchAlreadyEnded = false;
    }

    public void OnMatchEnded(string stageId, string result)
    {
        if (matchAlreadyEnded) return;
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
        if (!CanSaveNow()) return;

        if (saveLoadManager == null || string.IsNullOrWhiteSpace(saveLoadManager.CurrentPlayerId))
        {
            Debug.LogError("[AutoSave] Chưa có Player ID!");
            return;
        }

        SaveData data = currentProgress ?? saveLoadManager.LoadGame(0) ?? saveLoadManager.CreateNewSaveData();

        data.playerId = saveLoadManager.CurrentPlayerId;
        data.currentStageId = stageId;
        data.lastMatchResult = result;
        data.matchesPlayed++;

        if (result == "Win") data.matchesWon++;
        else if (result == "Lose") data.matchesLost++;

        bool success = saveLoadManager.SaveGame(data, 0);
        if (success)
        {
            lastSaveTime = Time.realtimeSinceStartup;
            currentProgress = data;
            Debug.Log("[AutoSave] Đã lưu kết quả trận: " + result);
        }
    }

    /// <summary>
    /// Force save ngay lập tức (bỏ qua cooldown)
    /// </summary>
    public void ForceSave()
    {
        if (saveLoadManager == null) return;

        SaveData data = currentProgress ?? saveLoadManager.LoadGame(0) ?? saveLoadManager.CreateNewSaveData();
        if (saveLoadManager.SaveGame(data, 0))
        {
            lastSaveTime = Time.realtimeSinceStartup;
            currentProgress = data;
        }
    }

    public void ManualSave(int slot = 1)
    {
        if (saveLoadManager == null) return;

        SaveData data = currentProgress ?? saveLoadManager.LoadGame(0) ?? saveLoadManager.CreateNewSaveData();
        saveLoadManager.SaveGame(data, slot);
    }

    private bool CanSaveNow()
    {
        if (!enableAutoSave) return false;
        return Time.realtimeSinceStartup - lastSaveTime >= minSecondsBetweenSaves;
    }
}