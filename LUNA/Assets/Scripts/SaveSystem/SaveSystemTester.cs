
using UnityEngine;

public class SaveSystemTester : MonoBehaviour
{
    [ContextMenu("Test Save")]
    public void TestSave()
    {
        SaveData data = new SaveData();
        data.chapter = "Chapter I";
        data.playerLevel = 1;
        data.gold = 500;
        data.ownedCardIds.Add("fire_card_01");
        data.deckCardIds.Add("fire_card_01");

        SaveLoadManager.Instance.SaveGame(data, 1);
        Debug.Log("Da test SAVE!");
    }

    [ContextMenu("Test Load")]
    public void TestLoad()
    {
        SaveData data = SaveLoadManager.Instance.LoadGame(1);

        if (data != null)
        {
            Debug.Log($"Chapter: {data.chapter}");
            Debug.Log($"Level: {data.playerLevel}");
            Debug.Log($"Gold: {data.gold}");
        }
    }
}
