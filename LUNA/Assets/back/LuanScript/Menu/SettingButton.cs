using UnityEngine;

public class SettingButton : MonoBehaviour
{
    [SerializeField] private GameObject panelMenu;
    [SerializeField] private GameObject panelSetting;

    public void OpenSetting()
    {
        panelMenu.SetActive(false);
        panelSetting.SetActive(true);
    }

    public void CloseSetting()
    {
        panelSetting.SetActive(false);
        panelMenu.SetActive(true);
    }
}