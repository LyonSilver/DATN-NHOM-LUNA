
using System;
using UnityEngine;

public class IslandStageManager : MonoBehaviour
{
    [Header("Island Stages")]
    [SerializeField] IslandStage[] islands;

    public IslandStage[] Islands => islands;

    public IslandStage CurrentIsland { get; private set; }

    public int CurrentIndex { get; private set; } = -1;

    public int ClearedChain { get; private set; }

    public event Action Refreshed;

    private void Start()
    {
        Refresh();
    }

    // =====================================
    // REFRESH ISLAND MAP
    // =====================================

    [ContextMenu("Refresh Island Map")]
    public void Refresh()
    {
        if (islands == null || islands.Length == 0)
        {
            Debug.LogWarning(
                "IslandStageManager: Chua gan cac dao!"
            );

            return;
        }

        // Kiểm tra dữ liệu trước khi cập nhật.
        for (int i = 0; i < islands.Length; i++)
        {
            if (islands[i] == null ||
                islands[i].Data == null)
            {
                Debug.LogWarning(
                    "Island " + i + " thieu StageData!"
                );

                return;
            }
        }

        _SaveData save = _SaveSystem.ReloadFromDisk();

        bool prevCleared = true;
        bool currentAssigned = false;
        bool chainBroken = false;

        CurrentIsland = null;
        CurrentIndex = -1;
        ClearedChain = 0;

        for (int i = 0; i < islands.Length; i++)
        {
            IslandStage island = islands[i];
            StageData data = island.Data;

            StageProgress progress =
                save.Get(data.stageId);

            bool unlocked =
                prevCleared &&
                save.playerLevel >= data.requiredPlayerLevel;

            // Đảo đã hoàn thành.
            if (progress.cleared)
            {
                island.SetState(
                    IslandState.Cleared,
                    progress.bestStars
                );
            }

            // Đảo hiện tại.
            else if (unlocked && !currentAssigned)
            {
                island.SetState(
                    IslandState.Current,
                    0
                );

                CurrentIsland = island;
                CurrentIndex = i;

                currentAssigned = true;
            }

            // Đảo chưa mở khóa.
            else
            {
                island.SetState(
                    IslandState.Locked,
                    0
                );
            }

            if (!chainBroken && progress.cleared)
            {
                ClearedChain++;
            }
            else
            {
                chainBroken = true;
            }

            prevCleared = progress.cleared;
        }

        Debug.Log(
            "IslandStageManager: Player Level = " +
            save.playerLevel +
            " | Cleared = " +
            ClearedChain
        );

        Refreshed?.Invoke();
    }

    // =====================================
    // GET ISLAND
    // =====================================

    public IslandStage GetIsland(int index)
    {
        if (islands == null ||
            index < 0 ||
            index >= islands.Length)
        {
            return null;
        }

        return islands[index];
    }

    public bool IsIslandUnlocked(int index)
    {
        IslandStage island = GetIsland(index);

        return island != null && island.IsUnlocked;
    }
}
