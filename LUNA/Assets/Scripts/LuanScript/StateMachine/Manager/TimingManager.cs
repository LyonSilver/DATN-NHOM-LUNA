
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimingManager : MonoBehaviour
{
    public enum TimingResult
    {
        MISS,
        GOOD,
        GREAT,
        PERFECT
    }

    [Header("Timing Bar")]
    [SerializeField] private RectTransform timingBar;
    [SerializeField] private RectTransform cursor;

    [Header("Cursor Settings")]
    [SerializeField] private float cursorSpeed = 1f;

    [Header("Timing Multipliers")]
    [SerializeField] private float missMultiplier = 1.5f;
    [SerializeField] private float goodMultiplier = 1.0f;
    [SerializeField] private float greatMultiplier = 0.75f;
    [SerializeField] private float perfectMultiplier = 0.5f;

    // Vi tri con tro tu 0 den 1
    private float cursorPosition = 0f;

    // Thoi gian chay cua Timing
    private float elapsedTime = 0f;

    // Trang thai Timing
    private bool timingActive = false;

    // Frame bat dau Timing
    private int startFrame = -1;

    // Event tra ket qua
    public event Action<TimingResult, float> OnTimingResult;

    // Kiem tra Timing co dang hoat dong khong
    public bool IsTimingActive => timingActive;

    private void Start()
    {
        StopTiming();
    }

    private void Update()
    {
        if (!timingActive)
            return;

        MoveCursor();

        // Khong nhan Space ngay frame bat dau
        if (Time.frameCount == startFrame)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CheckTiming();
        }
    }

    // ============================
    // DI CHUYEN CON TRO
    // ============================

    private void MoveCursor()
    {
        elapsedTime += Time.deltaTime;

        cursorPosition = Mathf.PingPong(
            elapsedTime * cursorSpeed,
            1f
        );

        UpdateCursorPosition();
    }

    // ============================
    // CAP NHAT VI TRI CON TRO
    // ============================

    private void UpdateCursorPosition()
    {
        if (timingBar == null || cursor == null)
            return;

        float barWidth = timingBar.rect.width;

        float xPosition = Mathf.Lerp(
            -barWidth / 2f,
            barWidth / 2f,
            cursorPosition
        );

        cursor.anchoredPosition = new Vector2(
            xPosition,
            cursor.anchoredPosition.y
        );
    }

    // ============================
    // KIEM TRA TIMING
    // ============================

    private void CheckTiming()
    {
        if (!timingActive)
            return;

        TimingResult result;
        float multiplier;

        // PERFECT
        if (cursorPosition >= 0.45f &&
            cursorPosition <= 0.55f)
        {
            result = TimingResult.PERFECT;
            multiplier = perfectMultiplier;
        }

        // GREAT
        else if (cursorPosition >= 0.35f &&
                 cursorPosition <= 0.65f)
        {
            result = TimingResult.GREAT;
            multiplier = greatMultiplier;
        }

        // GOOD
        else if (cursorPosition >= 0.20f &&
                 cursorPosition <= 0.80f)
        {
            result = TimingResult.GOOD;
            multiplier = goodMultiplier;
        }

        // MISS
        else
        {
            result = TimingResult.MISS;
            multiplier = missMultiplier;
        }

        Debug.Log(
            $"Timing Result: {result} | " +
            $"Position: {cursorPosition:F2} | " +
            $"Multiplier: {multiplier:F2}x"
        );

        // Dung Timing truoc khi gui ket qua
        StopTiming();

        // Gui ket qua cho cac script khac
        OnTimingResult?.Invoke(result, multiplier);
    }

    // ============================
    // BAT DAU TIMING
    // ============================

    public void StartTiming()
    {
        if (timingBar == null || cursor == null)
        {
            Debug.LogWarning(
                "TimingManager: Chua gan TimingBar hoac Cursor!"
            );
            return;
        }

        // Reset vi tri va thoi gian
        elapsedTime = 0f;
        cursorPosition = 0f;

        startFrame = Time.frameCount;

        // Hien TimingBar
        timingBar.gameObject.SetActive(true);

        // Bat dau di chuyen
        timingActive = true;

        UpdateCursorPosition();

        Debug.Log("Timing Started!");
    }

    // ============================
    // DUNG TIMING
    // ============================

    public void StopTiming()
    {
        timingActive = false;

        elapsedTime = 0f;
        startFrame = -1;

        if (timingBar != null)
        {
            timingBar.gameObject.SetActive(false);
        }

        Debug.Log("Timing Stopped!");
    }

    // ============================
    // THAY DOI TOC DO
    // ============================

    public void SetCursorSpeed(float newSpeed)
    {
        cursorSpeed = Mathf.Max(0.1f, newSpeed);
    }

    // ============================
    // RESET TOC DO
    // ============================

    public void ResetCursorSpeed()
    {
        cursorSpeed = 1f;
    }

    private void OnDisable()
    {
        StopTiming();
    }
}
