using UnityEngine;

public class StageSelectManager : MonoBehaviour
{
    [Header("Dữ liệu và đối tượng (Cùng số phần tử, cùng thứ tự)")]
    [SerializeField] StageData[] stages;
    [SerializeField] StageNode[] nodes;
    [SerializeField] PathBuilder2D path;

    public StageNode[] Nodes => nodes;
    public event System.Action Refreshed;

    void Start()
    {
        Refresh();
    }

    [ContextMenu("Refresh Map")]
    public void Refresh()
    {
        if (stages == null || nodes == null || stages.Length == 0 || stages.Length != nodes.Length)
        {
            Debug.LogWarning("StageSelectManager: Stages và Nodes phải có cùng số phần tử (5) và không được để trống.");
            return;
        }

        _SaveData save = _SaveSystem.ReloadFromDisk();

        bool prevCleared = true;
        bool currentAssigned = false;
        int clearedChain = 0;
        bool chainBroken = false;

        for (int i = 0; i < stages.Length; i++)
        {
            StageData data = stages[i];
            StageNode node = nodes[i];
            if (data == null || node == null)
            {
                Debug.LogWarning("StageSelectManager: Phần tử số " + i + " đang trống.");
                continue;
            }

            node.Setup(data);
            StageProgress p = save.Get(data.stageId);

            bool unlocked = prevCleared && save.playerLevel >= data.requiredPlayerLevel;

            if (p.cleared)
            {
                node.SetState(NodeState.Cleared, p.bestStars);
            }
            else if (unlocked && !currentAssigned)
            {
                node.SetState(NodeState.Current, 0);
                currentAssigned = true;
            }
            else
            {
                node.SetState(NodeState.Locked, 0);
            }

            if (!chainBroken && p.cleared) clearedChain++;
            else chainBroken = true;

            prevCleared = p.cleared;
        }

        int pathIndex = Mathf.Min(clearedChain, nodes.Length - 1);
        if (path != null) path.BuildWithCurrent(pathIndex);

        Debug.Log("StageSelectManager: Level " + save.playerLevel + ", thắng liên tiếp " + clearedChain + " Stage.");
        Refreshed?.Invoke();
    }
}