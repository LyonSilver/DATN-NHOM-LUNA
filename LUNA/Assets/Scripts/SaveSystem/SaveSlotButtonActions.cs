
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlotButtonActions : MonoBehaviour
{
    public void SaveNewSlot()
    {
        SaveToSlot(2);
    }

    public void SaveManualSlot()
    {
        SaveToSlot(1);
    }

    private void SaveToSlot(int slot)
    {
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError(
                "[Save System] Không tìm thấy SaveLoadManager."
            );
            return;
        }

        // Dữ liệu mẫu vì chưa kết nối với dữ liệu gameplay thực tế.
        SaveData data = new SaveData();
        SaveLoadManager.Instance.SaveGame(data, slot);

        Debug.Log($"Đã gọi lưu dữ liệu vào slot {slot}.");
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
                if (string.Equals(
                    sceneName,
                    menuName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    SceneManager.LoadScene(sceneName);
                    return;
                }
            }
        }

        Debug.LogWarning(
            "Không tìm thấy scene menu trong Build Settings. " +
            "Hãy kiểm tra tên scene và danh sách Scenes In Build."
        );
    }
}
