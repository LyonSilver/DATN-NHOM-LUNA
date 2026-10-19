using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Test Damage")]
    [SerializeField] private float baseDamage = 100f;

    [Header("Reference")]
    [SerializeField] private TimingManager timingManager;

    private void Start()
    {
        // Đăng ký nhận Event từ TimingManager
        if (timingManager != null)
        {
            timingManager.OnTimingResult += HandleTimingResult;
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký Event
        if (timingManager != null)
        {
            timingManager.OnTimingResult -= HandleTimingResult;
        }
    }

    private void HandleTimingResult(
        TimingManager.TimingResult result,
        float multiplier
    )
    {
        // Tính Damage sau khi áp dụng hệ số Timing
        float finalDamage = baseDamage * multiplier;

        Debug.Log(
            $"BattleManager | Timing: {result} | " +
            $"Base Damage: {baseDamage} | " +
            $"Multiplier: {multiplier:F2}x | " +
            $"Final Damage: {finalDamage:F0}"
        );
    }
}