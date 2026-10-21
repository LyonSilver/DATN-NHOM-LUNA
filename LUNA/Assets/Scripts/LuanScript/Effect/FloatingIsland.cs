
using System;
using UnityEngine;

public class FloatingIslandGroup : MonoBehaviour
{
    [Serializable]
    public class IslandElement
    {
        [Header("Island Object")]
        public Transform island;

        [Header("Floating Movement")]
        public float floatHeight = 0.15f;
        public float floatSpeed = 0.5f;

        [Header("Horizontal Movement")]
        public bool enableHorizontal = true;
        public float horizontalDistance = 0.05f;
        public float horizontalSpeed = 0.3f;

        [Header("Random Movement")]
        public bool randomStart = true;

        [NonSerialized] public Vector3 startPosition;
        [NonSerialized] public float verticalOffset;
        [NonSerialized] public float horizontalOffset;
    }

    [Header("Island Elements")]
    [SerializeField] private IslandElement[] islands;

    [Header("General Settings")]
    [SerializeField] private bool enableFloating = true;

    // =====================================
    // START
    // =====================================

    private void Start()
    {
        if (islands == null)
            return;

        foreach (IslandElement element in islands)
        {
            if (element == null || element.island == null)
                continue;

            // Lưu vị trí ban đầu của từng đảo.
            element.startPosition =
                element.island.localPosition;

            // Tạo pha chuyển động riêng.
            if (element.randomStart)
            {
                element.verticalOffset =
                    UnityEngine.Random.Range(
                        0f, Mathf.PI * 2f
                    );

                element.horizontalOffset =
                    UnityEngine.Random.Range(
                        0f, Mathf.PI * 2f
                    );
            }
            else
            {
                element.verticalOffset = 0f;
                element.horizontalOffset = 0f;
            }
        }
    }

    // =====================================
    // UPDATE
    // =====================================

    private void Update()
    {
        if (!enableFloating || islands == null)
            return;

        float time = Time.time;

        foreach (IslandElement element in islands)
        {
            if (element == null || element.island == null)
                continue;

            // =================================
            // VERTICAL MOVEMENT
            // =================================

            float y = Mathf.Sin(
                time *
                Mathf.Max(0f, element.floatSpeed) *
                Mathf.PI * 2f +
                element.verticalOffset
            ) * Mathf.Max(0f, element.floatHeight);

            // =================================
            // HORIZONTAL MOVEMENT
            // =================================

            float x = 0f;

            if (element.enableHorizontal)
            {
                x = Mathf.Sin(
                    time *
                    Mathf.Max(0f, element.horizontalSpeed) *
                    Mathf.PI * 2f +
                    element.horizontalOffset
                ) * Mathf.Max(
                    0f, element.horizontalDistance
                );
            }

            // =================================
            // APPLY MOVEMENT
            // =================================

            element.island.localPosition =
                element.startPosition +
                new Vector3(x, y, 0f);
        }
    }

    // =====================================
    // RESET ISLAND POSITIONS
    // =====================================

    private void OnDisable()
    {
        if (islands == null)
            return;

        foreach (IslandElement element in islands)
        {
            if (element == null || element.island == null)
                continue;

            element.island.localPosition =
                element.startPosition;
        }
    }
}
