using System.Collections.Generic;
using UnityEngine;

public class PathBuilder2D : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] Transform[] nodes;
    [SerializeField] DotView dotPrefab;
    [SerializeField] Transform dotsParent;

    [Header("Hình dạng đường")]
    [SerializeField, Range(0.1f, 0.6f)] float handleRatio = 0.37f;
    [SerializeField] float wobble = 0.6f;

    [Header("Chấm")]
    [SerializeField] float dotSpacing = 0.45f;
    [SerializeField] float nodeMargin = 0.35f;

    [Header("Thử nghiệm (Phase sau sẽ do code điều khiển)")]
    [SerializeField, Min(0)] int currentNodeIndex = 1;

    readonly List<DotView> dots = new List<DotView>();
    readonly List<int> dotSegment = new List<int>();

    [SerializeField] bool buildOnStart = true;

    void Start()
    {
        if (buildOnStart) Build();
    }

    public void BuildWithCurrent(int index)
    {
        currentNodeIndex = index;
        Build();
    }

    bool HasValidNodes()
    {
        if (nodes == null || nodes.Length < 2) return false;
        foreach (Transform n in nodes)
            if (n == null) return false;
        return true;
    }

    // Seg = Đoạn nối node seg và node seg+1; t chạy từ 0 đến 1
    Vector3 GetPoint(int seg, float t)
    {
        Vector3 a = nodes[seg].position;
        Vector3 b = nodes[seg + 1].position;
        float h = (b.x - a.x) * handleRatio;
        Vector3 c1 = a + new Vector3(h, 0f, 0f);
        Vector3 c2 = b - new Vector3(h, 0f, 0f);

        float u = 1f - t;
        Vector3 p = u * u * u * a + 3f * u * u * t * c1 + 3f * u * t * t * c2 + t * t * t * b;

        // Thêm độ uốn lượn, đổi hướng xen kẽ, bằng 0 tại hai đầu node
        float s = Mathf.Sin(Mathf.PI * t);
        float sign = (seg % 2 == 0) ? 1f : -1f;
        p.y += s * s * wobble * sign;
        p.z = 0f;
        return p;
    }

    bool NearAnyNode(Vector3 pos)
    {
        foreach (Transform n in nodes)
        {
            float radius = n.lossyScale.x * 0.5f + nodeMargin;
            Vector3 np = n.position;
            np.z = 0f;
            if (Vector3.Distance(pos, np) < radius) return true;
        }
        return false;
    }

    void ClearDots()
    {
        for (int i = dotsParent.childCount - 1; i >= 0; i--)
        {
            GameObject go = dotsParent.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(go);
            else DestroyImmediate(go);
        }
        dots.Clear();
        dotSegment.Clear();
    }

    void PlaceDot(Vector3 pos, int seg)
    {
        if (NearAnyNode(pos)) return;
        DotView dot = Instantiate(dotPrefab, pos, Quaternion.identity, dotsParent);
        dot.name = "Dot_" + dots.Count;
        dots.Add(dot);
        dotSegment.Add(seg);
    }

    [ContextMenu("Build Path")]
    public void Build()
    {
        if (dotPrefab == null || dotsParent == null || !HasValidNodes())
        {
            Debug.LogWarning("PathBuilder2D: Thiếu Nodes, Dot Prefab hoặc Dots Parent.");
            return;
        }
        if (dotSpacing < 0.1f) dotSpacing = 0.45f;

        ClearDots();

        // Lấy mẫu đường cong thành nhiều điểm nhỏ
        const int samplesPerSegment = 200;
        List<Vector3> pts = new List<Vector3>();
        List<int> segs = new List<int>();
        for (int seg = 0; seg < nodes.Length - 1; seg++)
        {
            for (int i = (seg == 0 ? 0 : 1); i <= samplesPerSegment; i++)
            {
                pts.Add(GetPoint(seg, i / (float)samplesPerSegment));
                segs.Add(seg);
            }
        }

        // Đi dọc đường, cứ đủ khoảng cách dotSpacing thì đặt một chấm
        PlaceDot(pts[0], segs[0]);
        float acc = 0f;
        for (int i = 1; i < pts.Count; i++)
        {
            acc += Vector3.Distance(pts[i - 1], pts[i]);
            if (acc >= dotSpacing)
            {
                PlaceDot(pts[i], segs[i]);
                acc = 0f;
            }
        }

        Debug.Log("PathBuilder2D: Đã tạo " + dots.Count + " chấm.");
        RefreshLit();
    }

    public void RefreshLit()
    {
        for (int i = 0; i < dots.Count; i++)
            dots[i].SetLit(dotSegment[i] < currentNodeIndex);
    }

    public void SetCurrentNode(int index)
    {
        currentNodeIndex = index;
        RefreshLit();
    }

    // Các hàm hỗ trợ hiệu ứng
    public int DotCount => dots.Count;
    public int DotSegmentAt(int i) => dotSegment[i];

    public void SetDotLit(int i, bool lit)
    {
        if (i >= 0 && i < dots.Count) dots[i].SetLit(lit);
    }

    public void ShowUntil(int index)
    {
        currentNodeIndex = index;
        RefreshLit();
    }

    // Vẽ đường vàng trong Scene view để xem trước
    void OnDrawGizmos()
    {
        if (!HasValidNodes()) return;
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.9f);
        for (int seg = 0; seg < nodes.Length - 1; seg++)
        {
            Vector3 prev = GetPoint(seg, 0f);
            for (int i = 1; i <= 40; i++)
            {
                Vector3 p = GetPoint(seg, i / 40f);
                Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }
    }
}