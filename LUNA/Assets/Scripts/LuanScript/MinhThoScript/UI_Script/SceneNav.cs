using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNav : MonoBehaviour
{
    [SerializeField] string menuScene = "MainMenu";

    public void GoBack()
    {
        if (Application.CanStreamedLevelBeLoaded(menuScene))
            SceneManager.LoadScene(menuScene);
        else
            Debug.Log("Chưa có scene \"" + menuScene + "\" trong Build Settings, nên chưa quay về được.");
    }
}