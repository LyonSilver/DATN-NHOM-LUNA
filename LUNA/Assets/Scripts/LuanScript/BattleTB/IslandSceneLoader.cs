using UnityEngine;
using UnityEngine.SceneManagement;

public class IslandSceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneName;

    public void GoToScene()
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Chưa nhập tên Scene!", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                "Không tìm thấy Scene: " + sceneName +
                ". Hãy kiểm tra tên Scene và Build Profiles.",
                this
            );
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}