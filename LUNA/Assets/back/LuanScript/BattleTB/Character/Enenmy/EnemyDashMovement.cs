
using UnityEngine;
using System.Collections;

public class EnemyDashMovement : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float returnDuration = 0.3f;

    [Header("Distance")]
    [SerializeField] private float stoppingDistance = 1.5f;

    private Vector3 originalPosition;
    private bool hasOriginalPosition;

    // =========================================
    // DASH TO PLAYER
    // =========================================

    public IEnumerator DashTo(Transform player)
    {
        if (player == null)
        {
            Debug.LogWarning(
                "EnemyDashMovement: Chưa có Player Target!"
            );

            yield break;
        }

        // Lưu vị trí ban đầu.
        originalPosition = transform.position;
        hasOriginalPosition = true;

        Vector3 targetPosition = originalPosition;

        // Xác định Player đang ở bên trái hay phải.
        float difference =
            originalPosition.x - player.position.x;

        // Chỉ tiến tới nếu đang đứng xa Player.
        if (Mathf.Abs(difference) > stoppingDistance)
        {
            float direction = Mathf.Sign(difference);

            targetPosition.x =
                player.position.x +
                direction * stoppingDistance;
        }

        // Giữ nguyên Y và Z cho game 2D.
        targetPosition.y = originalPosition.y;
        targetPosition.z = originalPosition.z;

        Debug.Log(
            $"{gameObject.name} bắt đầu lướt tới Player!"
        );

        yield return MoveTo(
            targetPosition,
            dashDuration
        );
    }

    // =========================================
    // RETURN TO ORIGINAL POSITION
    // =========================================

    public IEnumerator ReturnToStart()
    {
        if (!hasOriginalPosition)
            yield break;

        Debug.Log(
            $"{gameObject.name} quay về vị trí cũ!"
        );

        yield return MoveTo(
            originalPosition,
            returnDuration
        );

        hasOriginalPosition = false;
    }

    // =========================================
    // SMOOTH MOVEMENT
    // =========================================

    private IEnumerator MoveTo(
        Vector3 targetPosition,
        float duration)
    {
        Vector3 startPosition = transform.position;

        duration = Mathf.Max(0f, duration);

        if (duration <= 0f)
        {
            transform.position = targetPosition;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration
            );

            // Di chuyển mượt hơn Lerp thông thường.
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            yield return null;
        }

        // Đảm bảo đến đúng vị trí.
        transform.position = targetPosition;
    }
}
