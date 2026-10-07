using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnlockTag : MonoBehaviour
{
    public UnlockType unlockType = UnlockType.Card;
    public string itemId;
    public int requiredLevel = 1;

    [HideInInspector] public GameObject lockedOverlay;

    private void Start()
    {
        AutoDetectItemId();

        if (AIScannerUnlockOverlay.Instance != null)
        {
            AIScannerUnlockOverlay.Instance.RegisterAndEvaluate(this);
        }
    }

    public void AutoDetectItemId()
    {
        if (!string.IsNullOrEmpty(itemId)) return;

        TextMeshProUGUI tmpText = transform.Find("CardNameText")?.GetComponent<TextMeshProUGUI>();
        if (tmpText != null && !string.IsNullOrEmpty(tmpText.text))
        {
            itemId = tmpText.text.Trim();
            return;
        }

        Text legacyText = transform.Find("CardNameText")?.GetComponent<Text>();
        if (legacyText != null && !string.IsNullOrEmpty(legacyText.text))
        {
            itemId = legacyText.text.Trim();
            return;
        }

        TextMeshProUGUI[] allTmps = GetComponentsInChildren<TextMeshProUGUI>();
        if (allTmps.Length > 0 && !string.IsNullOrEmpty(allTmps[0].text))
        {
            itemId = allTmps[0].text.Trim();
            return;
        }

        itemId = gameObject.name.Replace("(Clone)", "").Trim();
    }
}