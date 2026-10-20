using UnityEngine;
using TMPro;

public class StageSelectHud : MonoBehaviour
{
    [SerializeField] StageSelectManager manager;
    [SerializeField] TMP_Text hudText;
    [SerializeField] int maxStars = 15;

    void OnEnable()
    {
        if (manager != null) manager.Refreshed += UpdateHud;
    }

    void OnDisable()
    {
        if (manager != null) manager.Refreshed -= UpdateHud;
    }

    void Start()
    {
        UpdateHud();
    }

    public void UpdateHud()
    {
        if (hudText == null) return;
        _SaveData d = _SaveSystem.Load();
        int total = 0;
        foreach (StageProgress p in d.stages) total += p.bestStars;
        int need = 50 * d.playerLevel;
        hudText.text = "Lv. " + d.playerLevel + "     EXP " + d.exp + "/" + need
                     + "     Coin " + d.coin + "     Sao " + total + "/" + maxStars;
    }
}