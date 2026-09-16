using UnityEngine;
using UnityEngine.InputSystem;

public class TimingManager : MonoBehaviour
{
    [Header("Timing Bar")]
    [SerializeField] private RectTransform timingBar;
    [SerializeField] private RectTransform cursor;

    [Header("Cursor")]
    [SerializeField] private float cursorSpeed = 1f;

    private float cursorPosition = 0f;
    private bool movingRight = true;
    private bool timingActive = false;

    private void Start()
    {
        StartTiming();
    }

    private void Update()
    {
        if (!timingActive)
            return;

        MoveCursor();

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CheckTiming();
        }
    }

    private void MoveCursor()
    {
        float direction = movingRight ? 1f : -1f;

        cursorPosition += direction * cursorSpeed * Time.deltaTime;

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
        string result;

        if (cursorPosition >= 0.45f && cursorPosition <= 0.55f)
        {
            result = "PERFECT";
        }
        else if (cursorPosition >= 0.35f && cursorPosition <= 0.65f)
        {
            result = "GREAT";
        }
        else if (cursorPosition >= 0.20f && cursorPosition <= 0.80f)
        {
            result = "GOOD";
        }
        else
        {
            result = "MISS";
        }

        Debug.Log(
            $"Timing: {result} | Cursor Position: {cursorPosition:F2}"
        );
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