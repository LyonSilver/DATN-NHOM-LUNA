using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinSelectionUI : MonoBehaviour
{
    [Header("References")]
    public SkinManager skinManager;
    public SkinPreviewController preview;
    public Transform contentParent;        // Content của ScrollView
    public GameObject skinButtonPrefab;    // Button có Image + Text + Rarity border
    public Button equipButton;
    public TextMeshProUGUI skinNameText;
    public TextMeshProUGUI rarityText;

    private List<SkinButton> buttons = new();
    private SkinData selectedSkin;

    void Start()
    {
        GenerateButtons();
        equipButton.onClick.AddListener(OnEquip);
    }

    void GenerateButtons()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        buttons.Clear();

        foreach (var skin in skinManager.allSkins)
        {
            var go = Instantiate(skinButtonPrefab, contentParent);
            var btn = go.GetComponent<SkinButton>();
            btn.Setup(skin, this);
            buttons.Add(btn);
        }
    }

    public void OnSelectSkin(SkinData skin)
    {
        selectedSkin = skin;
        preview.ShowSkin(skin);

        skinNameText.text = skin.displayName;
        rarityText.text = skin.rarity.ToString();
        rarityText.color = skin.rarityColor;

        equipButton.interactable = skinManager.IsUnlocked(skin);
        equipButton.GetComponentInChildren<TextMeshProUGUI>().text = 
            skinManager.IsUnlocked(skin) ? "EQUIP" : "LOCKED";

        // Highlight button đang chọn
        foreach (var b in buttons)
            b.SetSelected(b.skin == skin);
    }

    void OnEquip()
    {
        if (selectedSkin == null) return;
        skinManager.EquipSkin(selectedSkin);
        // Có thể thêm popup "Equipped!" hoặc đóng UI
    }
}