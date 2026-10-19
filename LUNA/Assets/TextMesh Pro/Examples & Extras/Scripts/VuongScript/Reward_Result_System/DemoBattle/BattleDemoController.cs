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
    [SerializeField] private ResultUI resultUI;


    public void EndBattleDemo()
    {
        // 1. LẤY DỮ LIỆU TỪ INPUT FIELD

        int damageDealt = GetInputValue(damageDealtInput);
        int damageReceived = GetInputValue(damageReceivedInput);
        int killCount = GetInputValue(killCountInput);
        int goldEarned = GetInputValue(goldEarnedInput);


        // 2. GỬI DỮ LIỆU CHO RESULT SYSTEM

        resultSystem.CalculateResult(
            damageDealt,
            damageReceived,
            killCount,
            goldEarned
        );


        // 3. ĐÓNG DEMO PANEL
        demoPanel.SetActive(false);


        // 4. MỞ RESULT PANEL

        resultPanel.SetActive(true);


        // 5. HIỂN THỊ KẾT QUẢ

        resultUI.ShowResult(
            resultSystem.CurrentResult
        );
    }


    private int GetInputValue(TMP_InputField input)
    {
        int value;

        if (int.TryParse(input.text, out value))
        {
            // Không cho phép số âm từ InputField
            return Mathf.Max(0, value);
        }

        // Nếu InputField trống hoặc nhập sai
        // thì mặc định = 0
        return 0;
    }
}