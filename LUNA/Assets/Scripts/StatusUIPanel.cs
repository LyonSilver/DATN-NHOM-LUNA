using System.Collections.Generic;
using UnityEngine;

public class StatusUIPanel : MonoBehaviour
{
    [SerializeField] private StatusHandler targetHandler;
    [SerializeField] private StatusIconUI iconPrefab;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private List<StatusSpriteData> iconSprites;

    [System.Serializable]
    public struct StatusSpriteData
    {
        public StatusType type;
        public Sprite sprite;
    }

    private void OnEnable()
    {
        if (targetHandler != null)
            targetHandler.OnStatusUpdated += RefreshUI;
    }

    private void OnDisable()
    {
        if (targetHandler != null)
            targetHandler.OnStatusUpdated -= RefreshUI;
    }

    public void RefreshUI()
    {
        foreach (Transform child in iconContainer)
            Destroy(child.gameObject);

        foreach (var status in targetHandler.activeStatuses)
        {
            var sprite = GetSprite(status.type);
            var iconInstance = Instantiate(iconPrefab, iconContainer);
            iconInstance.Setup(status, sprite);
        }
    }

    private Sprite GetSprite(StatusType type)
    {
        var found = iconSprites.Find(s => s.type == type);
        return found.sprite;
    }
}