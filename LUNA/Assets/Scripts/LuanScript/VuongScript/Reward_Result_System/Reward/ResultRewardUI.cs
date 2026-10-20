
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultRewardUI : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private RewardSystem rewardSystem;
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private BattleResultTracker battleStats;

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

    // =========================================
    // DIRECT INTEGRATION WITH ROUND MANAGER
    // =========================================

    public void SetBattleOutcome(bool isVictory)
    {
        if (roundManager == null)
        {
            Debug.LogError(
                "ResultRewardUI: Chưa gán RoundManager!"
            );
            return;
        }

        if (battleStats == null)
        {
            Debug.LogError(
                "ResultRewardUI: Chưa gán BattleResultTracker!"
            );
            return;
        }

        // =====================================
        // 1. BATTLE STATISTICS
        // =====================================

        if (damageDealtText != null)
        {
            damageDealtText.text =
                battleStats.DamageDealt.ToString("N0");
        }

        if (damageReceivedText != null)
        {
            damageReceivedText.text =
                battleStats.DamageReceived.ToString("N0");
        }

        if (killCountText != null)
        {
            killCountText.text =
                battleStats.KillCount.ToString("N0");
        }

        int goldCollected = roundManager.GetCurrentCoin();
        int expCollected = roundManager.GetCurrentExp();

        if (goldEarnedText != null)
        {
            goldEarnedText.text =
                goldCollected.ToString("N0");
        }

        // Chưa có công thức Score của BattleResultData.
        if (scoreText != null)
        {
            scoreText.text = "- / 600";
        }

        // =====================================
        // 2. TEMPORARY STAR DISPLAY
        // =====================================

        // Tạm thời thắng = 2 sao, thua = 0 sao.
        // Sau này thay bằng kết quả chấm sao thật.
        int stars = isVictory ? 2 : 0;

        UpdateStars(stars);

        // =====================================
        // 3. TEMPORARY REWARD DISPLAY
        // =====================================

        // Đây là phần thưởng RoundManager đã ghi nhận.
        // Chưa tự cộng vào tài khoản hay Inventory.
        bool showRewards = isVictory;

        UpdateRewardObjects(
            showRewards,
            showRewards,
            false
        );

        if (expRewardText != null)
        {
            expRewardText.text =
                "x" + (isVictory ? expCollected : 0);
        }

        if (goldRewardText != null)
        {
            goldRewardText.text =
                "x" + (isVictory ? goldCollected : 0);
        }

        if (cardRewardText != null)
        {
            cardRewardText.text = "x0";
        }

        // =====================================
        // 4. RESULT BUTTONS
        // =====================================

        UpdateButtons(isVictory);

        Debug.Log(
            $"Result: {(isVictory ? "WIN" : "LOSE")} | " +
            $"Damage Dealt: {battleStats.DamageDealt} | " +
            $"Damage Received: {battleStats.DamageReceived} | " +
            $"Kills: {battleStats.KillCount} | " +
            $"Gold: {goldCollected} | " +
            $"EXP: {expCollected}"
        );
    }

    // =========================================
    // ORIGINAL RESULT DATA / REWARD SYSTEM
    // =========================================

    public void ShowResultAndReward(BattleResultData result)
    {
        if (result == null)
        {
            Debug.LogWarning(
                "Không có dữ liệu kết quả trận đấu!"
            );
            return;
        }

        if (rewardSystem == null)
        {
            Debug.LogError(
                "ResultRewardUI: Chưa gán RewardSystem!"
            );
            return;
        }

        // =====================================
        // 1. DISPLAY STATISTICS
        // =====================================

        if (damageDealtText != null)
        {
            damageDealtText.text =
                result.damageDealt.ToString("N0");
        }

        if (damageReceivedText != null)
        {
            damageReceivedText.text =
                result.damageReceived.ToString("N0");
        }

        if (killCountText != null)
        {
            killCountText.text =
                result.killCount.ToString("N0");
        }

        if (goldEarnedText != null)
        {
            goldEarnedText.text =
                result.goldEarned.ToString("N0");
        }

        if (scoreText != null)
        {
            scoreText.text = result.score + " / 600";
        }

        // =====================================
        // 2. STARS
        // =====================================

        UpdateStars(result.star);

        // =====================================
        // 3. CALCULATE REWARD
        // =====================================

        rewardSystem.CalculateReward(result);

        int exp = rewardSystem.RewardExp;
        int gold = rewardSystem.RewardGold;
        int cards = rewardSystem.RewardCardCount;

        bool hasExp = result.star > 0;
        bool hasGold = result.star >= 2;
        bool hasCard = result.star >= 2;

        UpdateRewardObjects(
            hasExp,
            hasGold,
            hasCard
        );

        if (expRewardText != null)
        {
            expRewardText.text = "x" + exp;
        }

        if (goldRewardText != null)
        {
            goldRewardText.text = "x" + gold;
        }

        if (cardRewardText != null)
        {
            cardRewardText.text = "x" + cards;
        }

        // =====================================
        // 4. BUTTONS
        // =====================================

        UpdateButtons(result.star >= 2);
    }

    // =========================================
    // UPDATE STARS
    // =========================================

    private void UpdateStars(int earnedStars)
    {
        earnedStars = Mathf.Clamp(
            earnedStars,
            0,
            3
        );

        for (int i = 0; i < 3; i++)
        {
            bool isEarned = i < earnedStars;

            if (successStars != null &&
                i < successStars.Length &&
                successStars[i] != null)
            {
                successStars[i].gameObject.SetActive(
                    isEarned
                );
            }

            if (failStars != null &&
                i < failStars.Length &&
                failStars[i] != null)
            {
                failStars[i].gameObject.SetActive(
                    !isEarned
                );
            }
        }
    }

    // =========================================
    // UPDATE REWARD OBJECTS
    // =========================================

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

    // =========================================
    // UPDATE BUTTONS
    // =========================================

    private void UpdateButtons(bool canContinue)
    {
        if (nextButton != null)
            nextButton.SetActive(canContinue);

        if (againButton != null)
            againButton.SetActive(!canContinue);

        if (homeButton != null)
            homeButton.SetActive(true);
    }
}