using System;
using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private string currentPlayerId = "";

    public string CurrentPlayerId => currentPlayerId;

    private string SaveRoot => Path.Combine(Application.persistentDataPath, "PlayerSaves");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool SetPlayerId(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            Debug.LogError("[Save] Player ID không hợp lệ!");
            return false;
        }
        currentPlayerId = playerId.Trim();
        Debug.Log("[Save] Player ID set: " + currentPlayerId);
        return true;
    }

    private string GetSafeId(string id)
    {
        if (string.IsNullOrEmpty(id)) return "default";
        string safe = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(id))
            .TrimEnd('=').Replace('/', '_').Replace('+', '-');
        return safe;
    }

    private string GetPlayerFolder()
    {
        if (string.IsNullOrWhiteSpace(currentPlayerId))
        {
            Debug.LogWarning("[Save] Chưa có Player ID, dùng folder default");
            return Path.Combine(SaveRoot, "default");
        }
        return Path.Combine(SaveRoot, GetSafeId(currentPlayerId));
    }

    private string GetSlotPath(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex > 3)
        {
            Debug.LogError("[Save] Slot không hợp lệ: " + slotIndex);
            return null;
        }

        string folder = GetPlayerFolder();
        string fileName = slotIndex == 0 ? "autosave.json" : $"manual_slot_{slotIndex:00}.json";
        return Path.Combine(folder, fileName);
    }

    public bool HasSave(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);
        bool exists = !string.IsNullOrEmpty(path) && File.Exists(path);
        Debug.Log($"[Save] HasSave slot {slotIndex}: {exists} | Path: {path}");
        return exists;
    }

    public void SaveGame(SaveData data, int slotIndex)
    {
        if (data == null)
        {
            Debug.LogError("[Save] SaveData null!");
            return;
        }

        string path = GetSlotPath(slotIndex);
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            data.playerId = string.IsNullOrEmpty(currentPlayerId) ? "unknown" : currentPlayerId;
            data.slotType = slotIndex == 0 ? "Auto" : "Manual";
            data.lastSavedTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            string json = JsonUtility.ToJson(data, true);
            string tempPath = path + ".tmp";

            File.WriteAllText(tempPath, json);

            if (File.Exists(path))
                File.Delete(path);

            File.Move(tempPath, path);

            Debug.Log($"[Save] Thành công | Slot {slotIndex} | Path: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError("[Save] Lỗi ghi file: " + e.Message + "\nPath: " + path);
        }
    }

    public SaveData LoadGame(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);

        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("[Save] Path null");
            return null;
        }

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[Save] File không tồn tại: {path}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError("[Save] File rỗng: " + path);
                return null;
            }

            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null)
            {
                Debug.LogError("[Save] Parse JSON thất bại: " + path);
                return null;
            }

            // Không chặn load nếu Player ID khác (chỉ cảnh báo)
            if (!string.IsNullOrEmpty(currentPlayerId) && data.playerId != currentPlayerId)
            {
                Debug.LogWarning($"[Save] Player ID khác nhau. File: {data.playerId} | Hiện tại: {currentPlayerId}");
            }

            Debug.Log($"[Save] Load thành công | Slot {slotIndex} | Path: {path}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError("[Save] Lỗi đọc: " + e.Message + "\nPath: " + path);
            return null;
        }
    }

    public void DeleteSave(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("[Save] Đã xóa: " + path);
        }
    }

    // Debug: in ra toàn bộ đường dẫn save
    [ContextMenu("Log All Save Paths")]
    public void LogAllPaths()
    {
        Debug.Log("Save Root: " + SaveRoot);
        for (int i = 0; i <= 3; i++)
            Debug.Log($"Slot {i}: {GetSlotPath(i)} | Exists: {HasSave(i)}");
    }
}