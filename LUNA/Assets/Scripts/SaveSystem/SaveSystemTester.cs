using UnityEngine;

public class SaveSystemTester : MonoBehaviour
{
    [Header("Test Data")]
    public int testLevel = 5;
    public int testGold = 1500;
    public string testStageId = "stage_03";

    // =========================================================
    // Method này được Editor Tool (ChaosCardSaveButtonRepairTool) gọi
    // =========================================================
    public void TestSave()
    {
        CreateAndSaveTestData();
    }

    [ContextMenu("1. Create & Save Test Data (Auto Slot)")]
    public void CreateAndSaveTestData()
    {
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("Không tìm thấy SaveLoadManager!");
            return;
        }

        // Đảm bảo có Player ID
        if (string.IsNullOrEmpty(SaveLoadManager.Instance.CurrentPlayerId))
            SaveLoadManager.Instance.SetPlayerId("test_player_001");

        SaveData data = SaveLoadManager.Instance.CreateNewSaveData();

        // ===== Điền dữ liệu test =====
        data.playerLevel = testLevel;
        data.gold = testGold;
        data.chapter = 2;
        data.currentStageId = testStageId;
        data.matchesPlayed = 10;
        data.matchesWon = 7;
        data.matchesLost = 3;

        // Unlock
        data.unlockedCards.Add("card_fire_01");
        data.unlockedCards.Add("card_water_02");
        data.unlockedStages.Add("stage_01");
        data.unlockedStages.Add("stage_02");

        // Collection + Deck
        data.ownedCardIds.Add("card_fire_01");
        data.ownedCardIds.Add("card_water_02");
        data.ownedCardIds.Add("card_earth_03");

        data.deckCardIds.Add("card_fire_01");
        data.deckCardIds.Add("card_water_02");
        data.deckCardIds.Add("card_earth_03");

        // Lưu vào Auto Save (slot 0)
        bool success = SaveLoadManager.Instance.SaveGame(data, 0);

        if (success)
            Debug.Log("<color=green>Đã tạo + Save test data thành công!</color>");
        else
            Debug.LogError("Save thất bại!");
    }

    [ContextMenu("2. Load Auto Save & In ra Console")]
    public void LoadAndPrint()
    {
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("Không tìm thấy SaveLoadManager!");
            return;
        }

        SaveData data = SaveLoadManager.Instance.LoadGame(0);

        if (data == null)
        {
            Debug.LogWarning("Không có dữ liệu Auto Save!");
            return;
        }

        Debug.Log($"=== AUTO SAVE ===\n" +
                  $"Level: {data.playerLevel}\n" +
                  $"Gold: {data.gold}\n" +
                  $"Chapter: {data.chapter}\n" +
                  $"Stage: {data.currentStageId}\n" +
                  $"Win/Lose: {data.matchesWon}/{data.matchesLost}\n" +
                  $"Deck cards: {data.deckCardIds.Count}\n" +
                  $"Owned cards: {data.ownedCardIds.Count}\n" +
                  $"Saved at: {data.lastSavedTime}");
    }

    [ContextMenu("3. Manual Save (Slot 1)")]
    public void ManualSaveTest()
    {
        if (AutoSaveManager.Instance != null)
            AutoSaveManager.Instance.ManualSave(1);
        else
            Debug.LogError("Không tìm thấy AutoSaveManager");
    }

    [ContextMenu("4. Delete All Saves")]
    public void DeleteAll()
    {
        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.DeleteAllSavesOfCurrentPlayer();
    }
}