using UnityEngine;

public class PortalRotationUI : MonoBehaviour
{
[Header("Portal Rotation Settings")]
[SerializeField] private float rotationSpeed = -12f;

[Header("Zoom Rotation Settings")]
[SerializeField] private float zoomRotationSpeed = -180f;
[SerializeField] private float accelerationSpeed = 5f;

private float currentRotationSpeed;
private bool isZooming = false;

void Start()
{
    currentRotationSpeed = rotationSpeed;
}

void Update()
{
    // Tăng tốc độ xoay khi bắt đầu zoom
    if (isZooming)
    {
        currentRotationSpeed = Mathf.Lerp(
            currentRotationSpeed,
            zoomRotationSpeed,
            accelerationSpeed * Time.unscaledDeltaTime
        );
    }

    // Xoay cổng
    transform.Rotate(
        0f,
        0f,
        currentRotationSpeed * Time.unscaledDeltaTime
    );
}

public void StartZoomRotation()
{
    isZooming = true;
}

public void ResetRotationSpeed()
{
    isZooming = false;
    currentRotationSpeed = rotationSpeed;
}

}