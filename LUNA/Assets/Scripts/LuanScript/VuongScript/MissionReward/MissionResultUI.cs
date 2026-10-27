
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionResultUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject resultPanel;

    [Header("Result")]
    [SerializeField] private TMP_Text resultTitleText;

    [Header("Stars")]
    [SerializeField] private Image[] successStars;
    [SerializeField] private Image[] failStars;

    [Header("Mission Results")]
    [SerializeField] private TMP_Text mission1ResultText;
    [SerializeField] private TMP_Text mission2ResultText;
    [SerializeField] private TMP_Text mission3ResultText;

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

    private bool resultShown;

    private void Awake()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    public void ShowResult(
        MissionInfo mission1,
        MissionInfo mission2,
        MissionInfo mission3,
        MissionRewardData reward)
    {
        if (reward == null)
        {
            Debug.LogWarning(
                "MissionResultUI: Không có dữ liệu phần thưởng.");
            return;
        }

        if (resultShown)
            return;

        resultShown = true;

        if (resultPanel != null)
            resultPanel.SetActive(true);

        // 1. Tiêu đề kết quả
        if (resultTitleText != null)
        {
            resultTitleText.text = reward.victory
                ? "VICTORY"
                : "DEFEAT";
        }

        // 2. Hiển thị số sao theo nhiệm vụ
        UpdateStars(reward.stars);

        // 3. Hiển thị kết quả 3 nhiệm vụ
        SetMissionResult(mission1ResultText, mission1);
        SetMissionResult(mission2ResultText, mission2);
        SetMissionResult(mission3ResultText, mission3);

        // 4. Hiển thị phần thưởng theo kết quả nhiệm vụ
        bool hasExp = reward.victory && reward.exp > 0;
        bool hasGold = reward.victory && reward.gold > 0;
        bool hasCard = reward.victory && reward.cardCount > 0;

        UpdateRewardObjects(hasExp, hasGold, hasCard);

        if (expRewardText != null)
            expRewardText.text = "x" + reward.exp;

        if (goldRewardText != null)
            goldRewardText.text = "x" + reward.gold;

        if (cardRewardText != null)
            cardRewardText.text = "x" + reward.cardCount;

        // 5. Nút điều hướng
        UpdateButtons(reward.victory && reward.stars >= 2);

        Debug.Log(
            $"Mission Result | Victory: {reward.victory} | " +
            $"Stars: {reward.stars}/3 | " +
            $"EXP: {reward.exp} | Gold: {reward.gold} | " +
            $"Cards: {reward.cardCount}");
    }

    private void SetMissionResult(
        TMP_Text text,
        MissionInfo mission)
    {
        if (text == null || mission == null)
            return;

        text.text = mission.completed
            ? $"✓ {mission.title}"
            : $"✗ {mission.title}";
    }

    private void UpdateStars(int earnedStars)
    {
        earnedStars = Mathf.Clamp(earnedStars, 0, 3);

        for (int i = 0; i < 3; i++)
        {
            bool isEarned = i < earnedStars;

            if (successStars != null &&
                i < successStars.Length &&
                successStars[i] != null)
            {
                successStars[i].gameObject.SetActive(isEarned);
            }

            if (failStars != null &&
                i < failStars.Length &&
                failStars[i] != null)
            {
                failStars[i].gameObject.SetActive(!isEarned);
            }
        }
    }

    private void UpdateRewardObjects(
        bool showExp,
        bool showGold,
        bool showCard)
    {
        if (expRewardObject != null)
            expRewardObject.SetActive(showExp);

        if (goldRewardObject != null)
            goldRewardObject.SetActive(showGold);

        if (cardRewardObject != null)
            cardRewardObject.SetActive(showCard);

        if (expRewardText != null)
            expRewardText.gameObject.SetActive(showExp);

        if (goldRewardText != null)
            goldRewardText.gameObject.SetActive(showGold);

        if (cardRewardText != null)
            cardRewardText.gameObject.SetActive(showCard);
    }

    private void UpdateButtons(bool canContinue)
    {
        if (nextButton != null)
            nextButton.SetActive(canContinue);

        if (againButton != null)
            againButton.SetActive(!canContinue);

        if (homeButton != null)
            homeButton.SetActive(true);
    }

    public void HidePanel()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    public void ResetResult()
    {
        resultShown = false;

        if (resultPanel != null)
            resultPanel.SetActive(false);
    }
}
