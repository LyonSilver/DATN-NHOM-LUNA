using UnityEngine;
using TMPro;

public class BattleHUDDisplay : MonoBehaviour
{
    public BattleManager battleManager;
    public TMP_Text enemyStatusText;
    public TMP_Text playerStatusText;

    private void OnEnable()
    {
        if (battleManager != null)
        {
            battleManager.OnBattleStateChanged += UpdateTexts;
        }
    }

    private void OnDisable()
    {
        if (battleManager != null)
        {
            battleManager.OnBattleStateChanged -= UpdateTexts;
        }
    }

    private void Start()
    {
        UpdateTexts();
    }

    private void UpdateTexts()
    {
        if (battleManager == null) return;

        if (enemyStatusText != null)
        {
            string enemyLine = "ENEMY HP: " + battleManager.enemyHP + " / " + battleManager.enemyMaxHP;
            if (battleManager.enemyShield > 0) enemyLine += "   SHIELD: " + battleManager.enemyShield;
            if (battleManager.enemyBroken) enemyLine += "   [BROKEN]";
            if (battleManager.enemyPoisonTurns > 0) enemyLine += "   POISON: " + battleManager.enemyPoisonTurns + " turn(s)";
            enemyStatusText.text = enemyLine;
        }

        if (playerStatusText != null)
        {
            playerStatusText.text = "PLAYER HP: " + battleManager.playerHP + " / " + battleManager.playerMaxHP +
                                     "   SHIELD: " + battleManager.playerShield;
        }
    }
}