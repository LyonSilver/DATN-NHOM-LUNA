using UnityEngine;
using TMPro;

public class EnergyDisplay : MonoBehaviour
{
    public TurnManager turnManager;
    public TMP_Text energyText;

    private void OnEnable()
    {
        if (turnManager != null)
        {
            turnManager.OnEnergyChanged += UpdateText;
        }
    }

    private void OnDisable()
    {
        if (turnManager != null)
        {
            turnManager.OnEnergyChanged -= UpdateText;
        }
    }

    private void Start()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (turnManager == null || energyText == null) return;
        energyText.text = "ENERGY: " + turnManager.currentEnergy + " / " + turnManager.maxEnergy;
    }
}