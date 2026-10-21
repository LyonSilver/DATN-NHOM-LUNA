using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimingManager : MonoBehaviour
{
    [Header("Tham chiếu UI (kéo từ TimingUI)")]
    public GameObject timingPanel;
    public RectTransform timingBar;
    public RectTransform goodZone;
    public RectTransform greatZone;
    public RectTransform perfectZone;
    public RectTransform marker;
    public Button stopButton;
    public TMP_Text timingInfoText;

    [Header("Cấu hình Timing (đơn vị pixel trên thanh rộng 800px)")]
    public float goodZoneWidth = 280f;
    public float greatZoneWidth = 150f;
    public float perfectZoneWidth = 56f;
    public float markerSpeed = 600f;
    public float timeoutSeconds = 4f;
    public float resultDisplayTime = 0.6f;

    public bool IsRunning { get; private set; }

    private bool stopRequested;

    private void Awake()
    {
        if (stopButton != null)
        {
            stopButton.onClick.AddListener(RequestStop);
        }

        if (timingPanel != null)
        {
            timingPanel.SetActive(false);
        }
    }

    public void StartTiming(CardData card, float windowBonusPercent, string hitLabel, Action<TimingResult> onComplete)
    {
        if (IsRunning)
        {
            Debug.LogWarning("TimingManager: Đang có một Timing chạy, bỏ qua yêu cầu mới (trả về Miss).");
            if (onComplete != null) onComplete(TimingResult.Miss);
            return;
        }

        StartCoroutine(TimingRoutine(card, windowBonusPercent, hitLabel, onComplete));
    }

    private void RequestStop()
    {
        if (IsRunning)
        {
            stopRequested = true;
        }
    }

    private IEnumerator TimingRoutine(CardData card, float windowBonusPercent, string hitLabel, Action<TimingResult> onComplete)
    {
        IsRunning = true;
        stopRequested = false;

        float multiplier = 1f + windowBonusPercent / 100f;
        float goodWidth = goodZoneWidth * multiplier;
        float greatWidth = greatZoneWidth * multiplier;
        float perfectWidth = perfectZoneWidth * multiplier;

        SetZoneWidth(goodZone, goodWidth);
        SetZoneWidth(greatZone, greatWidth);
        SetZoneWidth(perfectZone, perfectWidth);

        float barWidth = timingBar.rect.width;
        float halfBar = barWidth / 2f;

        string info = card.cardName.ToUpper() + hitLabel;
        if (windowBonusPercent > 0f)
        {
            info += " [FOCUS +" + windowBonusPercent + "%]";
        }
        info += "  -  SPACE / STOP!";

        timingInfoText.text = info;
        timingInfoText.color = Color.white;

        marker.anchoredPosition = new Vector2(-halfBar, 0f);
        timingPanel.SetActive(true);

        float elapsed = 0f;
        float markerX = -halfBar;

        while (!stopRequested && elapsed < timeoutSeconds)
        {
            elapsed += Time.deltaTime;
            markerX = -halfBar + Mathf.PingPong(elapsed * markerSpeed, barWidth);
            marker.anchoredPosition = new Vector2(markerX, 0f);

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Space))
            {
                stopRequested = true;
            }
#endif

            yield return null;
        }

        TimingResult result;

        if (!stopRequested)
        {
            result = TimingResult.Miss;
        }
        else
        {
            result = Evaluate(Mathf.Abs(markerX), goodWidth / 2f, greatWidth / 2f, perfectWidth / 2f);
        }

        Debug.Log("TimingManager: Kết quả Timing = " + result);

        timingInfoText.text = result.ToString().ToUpper() + "!";
        timingInfoText.color = GetResultColor(result);

        yield return new WaitForSeconds(resultDisplayTime);

        timingPanel.SetActive(false);
        IsRunning = false;

        if (onComplete != null) onComplete(result);
    }

    private TimingResult Evaluate(float distance, float halfGood, float halfGreat, float halfPerfect)
    {
        if (distance <= halfPerfect) return TimingResult.Perfect;
        if (distance <= halfGreat) return TimingResult.Great;
        if (distance <= halfGood) return TimingResult.Good;
        return TimingResult.Miss;
    }

    private void SetZoneWidth(RectTransform zone, float width)
    {
        zone.sizeDelta = new Vector2(width, zone.sizeDelta.y);
    }

    private Color GetResultColor(TimingResult result)
    {
        switch (result)
        {
            case TimingResult.Perfect:
                return new Color32(255, 215, 0, 255);
            case TimingResult.Great:
                return new Color32(120, 210, 90, 255);
            case TimingResult.Good:
                return new Color32(70, 160, 220, 255);
            default:
                return new Color32(220, 70, 70, 255);
        }
    }
}