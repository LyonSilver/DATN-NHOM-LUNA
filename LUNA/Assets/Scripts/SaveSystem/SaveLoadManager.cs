
using System;
using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private string currentPlayerId = "";

    public string CurrentPlayerId => currentPlayerId;

    private string SaveRoot =>
        Path.Combine(Application.persistentDataPath, "PlayerSaves");

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

    // Gọi khi đăng nhập hoặc xác định được Player ID
    public bool SetPlayerId(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            Debug.LogError("[Save] Player ID không hợp lệ!");
            return false;
        }

        currentPlayerId = playerId.Trim();
        Debug.Log("[Save] Player ID: " + currentPlayerId);
        return true;
    }

    private string GetPlayerFolder()
    {
        if (string.IsNullOrWhiteSpace(currentPlayerId))
        {
            Debug.LogError("[Save] Chưa thiết lập Player ID!");
            return null;
        }

        // Mã hóa ID để tránh ký tự gây lỗi đường dẫn
        string safeId = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(currentPlayerId)
        ).TrimEnd('=').Replace('/', '_').Replace('+', '-');

        return Path.Combine(SaveRoot, safeId);
    }

    private string GetSlotPath(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex > 3)
        {
            Debug.LogError("[Save] Slot không hợp lệ!");
            return null;
        }

        string folder = GetPlayerFolder();
        if (string.IsNullOrEmpty(folder))
            return null;

        string fileName = slotIndex == 0
            ? "autosave.json"
            : $"manual_slot_{slotIndex:00}.json";

        return Path.Combine(folder, fileName);
    }

    public bool HasSave(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);
        return !string.IsNullOrEmpty(path) && File.Exists(path);
    }

    public void SaveGame(SaveData data, int slotIndex)
    {
        if (data == null)
        {
            Debug.LogError("[Save] SaveData không tồn tại!");
            return;
        }

        string path = GetSlotPath(slotIndex);
        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            data.playerId = currentPlayerId;
            data.slotType = slotIndex == 0 ? "Auto" : "Manual";
            data.lastSavedTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            string json = JsonUtility.ToJson(data, true);
            string tempPath = path + ".tmp";

            File.WriteAllText(tempPath, json);

            if (File.Exists(path))
                File.Delete(path);

            File.Move(tempPath, path);

            Debug.Log($"[Save] Player {currentPlayerId}, Slot {slotIndex}");
        }
        catch (Exception e)
        {
            Debug.LogError("[Save] Lỗi: " + e.Message);
        }
    }

    public SaveData LoadGame(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return null;

        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(
                File.ReadAllText(path)
            );

            if (data == null || data.playerId != currentPlayerId)
            {
                Debug.LogError("[Save] Player ID không khớp!");
                return null;
            }

            return data;
        }
        catch (Exception e)
        {
            Debug.LogError("[Save] Lỗi đọc dữ liệu: " + e.Message);
            return null;
        }
    }

    public void DeleteSave(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);

        if (!string.IsNullOrEmpty(path) && File.Exists(path))
            File.Delete(path);
    }
}
