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
        // Không tự Start Timing khi vào game
        StopTiming();
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


    private void MoveCursor()
    {
        cursorPosition = Mathf.PingPong(
            Time.time * cursorSpeed,
            1f
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
        TimingResult result;
        float multiplier;

        if (cursorPosition >= 0.45f &&
            cursorPosition <= 0.55f)
        {
            result = TimingResult.PERFECT;
            multiplier = 0.5f;
        }
        else if (cursorPosition >= 0.35f &&
                 cursorPosition <= 0.65f)
        {
            result = TimingResult.GREAT;
            multiplier = 0.75f;
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
            multiplier = 1.5f;
        }

        Debug.Log(
            $"Timing: {result} | " +
            $"Cursor Position: {cursorPosition:F2} | " +
            $"Damage Multiplier: {multiplier:F2}x"
        );

        // Tắt Timing trước
        StopTiming();

        // Gửi kết quả
        OnTimingResult?.Invoke(result, multiplier);
    }


    public void StartTiming()
    {
        if (timingBar != null)
        {
            timingBar.gameObject.SetActive(true);
        }

        timingActive = true;

        cursorPosition = 0f;

        UpdateCursorPosition();

        Debug.Log("Timing Started");
    }


    public void StopTiming()
    {
        timingActive = false;

        if (timingBar != null)
        {
            timingBar.gameObject.SetActive(false);
        }

        Debug.Log("Timing Stopped");
    }
}