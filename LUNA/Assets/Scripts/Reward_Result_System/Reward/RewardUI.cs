using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text goldText;

    [Header("Cards")]
    [SerializeField] private Image[] cardImages;

    [Header("Random Card Pool")]
    [SerializeField] private Sprite[] cardPool;


    public void ShowReward(
        int exp,
        int gold,
        int cardCount)
    {
        // 1. HIỂN THỊ EXP

        expText.text = "EXP: " + exp;


        // 2. HIỂN THỊ GOLD

        goldText.text = "Gold: " + gold;


        // 3. HIỂN THỊ CARD

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
    }


    private Sprite GetRandomCard()
    {
        // Nếu chưa có card nào trong pool
        if (cardPool == null || cardPool.Length == 0)
        {
            Debug.LogWarning("Card Pool đang trống!");
            return null;
        }

        // Random vị trí card
        int randomIndex = Random.Range(0, cardPool.Length);

        return cardPool[randomIndex];
    }
}