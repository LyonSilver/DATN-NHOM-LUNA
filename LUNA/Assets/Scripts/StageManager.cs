using UnityEngine;

public class StageManager : MonoBehaviour
{
    public void OnStageComplete(int completedStageID)
    {
        SaveData saveData = SaveSystem.Load();

        int nextStageID = completedStageID + 1;
        if (!saveData.unlockedStageIDs.Contains(nextStageID))
        {
            saveData.unlockedStageIDs.Add(nextStageID);
        }

        SaveSystem.AutoSave(saveData, $"Qua màn {completedStageID}");
    }
}