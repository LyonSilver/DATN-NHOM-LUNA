using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MapCameraController : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] Camera cam;

    [Header("Biên map (Đơn vị Unity)")]
    [SerializeField] float mapLeft = 0f;
    [SerializeField] float mapRight = 40f;

    [Header("Cảm giác điều khiển")]
    [SerializeField] float smoothTime = 0.25f;
    [SerializeField] float dragThresholdPixels = 8f;
    [SerializeField] float wheelStep = 1.5f;

    public event System.Action<Vector2> Clicked;   // Bấm chuột (Không phải kéo), tọa độ màn hình

    float targetX;
    float velocity;
    bool pressed;
    bool dragging;
    Vector2 pressScreen;
    float pressTargetX;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        targetX = transform.position.x;
    }

    float ClampX(float x)
    {
        float half = cam.orthographicSize * cam.aspect;
        float min = mapLeft + half;
        float max = mapRight - half;
        if (min > max) return (mapLeft + mapRight) * 0.5f;
        return Mathf.Clamp(x, min, max);
    }

    float WorldPerPixel()
    {
        return cam.orthographicSize * 2f * cam.aspect / Screen.width;
    }

    void SetX(float x)
    {
        Vector3 p = transform.position;
        p.x = x;
        transform.position = p;
    }

    public void FocusOn(float x, bool instant = false)
    {
        targetX = ClampX(x);
        if (instant)
        {
            SetX(targetX);
            velocity = 0f;
        }
    }

    bool PointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    void Update()
    {
        Mouse m = Mouse.current;
        if (m != null)
        {
            if (m.leftButton.wasPressedThisFrame && !PointerOverUI())
            {
                pressed = true;
                dragging = false;
                pressScreen = m.position.ReadValue();
                pressTargetX = targetX;
            }

            if (pressed && m.leftButton.isPressed)
            {
                float dxPixels = m.position.ReadValue().x - pressScreen.x;
                if (!dragging && Mathf.Abs(dxPixels) > dragThresholdPixels) dragging = true;
                if (dragging) targetX = ClampX(pressTargetX - dxPixels * WorldPerPixel());
            }

            if (pressed && m.leftButton.wasReleasedThisFrame)
            {
                if (!dragging) Clicked?.Invoke(m.position.ReadValue());
                pressed = false;
                dragging = false;
            }

            float scroll = m.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
                targetX = ClampX(targetX - Mathf.Sign(scroll) * wheelStep);
        }

        if (dragging)
        {
            SetX(targetX);          // Đang kéo: Bám tay ngay lập tức
            velocity = 0f;
        }
        else
        {
            float x = Mathf.SmoothDamp(transform.position.x, targetX, ref velocity, smoothTime);
            SetX(x);
        }
    }
}