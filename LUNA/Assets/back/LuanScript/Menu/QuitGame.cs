using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("QUIT BUTTON CLICKED");
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
}