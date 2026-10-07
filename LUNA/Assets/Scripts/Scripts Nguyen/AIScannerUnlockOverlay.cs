using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIScannerUnlockOverlay : MonoBehaviour
{
    public static AIScannerUnlockOverlay Instance { get; private set; }

    [Header("Player State for AI Rules")]
    public int playerCurrentLevel = 1;

    private List<UnlockTag> scannedTags = new List<UnlockTag>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        UnlockManager.OnDataUpdated += EvaluateAndApply;
    }

    private void OnDisable()
    {
        UnlockManager.OnDataUpdated -= EvaluateAndApply;
    }

    private void Start()
    {
        ScanScene();
        EvaluateAndApply();
    }

    public void ScanScene()
    {
        scannedTags.Clear();
        UnlockTag[] tags = FindObjectsOfType<UnlockTag>(true);
        scannedTags.AddRange(tags);

        foreach (var tag in scannedTags)
        {
            EnsureOverlayCreated(tag);
        }
    }

    private void EnsureOverlayCreated(UnlockTag tag)
    {
        if (tag.lockedOverlay != null) return;

        GameObject overlay = new GameObject("AI_Locked_Overlay");
        overlay.transform.SetParent(tag.transform, false);

        RectTransform rect = overlay.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        Image img = overlay.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(overlay.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;

        Text txt = textObj.AddComponent<Text>();
        txt.text = "LOCKED";
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.red;
        txt.fontSize = 20;

        tag.lockedOverlay = overlay;
    }

    public void EvaluateAndApply()
    {
        foreach (var tag in scannedTags)
        {
            if (tag == null) continue;

            bool isUnlocked = false;

            if (tag.unlockType == UnlockType.LevelRequirement)
            {
                isUnlocked = playerCurrentLevel >= tag.requiredLevel || UnlockManager.Instance.IsUnlocked(tag.unlockType, tag.itemId);
            }
            else
            {
                isUnlocked = UnlockManager.Instance.IsUnlocked(tag.unlockType, tag.itemId);
            }

            if (tag.lockedOverlay != null)
            {
                tag.lockedOverlay.SetActive(!isUnlocked);
            }

            Button btn = tag.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = isUnlocked;
            }
        }
    }

    public void SetPlayerLevel(int level)
    {
        playerCurrentLevel = level;
        EvaluateAndApply();
    }
}