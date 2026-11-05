using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class _EnemyCombatDisplay : MonoBehaviour
{
    public _EnemyCombatState enemyCombat;
    public CardEffectManager cardEffectManager; // tùy chọn: để hiện thêm Poison/Break
    public TMP_Text statusText;
    public float refreshInterval = 0.2f;

    private float timer;
    private string lastText;

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer > 0f)
        {
            return;
        }

        timer = refreshInterval;
        Refresh();
    }

    private void Refresh()
    {
        if (enemyCombat == null || statusText == null)
        {
            return;
        }

        List<string> parts = new List<string>();
        enemyCombat.AppendStatusParts(parts);

        if (cardEffectManager != null)
        {
            if (cardEffectManager.EnemyPoisonTurns > 0)
            {
                parts.Add("Poison " + cardEffectManager.EnemyPoisonTurns);
            }

            if (cardEffectManager.EnemyBreakTurns > 0)
            {
                parts.Add("Break " + cardEffectManager.EnemyBreakTurns);
            }
        }

        string elementHex = ColorUtility.ToHtmlStringRGB(ElementRules.GetColor(enemyCombat.Element));
        string status = parts.Count == 0 ? "Status: none" : "Status: " + string.Join(" | ", parts);

        string text = "ELEMENT: <color=#" + elementHex + ">" +
                      ElementRules.GetName(enemyCombat.Element) + "</color>\n" + status;

        if (text != lastText)
        {
            lastText = text;
            statusText.text = text;
        }
    }
}