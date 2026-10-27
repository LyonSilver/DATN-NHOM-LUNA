
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishingMiniGameManager : MonoBehaviour
{
    [Header("Timing System")]
    [SerializeField] private TimingManager timingManager;

    [Header("Fishing UI")]
    [SerializeField] private Button fishingButton;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text fragmentText;
    [SerializeField] private TMP_Text attemptsText;

    [Header("Fishing Settings")]
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 5f;

    [Header("Fishing Attempts")]
    [SerializeField] private int maxFishingAttempts = 5;

    [Header("Card Fragment Reward")]
    [SerializeField] private int goodReward = 1;
    [SerializeField] private int greatReward = 2;
    [SerializeField] private int perfectReward = 3;

    private enum FishingState
    {
        Idle,
        Waiting,
        Timing,
        Finished
    }

    private FishingState currentState = FishingState.Idle;

    private int totalFragments = 0;
    private int remainingAttempts;
    private Coroutine fishingCoroutine;

    private void Awake()
    {
        remainingAttempts = Mathf.Max(0, maxFishingAttempts);
    }

    private void OnEnable()
    {
        if (timingManager != null)
        {
            timingManager.OnTimingResult += HandleTimingResult;
        }
    }

    private void Start()
    {
        if (timingManager != null)
        {
            timingManager.StopTiming();
        }

        currentState = remainingAttempts > 0
            ? FishingState.Idle
            : FishingState.Finished;

        UpdateFragmentUI();
        UpdateAttemptsUI();

        if (remainingAttempts > 0)
        {
            SetStatus("Nhan Fishing de bat dau cau ca!");
        }
        else
        {
            SetStatus("Da het luot cau!");
        }

        UpdateFishingButton();
    }

    private void OnDisable()
    {
        if (timingManager != null)
        {
            timingManager.OnTimingResult -= HandleTimingResult;
            timingManager.StopTiming();
        }

        if (fishingCoroutine != null)
        {
            StopCoroutine(fishingCoroutine);
            fishingCoroutine = null;
        }

        if (currentState == FishingState.Waiting ||
            currentState == FishingState.Timing)
        {
            currentState = remainingAttempts > 0
                ? FishingState.Idle
                : FishingState.Finished;
        }
    }

    // Bat dau mot luot cau
    public void StartFishing()
    {
        if (currentState != FishingState.Idle)
            return;

        if (timingManager == null)
        {
            Debug.LogWarning("Chua gan TimingManager!");
            return;
        }

        if (remainingAttempts <= 0)
        {
            FinishFishing();
            return;
        }

        // Tru 1 luot ngay khi bat dau cau
        remainingAttempts--;

        UpdateAttemptsUI();

        currentState = FishingState.Waiting;
        UpdateFishingButton();

        SetStatus("Dang tha cau... Hay doi ca can!");

        fishingCoroutine = StartCoroutine(WaitForFish());
    }

    private IEnumerator WaitForFish()
    {
        float minTime = Mathf.Max(0f, minWaitTime);
        float maxTime = Mathf.Max(minTime, maxWaitTime);

        float waitTime = Random.Range(minTime, maxTime);

        yield return new WaitForSeconds(waitTime);

        fishingCoroutine = null;

        if (currentState != FishingState.Waiting)
            yield break;

        FishBite();
    }

    // Ca can cau
    private void FishBite()
    {
        currentState = FishingState.Timing;

        SetStatus("CA CAN CAU! Nhan SPACE de bat!");

        timingManager.StartTiming();
    }

    // Nhan ket qua tu TimingManager
    private void HandleTimingResult(
        TimingManager.TimingResult result,
        float multiplier)
    {
        if (currentState != FishingState.Timing)
            return;

        int reward = 0;
        string resultMessage = "";

        switch (result)
        {
            case TimingManager.TimingResult.MISS:
                reward = 0;
                resultMessage = "MISS! Ca da thoat!";
                break;

            case TimingManager.TimingResult.GOOD:
                reward = goodReward;
                resultMessage = "GOOD! Nhan " + reward + " manh the!";
                break;

            case TimingManager.TimingResult.GREAT:
                reward = greatReward;
                resultMessage = "GREAT! Nhan " + reward + " manh the!";
                break;

            case TimingManager.TimingResult.PERFECT:
                reward = perfectReward;
                resultMessage = "PERFECT! Nhan " + reward + " manh the!";
                break;
        }

        totalFragments += reward;

        UpdateFragmentUI();

        Debug.Log(
            $"Fishing Result: {result} | " +
            $"Reward: {reward} | " +
            $"Total: {totalFragments} | " +
            $"Remaining: {remainingAttempts}"
        );

        if (remainingAttempts <= 0)
        {
            FinishFishing(resultMessage);
        }
        else
        {
            currentState = FishingState.Idle;
            SetStatus(resultMessage);
            UpdateFishingButton();
        }
    }

    // Ket thuc khi het 5 luot
    private void FinishFishing(string lastResult = "")
    {
        currentState = FishingState.Finished;

        string message = "DA HET 5 LUOT CAU!\n" +
                         "Tong manh the: " + totalFragments;

        if (!string.IsNullOrEmpty(lastResult))
        {
            message = lastResult + "\n" + message;
        }

        SetStatus(message);
        UpdateFishingButton();

        Debug.Log(
            "Fishing Finished! Total Fragments: " + totalFragments
        );
    }

    // Cap nhat nut Fishing
    private void UpdateFishingButton()
    {
        if (fishingButton != null)
        {
            fishingButton.interactable =
                currentState == FishingState.Idle &&
                remainingAttempts > 0;
        }
    }

    // Cap nhat so manh the
    private void UpdateFragmentUI()
    {
        if (fragmentText != null)
        {
            fragmentText.text =
                "Card Fragments: " + totalFragments;
        }
    }

    // Cap nhat so luot cau
    private void UpdateAttemptsUI()
    {
        if (attemptsText != null)
        {
            attemptsText.text =
                "Luot cau: " + remainingAttempts +
                "/" + Mathf.Max(0, maxFishingAttempts);
        }
    }

    // Cap nhat trang thai
    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
