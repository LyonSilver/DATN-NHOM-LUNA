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

    [Header("Cursor")]
    [SerializeField] private float cursorSpeed = 1f;

    private float cursorPosition = 0f;
    private bool timingActive = false;

    public event Action<TimingResult, float> OnTimingResult;

    private void Start()
    {
        StartTiming();
    }

    private void Update()
    {
        if (!timingActive)
            return;

        MoveCursor();

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CheckTiming();
        }
    }

    // Di chuyển Cursor qua lại bằng Mathf.PingPong
    private void MoveCursor()
    {
        cursorPosition = Mathf.PingPong(
            Time.time * cursorSpeed,
            1f
        );

        UpdateCursorPosition();
    }

    // Cập nhật vị trí Cursor trên Timing Bar
    private void UpdateCursorPosition()
    {
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

    // Kiểm tra kết quả Timing và hệ số nhân
    private void CheckTiming()
    {
        TimingResult result;
        float multiplier;

        if (cursorPosition >= 0.45f &&
            cursorPosition <= 0.55f)
        {
            result = TimingResult.PERFECT;
            multiplier = 1.5f;
        }
        else if (cursorPosition >= 0.35f &&
                 cursorPosition <= 0.65f)
        {
            result = TimingResult.GREAT;
            multiplier = 1.25f;
        }
        else if (cursorPosition >= 0.20f &&
                 cursorPosition <= 0.80f)
        {
            result = TimingResult.GOOD;
            multiplier = 1.0f;
        }
        else
        {
            result = TimingResult.MISS;
            multiplier = 0.5f;
        }

        Debug.Log(
            $"Timing: {result} | " +
            $"Cursor Position: {cursorPosition:F2} | " +
            $"Multiplier: {multiplier:F2}x"
        );

        // Phát Event gửi kết quả và hệ số sang BattleManager
        OnTimingResult?.Invoke(result, multiplier);
    }

    // Bật Timing
    public void StartTiming()
    {
        timingActive = true;

        cursorPosition = 0f;

        UpdateCursorPosition();

        Debug.Log("Timing Started");
    }

    // Tắt Timing
    public void StopTiming()
    {
        timingActive = false;

        Debug.Log("Timing Stopped");
    }
}