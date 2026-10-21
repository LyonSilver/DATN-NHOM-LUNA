using UnityEngine;
using TMPro;

public enum NodeState { Locked, Current, Cleared }

public class StageNode : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] SpriteRenderer baseRenderer;
    [SerializeField] SpriteRenderer ringRenderer;
    [SerializeField] SpriteRenderer glowRenderer;
    [SerializeField] TextMeshPro numberText;
    [SerializeField] GameObject lockIcon;
    [SerializeField] GameObject starsRoot;
    [SerializeField] GameObject playerMarker;
    [SerializeField] SpriteRenderer[] stars;
    [SerializeField] Collider2D clickCollider;

    [Header("Thông tin Node (Manager sẽ ghi đè khi chạy)")]
    [SerializeField] int stageNumber = 1;
    [SerializeField] bool isBoss;

    
    // Các hàm hỗ trợ hiệu ứng
    public Sprite CircleSprite => baseRenderer != null ? baseRenderer.sprite : null;
    public Transform MarkerTransform => playerMarker != null ? playerMarker.transform : null;
    public Transform[] StarTransforms
    {
        get
        {
            if (stars == null) return new Transform[0];
            Transform[] t = new Transform[stars.Length];
            for (int i = 0; i < stars.Length; i++)
                t[i] = stars[i] != null ? stars[i].transform : null;
            return t;
        }
    }

    [Header("Xem thử (Apply Preview)")]
    [SerializeField] NodeState previewState = NodeState.Locked;
    [SerializeField, Range(0, 3)] int previewStars = 0;

    public StageData Data { get; private set; }
    public NodeState State { get; private set; }
    public int StageNumber => stageNumber;
    public bool IsBoss => isBoss;

    float markerBaseY;

    static Color Hex(string h)
    {
        ColorUtility.TryParseHtmlString("#" + h, out Color c);
        return c;
    }

    static void Active(GameObject g, bool v)
    {
        if (g != null) g.SetActive(v);
    }

    void Awake()
    {
        if (playerMarker != null) markerBaseY = playerMarker.transform.localPosition.y;
    }

    void Start()
    {
        // Nếu chưa có StageData (Manager chưa điều khiển) thì dùng trạng thái xem thử
        if (Data == null) SetState(previewState, previewStars);
    }

    [ContextMenu("Apply Preview")]
    void ApplyPreview()
    {
        SetState(previewState, previewStars);
    }

    public void Setup(StageData data)
    {
        Data = data;
        if (data == null) return;
        stageNumber = data.order;
        isBoss = data.isBossStage;
    }

    public void SetState(NodeState state, int starCount)
    {
        State = state;
        starCount = Mathf.Clamp(starCount, 0, 3);
        Color bossRing = Hex("B3122E");

        Color baseColor, ringColor;
        switch (state)
        {
            case NodeState.Current:
                baseColor = Hex("F5C242");
                ringColor = isBoss ? bossRing : Hex("FFF3B0");
                break;
            case NodeState.Cleared:
                baseColor = Hex("C4A1FF");
                ringColor = isBoss ? bossRing : Color.white;
                break;
            default:
                baseColor = Hex("4A4A55");
                ringColor = isBoss ? Hex("4A1A22") : Hex("2A2A33");
                break;
        }

        if (baseRenderer != null) baseRenderer.color = baseColor;
        if (ringRenderer != null) ringRenderer.color = ringColor;

        Active(lockIcon, state == NodeState.Locked);
        if (glowRenderer != null) Active(glowRenderer.gameObject, state == NodeState.Current);
        Active(playerMarker, state == NodeState.Current);
        Active(starsRoot, state == NodeState.Cleared);

        if (numberText != null)
        {
            numberText.text = stageNumber.ToString();
            numberText.gameObject.SetActive(state != NodeState.Locked);
        }

        if (clickCollider != null) clickCollider.enabled = state != NodeState.Locked;

        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
                if (stars[i] != null)
                    stars[i].color = i < starCount ? Hex("F5C242") : Hex("4A4A55");
        }
    }

    void Update()
    {
        if (State != NodeState.Current) return;

        float wave = Mathf.Sin(Time.time * 2.5f);

        if (glowRenderer != null)
        {
            glowRenderer.transform.localScale = Vector3.one * (1.9f + wave * 0.15f);
            Color c = Hex("F5C242");
            c.a = 0.30f + (wave + 1f) * 0.10f;
            glowRenderer.color = c;
        }

        if (playerMarker != null)
        {
            Vector3 p = playerMarker.transform.localPosition;
            p.y = markerBaseY + wave * 0.06f;
            playerMarker.transform.localPosition = p;
        }
    }
}