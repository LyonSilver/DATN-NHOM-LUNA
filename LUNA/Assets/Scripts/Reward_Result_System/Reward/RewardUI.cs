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

    public void ShowReward(
        int exp,
        int gold,
        int cardCount)
    {
        expText.text = "EXP: " + exp;
        goldText.text = "Gold: " + gold;

        for (int i = 0; i < cardImages.Length; i++)
        {
            cardImages[i].gameObject.SetActive(i < cardCount);
        }
    }
}