
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishingMiniGameManager : MonoBehaviour
{
    [Header("Fishing Tracking System")]
    [SerializeField] private FishingTrackingBar fishingTrackingBar;

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
    [SerializeField] private int fragmentsPerCatch = 3;

    private enum FishingState
    {
        Idle,
        Waiting,
        Tracking,
        Finished
    }

    private FishingState currentState = FishingState.Idle;

    private int totalFragments = 0;
    private int remainingAttempts;

    private Coroutine fishingCoroutine;

    // ===================================
    // KHOI TAO
    // ===================================

    private void Awake()
    {
        remainingAttempts = Mathf.Max(0, maxFishingAttempts);
    }

    private void OnEnable()
    {
        if (fishingTrackingBar != null)
        {
            fishingTrackingBar.OnFishingFinished += HandleFishingResult;
        }
    }

    private void Start()
    {
        if (fishingTrackingBar != null)
        {
            fishingTrackingBar.StopFishing();
        }

        currentState = remainingAttempts > 0
            ? FishingState.Idle
            : FishingState.Finished;

        UpdateFragmentUI();
        UpdateAttemptsUI();
        UpdateFishingButton();

        if (remainingAttempts > 0)
        {
            SetStatus("Nhan Fishing de bat dau cau ca!");
        }
        else
        {
            SetStatus("Da het luot cau!");
        }
    }

    private void OnDisable()
    {
        if (fishingTrackingBar != null)
        {
            fishingTrackingBar.OnFishingFinished -= HandleFishingResult;
            fishingTrackingBar.StopFishing();
        }

        if (fishingCoroutine != null)
        {
            StopCoroutine(fishingCoroutine);
            fishingCoroutine = null;
        }

        if (currentState == FishingState.Waiting ||
            currentState == FishingState.Tracking)
        {
            currentState = remainingAttempts > 0
                ? FishingState.Idle
                : FishingState.Finished;
        }
    }

    // ===================================
    // BAT DAU CAU CA
    // ===================================

    public void StartFishing()
    {
        if (currentState != FishingState.Idle)
            return;

        if (fishingTrackingBar == null)
        {
            Debug.LogWarning(
                "FishingMiniGameManager: Chua gan FishingTrackingBar!"
            );
            return;
        }

        if (!fishingTrackingBar.isActiveAndEnabled)
        {
            Debug.LogWarning(
                "FishingTrackingBar dang bi Disable!"
            );
            return;
        }

        if (remainingAttempts <= 0)
        {
            FinishFishing();
            return;
        }

        // Tru mot luot cau
        remainingAttempts--;

        currentState = FishingState.Waiting;

        UpdateAttemptsUI();
        UpdateFishingButton();

        SetStatus("Dang tha cau... Cho ca can!");

        fishingCoroutine = StartCoroutine(WaitForFish());
    }

    // ===================================
    // CHO CA CAN CAU
    // ===================================

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

    // ===================================
    // CA CAN CAU
    // ===================================

    private void FishBite()
    {
        if (fishingTrackingBar == null ||
            !fishingTrackingBar.isActiveAndEnabled)
        {
            CancelFailedStart();
            return;
        }

        currentState = FishingState.Tracking;

        SetStatus("CA CAN CAU! Giu SPACE de bat ca!");

        // Bat dau Timing moi
        fishingTrackingBar.StartFishing();

        // Neu thanh Tracking khong khoi dong duoc
        if (!fishingTrackingBar.IsPlaying)
        {
            CancelFailedStart();
        }
    }

    // Hoan tra luot neu Tracking bi loi
    private void CancelFailedStart()
    {
        remainingAttempts++;

        currentState = FishingState.Idle;

        UpdateAttemptsUI();
        UpdateFishingButton();

        SetStatus("Khong the bat dau Fishing Tracking!");

        Debug.LogWarning("Fishing Tracking khong khoi dong!");
    }

    // ===================================
    // NHAN KET QUA BAT CA
    // ===================================

    private void HandleFishingResult(bool success)
    {
        if (currentState != FishingState.Tracking)
            return;

        int reward = 0;
        string resultMessage;

        if (success)
        {
            reward = Mathf.Max(0, fragmentsPerCatch);

            totalFragments += reward;

            resultMessage =
                "SUCCESS! Bat ca thanh cong!\n" +
                "Nhan " + reward + " manh the!";

            Debug.Log(
                "Fishing Success! Reward: " + reward
            );
        }
        else
        {
            resultMessage =
                "FAILED! Ca da thoat!\n" +
                "Khong nhan duoc manh the!";

            Debug.Log("Fishing Failed!");
        }

        UpdateFragmentUI();
        UpdateAttemptsUI();

        Debug.Log(
            $"Total Fragments: {totalFragments} | " +
            $"Remaining Attempts: {remainingAttempts}"
        );

        // Het luot cau
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

    // ===================================
    // KET THUC MINI GAME
    // ===================================

    private void FinishFishing(string lastResult = "")
    {
        currentState = FishingState.Finished;

        string message =
            "DA HET " + Mathf.Max(0, maxFishingAttempts) +
            " LUOT CAU!\n" +
            "Tong manh the: " + totalFragments;

        if (!string.IsNullOrEmpty(lastResult))
        {
            message = lastResult + "\n" + message;
        }

        SetStatus(message);
        UpdateFishingButton();

        Debug.Log(
            "Fishing Finished! Total: " + totalFragments
        );
    }

    // ===================================
    // CHOI LAI MINI GAME
    // ===================================

    public void ResetFishingGame()
    {
        if (fishingCoroutine != null)
        {
            StopCoroutine(fishingCoroutine);
            fishingCoroutine = null;
        }

        if (fishingTrackingBar != null)
        {
            fishingTrackingBar.StopFishing();
        }

        totalFragments = 0;
        remainingAttempts = Mathf.Max(0, maxFishingAttempts);

        currentState = remainingAttempts > 0
            ? FishingState.Idle
            : FishingState.Finished;

        UpdateFragmentUI();
        UpdateAttemptsUI();
        UpdateFishingButton();

        SetStatus("Nhan Fishing de bat dau cau ca!");

        Debug.Log("Fishing MiniGame Reset!");
    }

    // ===================================
    // CAP NHAT BUTTON
    // ===================================

    private void UpdateFishingButton()
    {
        if (fishingButton == null)
            return;

        fishingButton.interactable =
            currentState == FishingState.Idle &&
            remainingAttempts > 0;
    }

    // ===================================
    // CAP NHAT FRAGMENTS
    // ===================================

    private void UpdateFragmentUI()
    {
        if (fragmentText != null)
        {
            fragmentText.text =
                "Card Fragments: " + totalFragments;
        }
    }

    // ===================================
    // CAP NHAT LUOT CAU
    // ===================================

    private void UpdateAttemptsUI()
    {
        if (attemptsText != null)
        {
            attemptsText.text =
                "Luot cau: " + remainingAttempts +
                "/" + Mathf.Max(0, maxFishingAttempts);
        }
    }

    // ===================================
    // CAP NHAT STATUS
    // ===================================

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}