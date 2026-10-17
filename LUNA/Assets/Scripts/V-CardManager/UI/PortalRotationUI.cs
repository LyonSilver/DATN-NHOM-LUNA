using UnityEngine;

public class PortalRotationUI : MonoBehaviour
{
    [Header("Portal Rotation Settings")]
    [SerializeField] private float rotationSpeed = -12f;

    void Update()
    {
        transform.Rotate(
            0f,
            0f,
            rotationSpeed * Time.unscaledDeltaTime
        );
    }
}