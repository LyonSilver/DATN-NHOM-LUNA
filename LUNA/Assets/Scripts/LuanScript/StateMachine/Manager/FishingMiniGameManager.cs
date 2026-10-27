
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

    [Header("Fishing Settings")]
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 5f;

    [Header("Card Fragment Reward")]
    [SerializeField] private int goodReward = 1;
    [SerializeField] private int greatReward = 2;
    [SerializeField] private int perfectReward = 3;

    private enum FishingState
    {
        Idle,
        Waiting,
        Timing
    }

    private FishingState currentState = FishingState.Idle;

    private int totalFragments = 0;
    private Coroutine fishingCoroutine;

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

        currentState = FishingState.Idle;

        UpdateFragmentUI();
        SetStatus("Nhan Fishing de bat dau cau ca!");

        if (fishingButton != null)
        {
            fishingButton.interactable = true;
        }
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
    }

    // Gan ham nay vao Button Fishing
    public void StartFishing()
    {
        if (currentState != FishingState.Idle)
            return;

        if (timingManager == null)
        {
            Debug.LogWarning("Chua gan TimingManager!");
            return;
        }

        currentState = FishingState.Waiting;

        if (fishingButton != null)
        {
            fishingButton.interactable = false;
        }

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

        currentState = FishingState.Idle;

        int reward = 0;

        switch (result)
        {
            case TimingManager.TimingResult.MISS:
                reward = 0;
                SetStatus("MISS! Ca da thoat!");
                break;

            case TimingManager.TimingResult.GOOD:
                reward = goodReward;
                SetStatus("GOOD! Nhan 1 manh the!");
                break;

            case TimingManager.TimingResult.GREAT:
                reward = greatReward;
                SetStatus("GREAT! Nhan 2 manh the!");
                break;

            case TimingManager.TimingResult.PERFECT:
                reward = perfectReward;
                SetStatus("PERFECT! Nhan 3 manh the!");
                break;
        }

        totalFragments += reward;

        UpdateFragmentUI();

        Debug.Log(
            $"Fishing Result: {result} | " +
            $"Reward: {reward} | " +
            $"Total: {totalFragments}"
        );

        if (fishingButton != null)
        {
            fishingButton.interactable = true;
        }
    }

    private void UpdateFragmentUI()
    {
        if (fragmentText != null)
        {
            fragmentText.text = "Card Fragments: " + totalFragments;
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
