using UnityEngine;
using System;

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

    [Header("Cursor Movement")]
    [SerializeField] private float cursorSpeed = 300f;

    [Header("Timing Window")]
    [SerializeField] private bool timingActive = false;

    [Header("Timing Zones")]
    [SerializeField] private float perfectMin = 0.45f;
    [SerializeField] private float perfectMax = 0.55f;

    [SerializeField] private float greatMin = 0.35f;
    [SerializeField] private float greatMax = 0.65f;

    [SerializeField] private float goodMin = 0.20f;
    [SerializeField] private float goodMax = 0.80f;

    // Event gửi kết quả Timing cho BattleManager
    public event Action<TimingResult, float> OnTimingResult;

    private float cursorPosition = 0f;
    private bool movingRight = true;

    private void Update()
    {
        if (!timingActive)
            return;

        MoveCursor();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckTiming();
        }
    }

    private void MoveCursor()
    {
        float direction = movingRight ? 1f : -1f;

        cursorPosition += direction * cursorSpeed * Time.deltaTime / timingBar.rect.width;

        if (cursorPosition >= 1f)
        {
            cursorPosition = 1f;
            movingRight = false;
        }
        else if (cursorPosition <= 0f)
        {
            cursorPosition = 0f;
            movingRight = true;
        }

        UpdateCursorPosition();
    }

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

    private void CheckTiming()
    {
        TimingResult result;
        float multiplier;

        if (cursorPosition >= perfectMin && cursorPosition <= perfectMax)
        {
            result = TimingResult.PERFECT;
            multiplier = 1.5f;
        }
        else if (cursorPosition >= greatMin && cursorPosition <= greatMax)
        {
            result = TimingResult.GREAT;
            multiplier = 1.25f;
        }
        else if (cursorPosition >= goodMin && cursorPosition <= goodMax)
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
            $"Timing Result: {result} | " +
            $"Cursor Position: {cursorPosition:F2} | " +
            $"Multiplier: {multiplier}x"
        );

        OnTimingResult?.Invoke(result, multiplier);

        StopTiming();
    }

    public void StartTiming()
    {
        timingActive = true;

        cursorPosition = 0f;
        movingRight = true;

        UpdateCursorPosition();

        Debug.Log("Timing Started");
    }

    public void StopTiming()
    {
        timingActive = false;

        Debug.Log("Timing Stopped");
    }
}