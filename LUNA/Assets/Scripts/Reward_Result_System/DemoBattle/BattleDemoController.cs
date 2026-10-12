using TMPro;
using UnityEngine;

public class BattleDemoController : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField damageDealtInput;
    [SerializeField] private TMP_InputField damageReceivedInput;
    [SerializeField] private TMP_InputField killCountInput;
    [SerializeField] private TMP_InputField goldEarnedInput;

    [Header("Panels")]
    [SerializeField] private GameObject demoPanel;
    [SerializeField] private GameObject resultPanel;

    [Header("Systems")]
    [SerializeField] private ResultSystem resultSystem;
    [SerializeField] private ResultRewardUI resultRewardUI;

    public void EndBattleDemo()
    {
        // 1. LẤY DỮ LIỆU GIẢ LẬP

        int damageDealt = GetInputValue(damageDealtInput);
        int damageReceived = GetInputValue(damageReceivedInput);
        int killCount = GetInputValue(killCountInput);
        int goldEarned = GetInputValue(goldEarnedInput);

        // 2. TÍNH KẾT QUẢ

        resultSystem.CalculateResult(
            damageDealt,
            damageReceived,
            killCount,
            goldEarned
        );

        // 3. HIỂN THỊ RESULT + REWARD TRÊN CÙNG PANEL

        resultRewardUI.ShowResultAndReward(
            resultSystem.CurrentResult
        );

        demoPanel.SetActive(false);
        resultPanel.SetActive(true);
    }

    private int GetInputValue(TMP_InputField input)
    {
        int value;

        if (input != null &&
            int.TryParse(input.text, out value))
        {
            return Mathf.Max(0, value);
        }

        return 0;
    }
}