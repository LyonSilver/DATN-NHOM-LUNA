using UnityEngine;
using UnityEngine.UI;

public class MouseSensitivitySettings : MonoBehaviour
{
    [SerializeField] private Slider sensitivitySlider;

    private const string SensitivityKey = "MouseSensitivity";

    public static float Sensitivity { get; private set; }

    private void Start()
    {
        float sensitivity = PlayerPrefs.GetFloat(SensitivityKey, 1f);

        Sensitivity = sensitivity;
        sensitivitySlider.value = sensitivity;

        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
    }

    private void SetSensitivity(float value)
    {
        Sensitivity = value;

        PlayerPrefs.SetFloat(SensitivityKey, value);
        PlayerPrefs.Save();
    }
}   