
using TMPro;
using UnityEngine;

public class MissionProgressUI : MonoBehaviour
{
    [Header("Mission Texts")]
    [SerializeField] private TMP_Text mission1Text;
    [SerializeField] private TMP_Text mission2Text;
    [SerializeField] private TMP_Text mission3Text;

    public void Refresh(
        MissionInfo mission1,
        MissionInfo mission2,
        MissionInfo mission3)
    {
        UpdateMissionText(mission1Text, mission1);
        UpdateMissionText(mission2Text, mission2);
        UpdateMissionText(mission3Text, mission3);
    }

    private void UpdateMissionText(
        TMP_Text text,
        MissionInfo mission)
    {
        if (text == null || mission == null)
            return;

        string status = mission.completed
            ? "✓ Hoàn thành"
            : "Đang thực hiện";

        text.text =
            $"{mission.title}\n" +
            $"{mission.description}\n" +
            $"Tiến trình: {mission.progress}/{mission.target}\n" +
            $"{status}";
    }
}
