
using UnityEngine;

public enum IslandState
{
    Locked,
    Current,
    Cleared
}

public class IslandStage : MonoBehaviour
{
    [Header("Stage Data")]
    [SerializeField] StageData stageData;

    [Header("Island References")]
    [SerializeField] IslandHover islandHover;
    [SerializeField] Collider2D clickCollider;

    [Header("Stage Visual")]
    [SerializeField] GameObject lockIcon;
    [SerializeField] GameObject starsRoot;
    [SerializeField] GameObject playerMarker;
    [SerializeField] SpriteRenderer[] stars;

    [Header("Preview")]
    [SerializeField] IslandState previewState =
        IslandState.Locked;

    [SerializeField, Range(0, 3)]
    int previewStars;

    public StageData Data => stageData;

    public IslandState State { get; private set; }

    public int StarCount { get; private set; }

    public bool IsUnlocked =>
        State != IslandState.Locked;

    public Collider2D ClickCollider =>
        clickCollider;

    public IslandHover Hover => islandHover;

    private void Awake()
    {
        if (islandHover == null)
        {
            islandHover =
                GetComponentInChildren<IslandHover>();
        }

        if (clickCollider == null && islandHover != null)
        {
            clickCollider =
                islandHover.GetComponent<Collider2D>();
        }

        SetState(previewState, previewStars);
    }

    // =====================================
    // SETUP STAGE DATA
    // =====================================

    public void Setup(StageData data)
    {
        stageData = data;
    }

    // =====================================
    // ISLAND STATE
    // =====================================

    public void SetState(
        IslandState state,
        int starCount)
    {
        State = state;

        StarCount = Mathf.Clamp(starCount, 0, 3);

        if (islandHover != null)
        {
            islandHover.SetUnlocked(IsUnlocked);
        }

        SetActive(
            lockIcon,
            state == IslandState.Locked
        );

        SetActive(
            playerMarker,
            state == IslandState.Current
        );

        SetActive(
            starsRoot,
            state == IslandState.Cleared
        );

        UpdateStars();
    }

    // =====================================
    // STARS
    // =====================================

    private void UpdateStars()
    {
        if (stars == null)
            return;

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null)
                continue;

            stars[i].color = i < StarCount
                ? new Color32(245, 194, 66, 255)
                : new Color32(74, 74, 85, 255);
        }
    }

    private void SetActive(
        GameObject obj,
        bool active)
    {
        if (obj != null)
        {
            obj.SetActive(active);
        }
    }

    // =====================================
    // CHECK CLICK POSITION
    // =====================================

    public bool ContainsPoint(Vector2 worldPosition)
    {
        return clickCollider != null &&
               clickCollider.enabled &&
               clickCollider.OverlapPoint(worldPosition);
    }

    [ContextMenu("Apply Preview")]
    private void ApplyPreview()
    {
        SetState(previewState, previewStars);
    }
}