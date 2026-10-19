
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AutoSceneTimer : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "Lobby";

    [Header("Timer Settings")]
    [SerializeField] private float delaySeconds = 10f;

    [SerializeField] private bool useUnscaledTime = true;

    private void Start()
    {
        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(delaySeconds);
        else
            yield return new WaitForSeconds(delaySeconds);

        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError(
                $"Scene '{nextSceneName}' chưa được thêm vào Build Profiles!"
            );
        }
    }
}
