
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
    [SerializeField] private float goodMultiplier = 1f;
    [SerializeField] private float greatMultiplier = 0.75f;
    [SerializeField] private float perfectMultiplier = 0.5f;

    private float cursorPosition = 0f;
    private float elapsedTime = 0f;
    private bool timingActive = false;
    private int startFrame = -1;

    public event Action<TimingResult, float> OnTimingResult;

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

        if (Time.frameCount == startFrame)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CheckTiming();
        }
    }

    private void MoveCursor()
    {
        elapsedTime += Time.deltaTime;

        cursorPosition = Mathf.PingPong(
            elapsedTime * cursorSpeed, 1f
        );

        UpdateCursorPosition();
    }

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

    private void CheckTiming()
    {
        if (!timingActive)
            return;

        TimingResult result;
        float multiplier;

        if (cursorPosition >= 0.45f &&
            cursorPosition <= 0.55f)
        {
            result = TimingResult.PERFECT;
            multiplier = perfectMultiplier;
        }
        else if (cursorPosition >= 0.35f &&
                 cursorPosition <= 0.65f)
        {
            result = TimingResult.GREAT;
            multiplier = greatMultiplier;
        }
        else if (cursorPosition >= 0.20f &&
                 cursorPosition <= 0.80f)
        {
            result = TimingResult.GOOD;
            multiplier = goodMultiplier;
        }
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

        StopTiming();

        OnTimingResult?.Invoke(result, multiplier);
    }

    public void StartTiming()
    {
        if (timingBar == null || cursor == null)
        {
            Debug.LogWarning(
                "TimingManager: Chua gan TimingBar hoac Cursor!"
            );
            return;
        }

        elapsedTime = 0f;
        cursorPosition = 0f;
        startFrame = Time.frameCount;

        timingBar.gameObject.SetActive(true);
        timingActive = true;

        UpdateCursorPosition();
    }

    public void StopTiming()
    {
        timingActive = false;
        elapsedTime = 0f;
        startFrame = -1;

        if (timingBar != null)
        {
            timingBar.gameObject.SetActive(false);
        }
    }

    public void SetCursorSpeed(float newSpeed)
    {
        cursorSpeed = Mathf.Max(0.1f, newSpeed);
    }

    public void ResetCursorSpeed()
    {
        cursorSpeed = 1f;
    }

    private void OnDisable()
    {
        StopTiming();
    }
}
