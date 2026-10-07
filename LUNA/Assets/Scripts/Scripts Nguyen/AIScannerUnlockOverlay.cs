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
        foreach (var tag in tags)
        {
            RegisterAndEvaluate(tag);
        }
    }

    public void RegisterAndEvaluate(UnlockTag tag)
    {
        if (tag == null) return;

        if (!scannedTags.Contains(tag))
        {
            scannedTags.Add(tag);
        }

        EnsureOverlayCreated(tag);
        ApplyTagState(tag);
    }

    private void EnsureOverlayCreated(UnlockTag tag)
    {
        if (tag.lockedOverlay != null) return;

        Transform existingOverlay = tag.transform.Find("AI_Locked_Overlay");
        if (existingOverlay != null)
        {
            tag.lockedOverlay = existingOverlay.gameObject;
            return;
        }

        GameObject overlay = new GameObject("AI_Locked_Overlay");
        overlay.transform.SetParent(tag.transform, false);
        overlay.transform.SetAsLastSibling();

        RectTransform rect = overlay.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        Image img = overlay.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.8f);
        img.raycastTarget = true;

        GameObject textObj = new GameObject("LockText");
        textObj.transform.SetParent(overlay.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        Text txt = textObj.AddComponent<Text>();
        txt.text = "LOCKED";
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.yellow;
        txt.fontSize = 16;

        tag.lockedOverlay = overlay;
    }

    public void EvaluateAndApply()
    {
        for (int i = scannedTags.Count - 1; i >= 0; i--)
        {
            if (scannedTags[i] == null)
            {
                scannedTags.RemoveAt(i);
                continue;
            }
            ApplyTagState(scannedTags[i]);
        }
    }

    private void ApplyTagState(UnlockTag tag)
    {
        if (string.IsNullOrEmpty(tag.itemId))
        {
            tag.AutoDetectItemId();
        }

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
        if (btn == null)
        {
            btn = tag.GetComponentInChildren<Button>();
        }

        if (btn != null)
        {
            btn.interactable = isUnlocked;
        }
    }

    public void SetPlayerLevel(int level)
    {
        playerCurrentLevel = level;
        EvaluateAndApply();
    }
}