using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : MonoBehaviour
{
    [Header("Round Manager")]
    [SerializeField] private RoundManager roundManager;

    [Header("Texts")]
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text goldText;

    [Header("Cards")]
    [SerializeField] private Image[] cardImages;

    [Header("Random Card Pool")]
    [SerializeField] private Sprite[] cardPool;


    // =========================================================
    // SHOW REWARD
    // Lấy tổng Coin và EXP từ RoundManager
    // =========================================================

    public void ShowReward(int cardCount)
    {
        if (roundManager == null)
        {
            Debug.LogError(
                "RewardUI: Chưa gán RoundManager!"
            );

            return;
        }


        // =====================================================
        // LẤY TỔNG EXP
        // =====================================================

        int totalExp = roundManager.GetCurrentExp();


        // =====================================================
        // LẤY TỔNG COIN
        // =====================================================

        int totalCoin = roundManager.GetCurrentCoin();


        // =====================================================
        // HIỂN THỊ EXP
        // =====================================================

        if (expText != null)
        {
            expText.text =
                "EXP: " +
                totalExp;
        }


        // =====================================================
        // HIỂN THỊ COIN
        // =====================================================

        if (goldText != null)
        {
            goldText.text =
                "Coin: " +
                totalCoin;
        }


        // =====================================================
        // HIỂN THỊ CARD
        // =====================================================

        for (int i = 0; i < cardImages.Length; i++)
        {
            if (i < cardCount)
            {
                // Bật ô card
                cardImages[i].gameObject.SetActive(true);

                // Random một card
                Sprite randomCard = GetRandomCard();

                // Gán hình card
                cardImages[i].sprite = randomCard;
            }
            else
            {
                // Tắt ô card không sử dụng
                cardImages[i].gameObject.SetActive(false);
            }
        }


        Debug.Log(
            "RewardUI: Tổng Reward = " +
            totalCoin +
            " Coin / " +
            totalExp +
            " EXP"
        );
    }


    // =========================================================
    // RANDOM CARD
    // =========================================================

    private Sprite GetRandomCard()
    {
        // Nếu chưa có card nào trong pool
        if (cardPool == null || cardPool.Length == 0)
        {
            Debug.LogWarning(
                "RewardUI: Card Pool đang trống!"
            );

            return null;
        }


        // Random vị trí card
        int randomIndex =
            Random.Range(0, cardPool.Length);


        return cardPool[randomIndex];
    }
}