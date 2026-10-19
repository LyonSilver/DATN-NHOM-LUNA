
using UnityEngine;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text autoSaveInfo;
    public TMP_Text manualSaveInfo;

    private void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        // Auto Save: Slot 0
        SaveData autoData = SaveLoadManager.Instance.LoadGame(0);

        if (autoData != null)
        {
            autoSaveInfo.text =
                $"Chapter: {autoData.chapter}\n" +
                $"Level: {autoData.playerLevel}\n" +
                $"Gold: {autoData.gold}\n" +
                $"Saved: {autoData.lastSavedTime}";
        }
        else
        {
            autoSaveInfo.text = "No save data";
        }

        // Manual Save: Slot 1
        SaveData manualData = SaveLoadManager.Instance.LoadGame(1);

        if (manualData != null)
        {
            manualSaveInfo.text =
                $"Chapter: {manualData.chapter}\n" +
                $"Level: {manualData.playerLevel}\n" +
                $"Gold: {manualData.gold}\n" +
                $"Saved: {manualData.lastSavedTime}";
        }
        else
        {
            manualSaveInfo.text = "Empty Slot";
        }
    }
}
