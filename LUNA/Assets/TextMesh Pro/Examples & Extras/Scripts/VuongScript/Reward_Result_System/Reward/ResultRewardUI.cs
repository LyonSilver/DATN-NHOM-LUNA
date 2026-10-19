using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultRewardUI : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private RewardSystem rewardSystem;

    [Header("Result Text")]
    [SerializeField] private TMP_Text damageDealtText;
    [SerializeField] private TMP_Text damageReceivedText;
    [SerializeField] private TMP_Text killCountText;
    [SerializeField] private TMP_Text goldEarnedText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Stars")]
    [SerializeField] private Image[] successStars;
    [SerializeField] private Image[] failStars;

    [Header("Reward Objects")]
    [SerializeField] private GameObject expRewardObject;
    [SerializeField] private GameObject goldRewardObject;
    [SerializeField] private GameObject cardRewardObject;

    [Header("Reward Amount Text")]
    [SerializeField] private TMP_Text expRewardText;
    [SerializeField] private TMP_Text goldRewardText;
    [SerializeField] private TMP_Text cardRewardText;

    [Header("Buttons")]
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject againButton;
    [SerializeField] private GameObject homeButton;

    public void ShowResultAndReward(BattleResultData result)
    {
        if (result == null)
        {
            Debug.LogWarning("Khong co du lieu ket qua tran dau!");
            return;
        }

        if (rewardSystem == null)
        {
            Debug.LogError("Chua gan RewardSystem!");
            return;
        }

        // 1. HIEN THI CHI SO TRAN DAU

        if (damageDealtText != null)
            damageDealtText.text = result.damageDealt.ToString("N0");

        if (damageReceivedText != null)
            damageReceivedText.text = result.damageReceived.ToString("N0");

        if (killCountText != null)
            killCountText.text = result.killCount.ToString("N0");

        if (goldEarnedText != null)
            goldEarnedText.text = result.goldEarned.ToString("N0");

        if (scoreText != null)
            scoreText.text = result.score + " / 600";

        // 2. HIEN THI SAO

        for (int i = 0; i < 3; i++)
        {
            bool earnedStar = i < result.star;

            if (successStars != null && i < successStars.Length
                && successStars[i] != null)
            {
                successStars[i].gameObject.SetActive(earnedStar);
            }

            if (failStars != null && i < failStars.Length
                && failStars[i] != null)
            {
                failStars[i].gameObject.SetActive(!earnedStar);
            }
        }

        // 3. TINH PHAN THUONG

        rewardSystem.CalculateReward(result);

        int exp = rewardSystem.RewardExp;
        int gold = rewardSystem.RewardGold;
        int cards = rewardSystem.RewardCardCount;

       // 4. HIEN THI KHUNG VA TEXT PHAN THUONG

        bool hasReward = result.star > 0;

        if (expRewardObject != null)
            expRewardObject.SetActive(hasReward);

        if (goldRewardObject != null)
            goldRewardObject.SetActive(result.star >= 2);

        if (cardRewardObject != null)
            cardRewardObject.SetActive(result.star >= 2);

        if (expRewardText != null)
            expRewardText.gameObject.SetActive(hasReward);

        if (goldRewardText != null)
            goldRewardText.gameObject.SetActive(result.star >= 2);

        if (cardRewardText != null)
            cardRewardText.gameObject.SetActive(result.star >= 2);

        if (expRewardText != null)
            expRewardText.text =
                "x" + rewardSystem.RewardExp;

        if (goldRewardText != null)
            goldRewardText.text =
                "x" + rewardSystem.RewardGold;

        if (cardRewardText != null)
            cardRewardText.text =
                "x" + rewardSystem.RewardCardCount;

        // 5. HIEN THI BUTTON

        bool canContinue = result.star >= 2;

        if (nextButton != null)
            nextButton.SetActive(canContinue);

        if (againButton != null)
            againButton.SetActive(!canContinue);

        if (homeButton != null)
            homeButton.SetActive(true);
    }
}