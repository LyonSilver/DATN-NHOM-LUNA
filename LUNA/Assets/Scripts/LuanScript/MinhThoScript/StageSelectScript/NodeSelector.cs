using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NodeSelector : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] StageSelectManager manager;
    [SerializeField] MapCameraController cameraController;
    [SerializeField] LineRenderer ring;

    [Header("Cảm giác")]
    [SerializeField] float screenOffsetX = 1.9f;    // Node nằm lệch trái màn hình để thấy đường phía trước
    [SerializeField] float ringPadding = 1.28f;     // Vòng chọn to hơn node bao nhiêu lần

    public event System.Action<StageData> StageChosen;
    public event System.Action Moved;
    public event System.Action LockedClicked;
    public bool InputLocked;
    public StageNode Selected => (selectedIndex >= 0 && nodes != null) ? nodes[selectedIndex] : null;

    StageNode[] nodes;
    int selectedIndex = -1;
    Camera cam;
    bool shaking;

    void Awake()
    {
        cam = Camera.main;
        if (ring != null)
        {
            ring.loop = true;
            ring.useWorldSpace = true;
            ring.sortingLayerName = "FX";
            ring.sortingOrder = 5;
            ring.numCornerVertices = 4;
            ring.startColor = new Color32(0xF5, 0xC2, 0x42, 255);
            ring.endColor = new Color32(0xF5, 0xC2, 0x42, 255);
            ring.enabled = false;
        }
    }

    void OnEnable()
    {
        if (manager != null) manager.Refreshed += OnRefreshed;
        if (cameraController != null) cameraController.Clicked += OnClicked;
    }

    void OnDisable()
    {
        if (manager != null) manager.Refreshed -= OnRefreshed;
        if (cameraController != null) cameraController.Clicked -= OnClicked;
    }
    
    public void Reselect()
    {
        OnRefreshed();
    }

    public void HideRing()
    {
        if (ring != null) ring.enabled = false;
    }

    void OnRefreshed()
    {
        nodes = manager.Nodes;
        if (nodes == null || nodes.Length == 0) return;

        int pick = -1;
        for (int i = 0; i < nodes.Length; i++)
            if (nodes[i].State == NodeState.Current) { pick = i; break; }

        if (pick < 0)
            for (int i = nodes.Length - 1; i >= 0; i--)
                if (nodes[i].State != NodeState.Locked) { pick = i; break; }

        if (pick < 0) pick = 0;
        Select(pick, true);
    }

    void BuildRing(Vector3 center, float radius)
    {
        const int count = 48;
        ring.positionCount = count;
        for (int i = 0; i < count; i++)
        {
            float a = i / (float)count * Mathf.PI * 2f;
            ring.SetPosition(i, center + new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
        }
    }

    void Select(int index, bool instant)
    {
        selectedIndex = index;
        StageNode n = nodes[index];

        if (ring != null)
        {
            ring.enabled = true;
            BuildRing(n.transform.position, n.transform.lossyScale.x * 0.5f * ringPadding);
        }

        if (cameraController != null)
            cameraController.FocusOn(n.transform.position.x + screenOffsetX, instant);
        
        if (!instant) Moved?.Invoke();
    }

    int HitNode(Vector2 screenPos)
    {
        Vector3 w = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
        for (int i = 0; i < nodes.Length; i++)
        {
            Vector2 np = nodes[i].transform.position;
            float r = nodes[i].transform.lossyScale.x * 0.5f + 0.15f;
            if (((Vector2)w - np).sqrMagnitude <= r * r) return i;
        }
        return -1;
    }

    void OnClicked(Vector2 screenPos)
    {
        if (nodes == null || InputLocked) return;
        int idx = HitNode(screenPos);
        if (idx < 0) return;

        if (nodes[idx].State == NodeState.Locked)
        {
            Debug.Log("Stage " + (idx + 1) + " đang khóa. " + Reason(idx));
            LockedClicked?.Invoke();
            if (!shaking) StartCoroutine(Shake(nodes[idx].transform));
            return;
        }

        Select(idx, false);
        Confirm();
    }

    string Reason(int idx)
    {
        if (idx > 0 && nodes[idx - 1].State != NodeState.Cleared && nodes[idx - 1].Data != null)
            return "Cần hoàn thành \"" + nodes[idx - 1].Data.displayName + "\" trước.";

        int need = nodes[idx].Data != null ? nodes[idx].Data.requiredPlayerLevel : 0;
        return "Cần Player Level " + need + " (Hiện tại " + _SaveSystem.Load().playerLevel + ").";
    }

    IEnumerator Shake(Transform t)
    {
        shaking = true;
        Vector3 basePos = t.position;
        const float duration = 0.3f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Mathf.Sin(elapsed * 60f) * 0.08f * (1f - elapsed / duration);
            t.position = basePos + new Vector3(x, 0f, 0f);
            yield return null;
        }
        t.position = basePos;
        shaking = false;
    }

    void Move(int dir)
    {
        if (nodes == null || selectedIndex < 0) return;
        int i = selectedIndex + dir;
        while (i >= 0 && i < nodes.Length)
        {
            if (nodes[i].State != NodeState.Locked)
            {
                Select(i, false);
                return;
            }
            i += dir;
        }
    }

    void Confirm()
    {
        StageNode n = Selected;
        if (n == null || n.Data == null) return;
        Debug.Log("Chọn Stage " + n.Data.order + ": " + n.Data.displayName + (n.Data.isBossStage ? " (BOSS)" : ""));
        StageChosen?.Invoke(n.Data);
    }

    void Update()
    {
        if (ring != null && ring.enabled)
            ring.widthMultiplier = 0.08f + Mathf.Sin(Time.time * 4f) * 0.025f;

        if (InputLocked) return;
       Keyboard k = Keyboard.current;
        if (k == null || nodes == null) return;

        if (k.aKey.wasPressedThisFrame || k.leftArrowKey.wasPressedThisFrame) Move(-1);
        if (k.dKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame) Move(1);
        if (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame) Confirm();
        if (k.escapeKey.wasPressedThisFrame) Debug.Log("Esc: Sau này sẽ quay về Main Menu.");
    }
}