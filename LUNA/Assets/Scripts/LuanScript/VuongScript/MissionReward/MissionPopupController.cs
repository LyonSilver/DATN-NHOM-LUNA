using UnityEngine;

public class MissionPopupController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject missionButton;
    [SerializeField] private GameObject missionProgressPanel;
    [SerializeField] private GameObject missionModalOverlay;

    private bool isPopupOpen = false;
    private bool isResultShown = false;

    private void Start()
    {
        CloseMissionPopup();
        isResultShown = false;

        if (missionButton != null)
            missionButton.SetActive(true);
    }

    // Gắn hàm này vào OnClick của MissionButton
    public void ToggleMissionPopup()
    {
        if (isResultShown)
            return;

        if (isPopupOpen)
            CloseMissionPopup();
        else
            OpenMissionPopup();
    }

    private void OpenMissionPopup()
    {
        isPopupOpen = true;

        if (missionModalOverlay != null)
            missionModalOverlay.SetActive(true);

        if (missionProgressPanel != null)
            missionProgressPanel.SetActive(true);
    }

    // Hàm đóng popup
    public void CloseMissionPopup()
    {
        isPopupOpen = false;

        if (missionProgressPanel != null)
            missionProgressPanel.SetActive(false);

        if (missionModalOverlay != null)
            missionModalOverlay.SetActive(false);
    }

    // Gọi khi ResultPanel xuất hiện
    public void OnResultShown()
    {
        isResultShown = true;
        CloseMissionPopup();

        if (missionButton != null)
            missionButton.SetActive(false);
    }
}