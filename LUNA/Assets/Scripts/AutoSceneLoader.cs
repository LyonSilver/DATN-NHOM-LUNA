
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneName;

    public void LoadScene()
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Chưa nhập tên Scene!");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("Scene chưa được thêm vào Build Profiles: " + sceneName);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
