using UnityEngine;

public class ResetData : MonoBehaviour
{
    [Header("Settings")]
    public bool resetSlot2Unlock = true;
    public bool resetGold = true;
    public bool resetLevel = true;
    public bool resetCards = true;
    public bool resetAllSlots = false;

    [Header("Optional")]
    public SaveNotificationUI notificationUI;

    [ContextMenu("RESET DATA NOW")]
    public void DoReset()
    {
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("SaveLoadManager not found!");
            return;
        }

        SaveData newData = SaveLoadManager.Instance.CreateNewSaveData();
        newData.playerId = SaveLoadManager.Instance.CurrentPlayerId;

        if (!resetGold) newData.gold = GetOldGold();
        if (!resetLevel)
        {
            newData.playerLevel = GetOldLevel();
            newData.playerExp = GetOldExp();
        }

        if (!resetCards)
        {
            var old = SaveLoadManager.Instance.LoadGame(0);
            if (old != null)
            {
                newData.ownedCardIds = old.ownedCardIds;
                newData.deckCardIds = old.deckCardIds;
                newData.cardInstances = old.cardInstances;
                newData.unlockedCards = old.unlockedCards;
            }
        }

        if (!resetSlot2Unlock)
        {
            var old = SaveLoadManager.Instance.LoadGame(0);
            if (old != null)
                newData.isSlot2Unlocked = old.isSlot2Unlocked;
        }
        else
        {
            newData.isSlot2Unlocked = false;
        }

        bool success = SaveLoadManager.Instance.SaveGame(newData, 0);

        if (resetAllSlots)
        {
            SaveLoadManager.Instance.DeleteSave(1);
            SaveLoadManager.Instance.DeleteSave(2);
            SaveLoadManager.Instance.DeleteSave(3);
        }

        if (AutoSaveManager.Instance != null)
            AutoSaveManager.Instance.SetCurrentProgress(newData);

        if (Slot2Lock.Instance != null)
            Slot2Lock.Instance.RefreshUI();

        string msg = success ? "Data reset successfully!" : "Reset failed!";
        ShowNoti(msg);
        Debug.Log(msg);
    }

    private int GetOldGold()
    {
        var data = SaveLoadManager.Instance.LoadGame(0);
        return data != null ? data.gold : 0;
    }

    private int GetOldLevel()
    {
        var data = SaveLoadManager.Instance.LoadGame(0);
        return data != null ? data.playerLevel : 1;
    }

    private int GetOldExp()
    {
        var data = SaveLoadManager.Instance.LoadGame(0);
        return data != null ? data.playerExp : 0;
    }

    private void ShowNoti(string message)
    {
        if (notificationUI != null)
            notificationUI.Show(message);
        else if (SaveNotificationUI.Instance != null)
            SaveNotificationUI.Instance.Show(message);
        else
            Debug.Log(message);
    }
}