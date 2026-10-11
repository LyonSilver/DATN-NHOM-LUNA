using UnityEngine;

public static class DamageCalculator
{
    // TÍNH SÁT THƯƠNG CUỐI CÙNG
    public static int CalculateDamage(
        int baseDamage,
        TimingGrade timingGrade,
        float statusMultiplier = 1f,
        float chaosMultiplier = 1f)
    {
        // 1. Lấy hệ số Timing
        float timingMultiplier = GetTimingMultiplier(timingGrade);

        // 2. Tính sát thương
        float finalDamage =
            Mathf.Max(0, baseDamage)
            * timingMultiplier
            * Mathf.Max(0f, statusMultiplier)
            * Mathf.Max(0f, chaosMultiplier);

        // 3. Làm tròn và trả kết quả
        return Mathf.Max(0, Mathf.RoundToInt(finalDamage));
    }

    // HỆ SỐ TIMING
    public static float GetTimingMultiplier(TimingGrade grade)
    {
        switch (grade)
        {
            case TimingGrade.MISS:
                return 0.5f;

            case TimingGrade.GOOD:
                return 1.0f;

            case TimingGrade.GREAT:
                return 1.2f;

            case TimingGrade.PERFECT:
                return 1.5f;

            default:
                return 1.0f;
        }
    }
}