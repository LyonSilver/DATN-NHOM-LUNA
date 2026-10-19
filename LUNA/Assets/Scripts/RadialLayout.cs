using UnityEngine;

public class RadialLayout : MonoBehaviour
{
    public float radius = 220f;
    public float startAngle = 90f;
    public Vector2 buttonSize = new Vector2(140, 50);

    void OnEnable()
    {
        Arrange();
    }

    [ContextMenu("Arrange Now")]
    public void Arrange()
    {
        int count = transform.childCount;
        if (count == 0) return;

        RectTransform parent = GetComponent<RectTransform>();
        parent.anchorMin = parent.anchorMax = new Vector2(0.5f, 0.5f);
        parent.anchoredPosition = Vector2.zero;

        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child == null) continue;

            child.anchorMin = child.anchorMax = new Vector2(0.5f, 0.5f);
            child.pivot = new Vector2(0.5f, 0.5f);
            // child.sizeDelta = buttonSize;

            float angle = (startAngle - i * angleStep) * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );

            child.anchoredPosition = pos;
        }
    }
}