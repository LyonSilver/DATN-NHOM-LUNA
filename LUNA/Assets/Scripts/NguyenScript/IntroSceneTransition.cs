
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroSceneTransition : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "GameScene";
    [SerializeField] private float introDuration = 5f;

    private void Start()
    {
        StartCoroutine(WaitForIntro());
    }

    private IEnumerator WaitForIntro()
    {
        yield return new WaitForSeconds(introDuration);

        SceneManager.LoadScene(nextSceneName);
    }
}