using UnityEngine;

public class RewardResultManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject rewardPanel;

    [Header("Systems")]
    [SerializeField] private ResultSystem resultSystem;
    [SerializeField] private RewardSystem rewardSystem;

    [Header("UI")]
    [SerializeField] private RewardUI rewardUI;


    public void OpenReward()
    {
        // =====================================================
        // 1. LẤY KẾT QUẢ TRẬN
        // =====================================================

        BattleResultData result =
            resultSystem.CurrentResult;


        // =====================================================
        // 2. KIỂM TRA KẾT QUẢ
        // =====================================================

        if (result == null)
        {
            Debug.LogWarning(
                "Chưa có BattleResultData!"
            );

            return;
        }


        // =====================================================
        // 3. TÍNH PHẦN THƯỞNG
        // =====================================================

        rewardSystem.CalculateReward(result);


        // =====================================================
        // 4. HIỂN THỊ REWARD UI
        // =====================================================

        if (rewardUI != null)
        {
            rewardUI.ShowReward(
                rewardSystem.RewardCardCount
            );
        }
        else
        {
            Debug.LogError(
                "RewardResultManager: Chưa gán RewardUI!"
            );
        }


        // =====================================================
        // 5. TẮT RESULT PANEL
        // =====================================================

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }


        // =====================================================
        // 6. BẬT REWARD PANEL
        // =====================================================

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
        }
    }
}