
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("Scene Settings")]
    [SerializeField] private string defaultSceneName = "SelectMap";

    private void Awake()
    {
        // Chỉ tồn tại một SceneTransition trong game.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Giữ GameObject khi chuyển Scene.
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    // Chuyển đến Scene mặc định trong Inspector.
    public void LoadScene()
    {
        LoadSceneByName(defaultSceneName);
    }

    // Chuyển Scene bằng tên.
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("SceneTransition: Chưa nhập tên Scene!");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"SceneTransition: Scene '{sceneName}' " +
                "chưa có trong Build Profiles!"
            );
            return;
        }

        // Tránh Scene mới bị đứng nếu Scene cũ đang Pause.
        Time.timeScale = 1f;

        SceneManager.LoadScene(sceneName);
    }

    // Tải lại Scene hiện tại.
    public void ReloadScene()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // Thoát game.
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
