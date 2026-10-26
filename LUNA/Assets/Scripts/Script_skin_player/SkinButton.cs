using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinButton : MonoBehaviour
{
    public Image icon;
    public Image rarityBorder;
    public TextMeshProUGUI nameText;
    public GameObject lockIcon;
    public GameObject vipBadge;
    public GameObject selectedHighlight;

    [HideInInspector] public SkinData skin;
    private SkinSelectionUI ui;

    public void Setup(SkinData data, SkinSelectionUI selectionUI)
    {
        skin = data;
        ui = selectionUI;

        icon.sprite = data.icon;
        nameText.text = data.displayName;
        rarityBorder.color = data.rarityColor;

        bool unlocked = SkinManager.Instance.IsUnlocked(data);
        lockIcon.SetActive(!unlocked);
        vipBadge.SetActive(data.isVIPExclusive);

        GetComponent<Button>().onClick.AddListener(() => ui.OnSelectSkin(skin));
    }

    public void SetSelected(bool selected)
    {
        if (selectedHighlight)
            selectedHighlight.SetActive(selected);
    }
}