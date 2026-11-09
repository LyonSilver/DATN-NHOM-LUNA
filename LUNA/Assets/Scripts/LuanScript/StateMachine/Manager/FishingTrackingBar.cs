
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class FishingTrackingBar : MonoBehaviour
{
    [Header("Fishing UI")]
    [SerializeField] private GameObject fishingUI;
    [SerializeField] private RectTransform barTrack;
    [SerializeField] private RectTransform controlArea;
    [SerializeField] private RectTransform fishPointer;
    [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text resultText;

    [Header("Player Settings")]
    [SerializeField] private float controlSpeed = 0.7f;

    [Header("Fish Settings")]
    [SerializeField] private float fishSpeed = 0.4f;
    [SerializeField] private float minChangeTime = 0.6f;
    [SerializeField] private float maxChangeTime = 1.5f;

    [Header("Progress Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float startProgress = 0.4f;

    [SerializeField] private float progressGain = 0.3f;
    [SerializeField] private float progressLoss = 0.15f;

    private float controlPosition;
    private float fishPosition;
    private float fishTarget;

    private float changeTimer;
    private float progress;

    private bool isPlaying;

    public bool IsPlaying => isPlaying;

    public event Action<bool> OnFishingFinished;

    private void Start()
    {
        isPlaying = false;

        if (fishingUI != null)
            fishingUI.SetActive(false);

        SetResult("Nhan Fishing de bat dau!");
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        float deltaTime = Time.deltaTime;

        MoveControlArea(deltaTime);
        MoveFish(deltaTime);
        UpdateUIPositions();
        UpdateProgress(deltaTime);
    }

    // =============================
    // BAT DAU MINI GAME
    // =============================

    public void StartFishing()
    {
        if (isPlaying)
            return;

        if (barTrack == null ||
            controlArea == null ||
            fishPointer == null ||
            progressFill == null)
        {
            Debug.LogWarning(
                "FishingTrackingBar: Chua gan du UI!"
            );
            return;
        }

        controlPosition = 0.5f;
        fishPosition = 0.5f;
        fishTarget = 0.5f;

        changeTimer = 1f;
        progress = Mathf.Clamp01(startProgress);

        if (fishingUI != null)
            fishingUI.SetActive(true);

        Canvas.ForceUpdateCanvases();

        progressFill.fillAmount = progress;

        UpdateUIPositions();

        isPlaying = true;

        SetResult("Giu SPACE hoac chuot trai de bat ca!");

        Debug.Log("Fishing Tracking Started!");
    }

    // =============================
    // DIEU KHIEN VUNG BAT CA
    // =============================

    private void MoveControlArea(float deltaTime)
    {
        bool holdSpace =
            Keyboard.current != null &&
            Keyboard.current.spaceKey.isPressed;

        bool holdMouse =
            Mouse.current != null &&
            Mouse.current.leftButton.isPressed;

        bool holding = holdSpace || holdMouse;

        float direction = holding ? 1f : -1f;

        controlPosition +=
            direction * controlSpeed * deltaTime;

        controlPosition = Mathf.Clamp01(controlPosition);
    }

    // =============================
    // CA DI CHUYEN NGAU NHIEN
    // =============================

    private void MoveFish(float deltaTime)
    {
        changeTimer -= deltaTime;

        if (changeTimer <= 0f)
        {
            fishTarget = UnityEngine.Random.Range(
                0.05f,
                0.95f
            );

            float minTime = Mathf.Max(0.1f, minChangeTime);
            float maxTime = Mathf.Max(minTime, maxChangeTime);

            changeTimer = UnityEngine.Random.Range(
                minTime,
                maxTime
            );
        }

        fishPosition = Mathf.MoveTowards(
            fishPosition,
            fishTarget,
            fishSpeed * deltaTime
        );
    }

    // =============================
    // CAP NHAT VI TRI UI
    // =============================

    private void UpdateUIPositions()
    {
        float width = barTrack.rect.width;

        float controlWidth = controlArea.rect.width;
        float fishWidth = fishPointer.rect.width;

        float controlTravel = Mathf.Max(
            0f,
            width - controlWidth
        );

        float fishTravel = Mathf.Max(
            0f,
            width - fishWidth
        );

        float controlX = Mathf.Lerp(
            -controlTravel / 2f,
            controlTravel / 2f,
            controlPosition
        );

        float fishX = Mathf.Lerp(
            -fishTravel / 2f,
            fishTravel / 2f,
            fishPosition
        );

        controlArea.anchoredPosition = new Vector2(
            controlX,
            controlArea.anchoredPosition.y
        );

        fishPointer.anchoredPosition = new Vector2(
            fishX,
            fishPointer.anchoredPosition.y
        );
    }

    // =============================
    // KIEM TRA CA NAM TRONG VUNG
    // =============================

    private bool FishInsideArea()
    {
        float distance = Mathf.Abs(
            fishPointer.anchoredPosition.x -
            controlArea.anchoredPosition.x
        );

        float halfWidth = controlArea.rect.width / 2f;

        return distance <= halfWidth;
    }

    // =============================
    // CAP NHAT TIEN DO BAT CA
    // =============================

    private void UpdateProgress(float deltaTime)
    {
        if (FishInsideArea())
        {
            progress += progressGain * deltaTime;
        }
        else
        {
            progress -= progressLoss * deltaTime;
        }

        progress = Mathf.Clamp01(progress);

        progressFill.fillAmount = progress;

        if (progress >= 1f)
        {
            FinishFishing(true);
        }
        else if (progress <= 0f)
        {
            FinishFishing(false);
        }
    }

    // =============================
    // KET THUC MINI GAME
    // =============================

    private void FinishFishing(bool success)
    {
        if (!isPlaying)
            return;

        isPlaying = false;

        if (fishingUI != null)
            fishingUI.SetActive(false);

        if (success)
        {
            SetResult("SUCCESS! Bat ca thanh cong!");
            Debug.Log("Fishing Success!");
        }
        else
        {
            SetResult("FAILED! Ca da thoat!");
            Debug.Log("Fishing Failed!");
        }

        OnFishingFinished?.Invoke(success);
    }

    // =============================
    // DUNG MINI GAME
    // =============================

    public void StopFishing()
    {
        isPlaying = false;

        if (fishingUI != null)
            fishingUI.SetActive(false);

        SetResult("Da dung cau ca.");
    }

    private void SetResult(string message)
    {
        if (resultText != null)
            resultText.text = message;
    }

    private void OnDisable()
    {
        isPlaying = false;
    }
}