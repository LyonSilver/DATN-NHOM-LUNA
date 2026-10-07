using UnityEngine;

public class UnlockTag : MonoBehaviour
{
    public UnlockType unlockType;
    public string itemId;
    public int requiredLevel = 1;

    [HideInInspector] public GameObject lockedOverlay;

    private void Awake()
    {
        Transform existingOverlay = transform.Find("AI_Locked_Overlay");
        if (existingOverlay != null)
        {
            lockedOverlay = existingOverlay.gameObject;
        }
    }
}