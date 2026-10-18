using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slot2Lock : MonoBehaviour
{
    public static Slot2Lock Instance { get; private set; }

    [Header("Unlock Settings")]
    public int unlockCost = 500;
    public string currencyName = "Gold";

    [Header("UI - Slot 2")]
    public Button slot2Button;
    public GameObject lockIcon;
    public TMP_Text costText;
    public Button unlockButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
{
    bool unlocked = IsUnlocked();

    if (slot2Button != null)
        slot2Button.interactable = unlocked;

    if (lockIcon != null)
        lockIcon.SetActive(!unlocked);

    // Đổi chỗ này
    if (costText != null)
        costText.text = unlocked ? "SAVE" : $"{unlockCost} {currencyName}";

    if (unlockButton != null)
        unlockButton.gameObject.SetActive(!unlocked);
}

    public bool IsUnlocked()
    {
        SaveData data = GetData();
        return data != null && data.isSlot2Unlocked;
    }

    public bool CanUseSlot2()
    {
        return IsUnlocked();
    }

    public void TryUnlock()
    {
        SaveData data = GetData();
        if (data == null) return;

        if (data.isSlot2Unlocked)
        {
            ShowNoti("Slot 2 is already unlocked!");
            RefreshUI();
            return;
        }

        if (data.gold < unlockCost)
        {
            ShowNoti($"Not enough {currencyName}! Need {unlockCost}");
            return;
        }

        data.gold -= unlockCost;
        data.isSlot2Unlocked = true;

        bool success = SaveLoadManager.Instance.SaveGame(data, 0);

        if (success)
        {
            ShowNoti("Slot 2 unlocked successfully!");
            RefreshUI();

            if (AutoSaveManager.Instance != null)
                AutoSaveManager.Instance.SetCurrentProgress(data);
        }
        else
        {
            ShowNoti("Failed to save data!");
        }
    }

    private SaveData GetData()
    {
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("SaveLoadManager not found!");
            return null;
        }

        SaveData data = SaveLoadManager.Instance.LoadGame(0);
        if (data == null)
            data = SaveLoadManager.Instance.CreateNewSaveData();

        return data;
    }

    private void ShowNoti(string message)
    {
        if (SaveNotificationUI.Instance != null)
            SaveNotificationUI.Instance.Show(message);
        else
            Debug.Log(message);
    }
}