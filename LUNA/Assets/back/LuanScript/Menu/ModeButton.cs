using UnityEngine;
using TMPro;

public class ModeButton : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelMode;

    [Header("Mode Text")]
    [SerializeField] private TMP_Text modeText;

    public void OpenMode()
    {
        panelMode.SetActive(true);
    }

    public void SelectEasy()
    {
        SelectMode("Easy");
    }

    public void SelectNormal()
    {
        SelectMode("Normal");
    }

    public void SelectHard()
    {
        SelectMode("Hard");
    }

    private void SelectMode(string mode)
    {
        modeText.text = mode;
        panelMode.SetActive(false);
    }
}