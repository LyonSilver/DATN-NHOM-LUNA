
using UnityEngine;
using UnityEngine.SceneManagement;

public class AgainButton : MonoBehaviour
{
    // =====================================
    // RESTART BATTLE
    // =====================================

    public void RestartBattle()
    {
        Debug.Log("AgainButton: Chơi lại trận đấu!");

        // Khôi phục TimeScale vì ResultPanel
        // đã Pause game khi Player thua.
        Time.timeScale = 1f;

        // Lấy Scene hiện tại.
        Scene currentScene =
            SceneManager.GetActiveScene();

        // Load lại Scene từ đầu.
        SceneManager.LoadScene(currentScene.name);
    }
}
