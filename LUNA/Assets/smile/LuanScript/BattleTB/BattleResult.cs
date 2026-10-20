using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleResult : MonoBehaviour
{
    [Header("Scene bản đồ")]
    [SerializeField] private string mapSceneName = "LevelStageCanvas";

    public void ReturnToMap()
    {
        SceneManager.LoadScene(mapSceneName);
    }
}