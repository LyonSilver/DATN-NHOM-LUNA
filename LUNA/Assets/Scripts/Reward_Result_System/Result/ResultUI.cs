using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [Header("Result Text")]
    [SerializeField] private TMP_Text damageDealtText;
    [SerializeField] private TMP_Text damageReceivedText;
    [SerializeField] private TMP_Text killCountText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Result Title")]
    [SerializeField] private TMP_Text resultTitleText;

    [Header("Stars")]
    [SerializeField] private Image[] starImages;

    [Header("Buttons")]
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject againButton;

    [Header("Star Colors")]
    [SerializeField] private Color emptyStarColor = Color.white;
    [SerializeField] private Color activeStarColor = Color.yellow;

    public void ShowResult(BattleResultData result)
    {
        // HIỂN THỊ CHỈ SỐ

        damageDealtText.text =
            "Damage gây ra: " + result.damageDealt;

        damageReceivedText.text =
            "Damage nhận: " + result.damageReceived;

        killCountText.text =
            "Số quái giết: " + result.killCount;

        goldText.text =
            "Tổng Gold trong trận: " + result.goldEarned;

        scoreText.text =
            "Score: " + result.score;


        // HIỂN THỊ VICTORY / DEFEAT

        if (result.victory)
        {
            resultTitleText.text = "VICTORY";
        }
        else
        {
            resultTitleText.text = "DEFEAT";
        }


        // HIỂN THỊ SỐ SAO

        ShowStars(result.star);


        // HIỂN THỊ BUTTON

        if (result.star >= 2)
        {
            nextButton.SetActive(true);
            againButton.SetActive(false);
        }
        else
        {
            nextButton.SetActive(false);
            againButton.SetActive(true);
        }
    }


    private void ShowStars(int starCount)
    {
        for (int i = 0; i < starImages.Length; i++)
        {
            if (i < starCount)
            {
                starImages[i].color = activeStarColor;
            }
            else
            {
                starImages[i].color = emptyStarColor;
            }
        }
    }
}