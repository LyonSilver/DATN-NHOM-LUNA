using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlotButtonActions : MonoBehaviour
{
    public void SaveManualSlot()
    {
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("SaveLoadManager not found!");
            if (SaveNotificationUI.Instance != null)
                SaveNotificationUI.Instance.Show("Save system error!");
            return;
        }

        // Đảm bảo có Player ID
        if (string.IsNullOrEmpty(SaveLoadManager.Instance.CurrentPlayerId))
            SaveLoadManager.Instance.SetPlayerId("player_001");

        // Lấy dữ liệu từ Auto Save trước, không có thì tạo mới
        SaveData data = SaveLoadManager.Instance.LoadGame(0);

        if (data == null)
            data = SaveLoadManager.Instance.CreateNewSaveData();

        data.playerId = SaveLoadManager.Instance.CurrentPlayerId;
        data.slotType = "Manual";

        bool success = SaveLoadManager.Instance.SaveGame(data, 1);

        if (success)
        {
            if (SaveNotificationUI.Instance != null)
                SaveNotificationUI.Instance.Show("Game Saved - Slot 1");
            Debug.Log("Saved to Slot 1 successfully!");
        }
        else
        {
            if (SaveNotificationUI.Instance != null)
                SaveNotificationUI.Instance.Show("Failed to save Slot 1!");
            Debug.LogError("Failed to save Slot 1");
        }
    }

    public void SaveNewSlot()
    {
        // Kiểm tra Slot 2 đã mở khóa chưa
        if (Slot2Lock.Instance != null && !Slot2Lock.Instance.CanUseSlot2())
        {
            if (SaveNotificationUI.Instance != null)
                SaveNotificationUI.Instance.Show("Slot 2 is locked!");
            return;
        }

        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("SaveLoadManager not found!");
            return;
        }

        if (string.IsNullOrEmpty(SaveLoadManager.Instance.CurrentPlayerId))
            SaveLoadManager.Instance.SetPlayerId("player_001");

        SaveData data = SaveLoadManager.Instance.LoadGame(0);

        if (data == null)
            data = SaveLoadManager.Instance.CreateNewSaveData();

        data.playerId = SaveLoadManager.Instance.CurrentPlayerId;
        data.slotType = "Manual";

        bool success = SaveLoadManager.Instance.SaveGame(data, 2);

        if (success)
        {
            if (SaveNotificationUI.Instance != null)
                SaveNotificationUI.Instance.Show("Game Saved - Slot 2");
        }
        else
        {
            if (SaveNotificationUI.Instance != null)
                SaveNotificationUI.Instance.Show("Failed to save Slot 2!");
        }
    }

    public void LoadAutoSave()
    {
        var data = SaveLoadManager.Instance?.LoadGame(0);
        if (data != null)
        {
            Debug.Log("Loaded Auto Save");
            // TODO: Apply data vào gameplay
        }
        else
        {
            if (SaveNotificationUI.Instance != null)
                SaveNotificationUI.Instance.Show("No Auto Save found!");
        }
    }

    public void LoadManualSave()
    {
        var data = SaveLoadManager.Instance?.LoadGame(1);
        if (data != null)
        {
            Debug.Log("Loaded Manual Save (Slot 1)");
            // TODO: Apply data vào gameplay
        }
        else
        {
            if (SaveNotificationUI.Instance != null)
                SaveNotificationUI.Instance.Show("Slot 1 is empty!");
        }
    }

    public void ExitToMenu()
    {
        string[] menuNames =
        {
            "MainMenu", "Main Menu",
            "StartMenu", "Start Menu", "Menu"
        };

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = Path.GetFileNameWithoutExtension(path);

            foreach (string menuName in menuNames)
            {
                if (string.Equals(sceneName, menuName, StringComparison.OrdinalIgnoreCase))
                {
                    SceneManager.LoadScene(sceneName);
                    return;
                }
            }
        }

        Debug.LogWarning("Menu scene not found in Build Settings.");
    }
}