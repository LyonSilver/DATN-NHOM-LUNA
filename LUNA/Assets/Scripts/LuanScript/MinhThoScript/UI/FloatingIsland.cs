
using UnityEngine;

public class FloatingIsland : MonoBehaviour
{
    [Header("Floating Settings")]
    public float floatHeight = 0.15f;
    public float floatSpeed = 1f;

    [Header("Rotation Settings")]
    public bool enableRotation = false;
    public float rotationAngle = 1f;
    public float rotationSpeed = 0.5f;

    [Header("Random Settings")]
    public bool randomStart = true;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private float timeOffset;

    void Start()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;

        if (randomStart)
        {
            timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    void Update()
    {
        float time = Time.time + timeOffset;

        float offsetY = Mathf.Sin(time * floatSpeed)
                        * floatHeight;

        transform.localPosition =
            startPosition + Vector3.up * offsetY;

        if (enableRotation)
        {
            float angle = Mathf.Sin(time * rotationSpeed)
                          * rotationAngle;

            transform.localRotation =
                startRotation * Quaternion.Euler(0, 0, angle);
        }
    }
}
