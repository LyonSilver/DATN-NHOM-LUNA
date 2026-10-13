
using System;
using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    private string SaveFolder =>
        Application.persistentDataPath;

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

    private string GetSlotPath(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex > 3)
        {
            Debug.LogError("Slot khong hop le!");
            return null;
        }

        string fileName = slotIndex == 0
            ? "autosave.json"
            : $"manual_slot_{slotIndex:00}.json";

        return Path.Combine(SaveFolder, fileName);
    }

    public bool HasSave(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);

        return !string.IsNullOrEmpty(path)
            && File.Exists(path);
    }

    public void SaveGame(SaveData data, int slotIndex)
    {
        if (data == null)
        {
            Debug.LogError("SaveData khong ton tai!");
            return;
        }

        string path = GetSlotPath(slotIndex);

        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            Directory.CreateDirectory(SaveFolder);

            data.slotType = slotIndex == 0
                ? "Auto"
                : "Manual";

            data.lastSavedTime =
                DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(path, json);

            Debug.Log($"Da luu game tai slot {slotIndex}: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Loi Save Game: {e.Message}");
        }
    }

    public SaveData LoadGame(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Debug.LogWarning($"Slot {slotIndex} chua co du lieu!");
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);

            SaveData data = JsonUtility.FromJson<SaveData>(json);

            Debug.Log($"Da doc du lieu slot {slotIndex}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Loi Load Game: {e.Message}");
            return null;
        }
    }

    public void DeleteSave(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);

        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Da xoa slot {slotIndex}");
        }
    }
}
