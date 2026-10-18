using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    public event Action<SaveData, int> OnSaved;
    public event Action<SaveData, int> OnLoaded;
    public event Action<string> OnSaveError;

    [Header("Player")]
    [SerializeField] private string currentPlayerId = "";

    [Header("Settings")]
    [SerializeField] private bool enableChecksum = true;
    [SerializeField] private bool enableBackup = true;
    [SerializeField] private int maxBackupCount = 2;

    public string CurrentPlayerId => currentPlayerId;
    public const int CURRENT_SAVE_VERSION = 1;

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
        Debug.Log("[Save] Player ID: " + currentPlayerId);
        return true;
    }

    private string GetSafePlayerFolder()
    {
        if (string.IsNullOrWhiteSpace(currentPlayerId))
        {
            Debug.LogError("[Save] Chưa thiết lập Player ID!");
            return null;
        }

        string safeId = Convert.ToBase64String(Encoding.UTF8.GetBytes(currentPlayerId))
            .TrimEnd('=').Replace('/', '_').Replace('+', '-');

        return Path.Combine(SaveRoot, safeId);
    }

    private string GetSlotPath(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex > 3)
        {
            Debug.LogError("[Save] Slot không hợp lệ (0-3)!");
            return null;
        }

        string folder = GetSafePlayerFolder();
        if (string.IsNullOrEmpty(folder)) return null;

        string fileName = slotIndex == 0
            ? "autosave.json"
            : $"manual_slot_{slotIndex:00}.json";

        return Path.Combine(folder, fileName);
    }

    private string GetBackupPath(int slotIndex, int backupIndex)
    {
        string mainPath = GetSlotPath(slotIndex);
        if (string.IsNullOrEmpty(mainPath)) return null;
        return mainPath + $".bak{backupIndex}";
    }

    public bool HasSave(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);
        return !string.IsNullOrEmpty(path) && File.Exists(path);
    }

    // ===================== SAVE =====================
    public bool SaveGame(SaveData data, int slotIndex)
    {
        if (data == null)
        {
            LogError("SaveData null!");
            return false;
        }

        string path = GetSlotPath(slotIndex);
        if (string.IsNullOrEmpty(path)) return false;

        try
        {
            data.EnsureLists();
            data.playerId = currentPlayerId;
            data.slotType = slotIndex == 0 ? "Auto" : "Manual";
            data.lastSavedTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            data.saveVersion = CURRENT_SAVE_VERSION;

            if (enableChecksum)
                data.checksum = CalculateChecksum(data);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // Backup trước khi ghi đè
            if (enableBackup && File.Exists(path))
                CreateBackup(slotIndex);

            string json = JsonUtility.ToJson(data, true);
            string tempPath = path + ".tmp";

            File.WriteAllText(tempPath, json, Encoding.UTF8);

            if (File.Exists(path))
                File.Delete(path);

            File.Move(tempPath, path);

            Debug.Log($"[Save] Thành công → Player:{currentPlayerId} | Slot:{slotIndex}");
            OnSaved?.Invoke(data, slotIndex);
            return true;
        }
        catch (Exception e)
        {
            LogError("Lỗi khi Save: " + e.Message);
            return false;
        }
    }

    // ===================== LOAD =====================
    public SaveData LoadGame(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                LogError("Parse SaveData thất bại → thử backup");
                return TryLoadFromBackup(slotIndex);
            }

            if (data.playerId != currentPlayerId)
            {
                LogError("Player ID không khớp!");
                return null;
            }

            data.EnsureLists();

            // Version migration
            if (data.saveVersion < CURRENT_SAVE_VERSION)
                MigrateSave(data);

            // Checksum
            if (enableChecksum && !string.IsNullOrEmpty(data.checksum))
            {
                string expected = data.checksum;
                data.checksum = ""; // tạm xóa để tính lại
                string actual = CalculateChecksum(data);
                data.checksum = expected;

                if (expected != actual)
                {
                    LogError("Checksum không khớp! File có thể bị hỏng → thử backup");
                    return TryLoadFromBackup(slotIndex);
                }
            }

            OnLoaded?.Invoke(data, slotIndex);
            return data;
        }
        catch (Exception e)
        {
            LogError("Lỗi Load: " + e.Message);
            return TryLoadFromBackup(slotIndex);
        }
    }

    private SaveData TryLoadFromBackup(int slotIndex)
    {
        for (int i = 0; i < maxBackupCount; i++)
        {
            string bakPath = GetBackupPath(slotIndex, i);
            if (string.IsNullOrEmpty(bakPath) || !File.Exists(bakPath))
                continue;

            try
            {
                string json = File.ReadAllText(bakPath, Encoding.UTF8);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                if (data != null && data.playerId == currentPlayerId)
                {
                    Debug.LogWarning($"[Save] Đã khôi phục từ backup {i}");
                    data.EnsureLists();
                    return data;
                }
            }
            catch { /* tiếp tục thử backup tiếp theo */ }
        }

        LogError("Không thể load cả file chính lẫn backup!");
        return null;
    }

    private void CreateBackup(int slotIndex)
    {
        string mainPath = GetSlotPath(slotIndex);
        if (string.IsNullOrEmpty(mainPath) || !File.Exists(mainPath)) return;

        // Xoay vòng backup: bak0 ← bak1 ← main
        for (int i = maxBackupCount - 1; i > 0; i--)
        {
            string older = GetBackupPath(slotIndex, i);
            string newer = GetBackupPath(slotIndex, i - 1);

            if (File.Exists(newer))
            {
                if (File.Exists(older)) File.Delete(older);
                File.Move(newer, older);
            }
        }

        string bak0 = GetBackupPath(slotIndex, 0);
        if (File.Exists(bak0)) File.Delete(bak0);
        File.Copy(mainPath, bak0);
    }

    // ===================== CHECKSUM =====================
    private string CalculateChecksum(SaveData data)
    {
        // Tạm thời bỏ checksum field để tính
        string original = data.checksum;
        data.checksum = "";

        string json = JsonUtility.ToJson(data);
        data.checksum = original;

        using (var md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(json));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    // ===================== MIGRATION =====================
    private void MigrateSave(SaveData data)
    {
        // Ví dụ: từ version 0 → 1
        if (data.saveVersion < 1)
        {
            data.EnsureLists();
            // thêm field mới ở đây nếu cần
        }

        data.saveVersion = CURRENT_SAVE_VERSION;
        Debug.Log($"[Save] Migrated to version {CURRENT_SAVE_VERSION}");
    }

    // ===================== UTILS =====================
    public void DeleteSave(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
            File.Delete(path);

        for (int i = 0; i < maxBackupCount; i++)
        {
            string bak = GetBackupPath(slotIndex, i);
            if (!string.IsNullOrEmpty(bak) && File.Exists(bak))
                File.Delete(bak);
        }
    }

    public SaveData CreateNewSaveData()
    {
        var data = new SaveData
        {
            playerId = currentPlayerId,
            playerLevel = 1,
            gold = 100,
            chapter = 1
        };
        data.EnsureLists();
        return data;
    }

    private void LogError(string msg)
    {
        Debug.LogError("[Save] " + msg);
        OnSaveError?.Invoke(msg);
    }

    [ContextMenu("Delete All Saves Of Current Player")]
    public void DeleteAllSavesOfCurrentPlayer()
    {
        for (int i = 0; i <= 3; i++)
            DeleteSave(i);
    }
}