
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingButton : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject panelMenu;
    [SerializeField] private GameObject panelSetting;

    [Header("Scene")]
    [SerializeField] private string backSceneName = "MapBT";

    private bool isPaused;

    private void Start()
    {
        Time.timeScale = 1f;

        isPaused = false;

        if (panelSetting != null)
            panelSetting.SetActive(false);
    }

    // =====================================
    // PAUSE GAME
    // =====================================

    public void OpenSetting()
    {
        if (isPaused || panelSetting == null)
            return;

        if (panelMenu != null)
            panelMenu.SetActive(false);

        panelSetting.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;

        Debug.Log("Game Paused!");
    }

    // =====================================
    // CONTINUE GAME
    // =====================================

    public void ContinueGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (panelSetting != null)
            panelSetting.SetActive(false);

        if (panelMenu != null)
            panelMenu.SetActive(true);

        Debug.Log("Game Continued!");
    }

    // Giữ hàm cũ để không mất liên kết Button.
    public void CloseSetting()
    {
        ContinueGame();
    }

    // =====================================
    // BACK TO SCENE
    // =====================================

    public void BackToScene()
    {
        if (string.IsNullOrWhiteSpace(backSceneName) ||
            !Application.CanStreamedLevelBeLoaded(backSceneName))
        {
            Debug.LogError(
                "Scene chưa có trong Build Profiles: " +
                backSceneName
            );
            return;
        }

        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene(backSceneName);
    }
}
