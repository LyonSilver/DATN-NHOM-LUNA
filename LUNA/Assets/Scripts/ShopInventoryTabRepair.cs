using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class ShopInventoryTabRepair : MonoBehaviour
{
    public Button shopButton;
    public Button inventoryButton;
    public GameObject shopPanel;
    public GameObject inventoryPanel;
    public bool startWithInventory = true;

    void Awake()
    {
        Bind();
        if (shopPanel && inventoryPanel && shopPanel != inventoryPanel)
        {
            if (startWithInventory) ShowInventory(); else ShowShop();
        }
    }

    public void Bind()
    {
        Configure(shopButton);
        Configure(inventoryButton);
        if (shopButton) { shopButton.onClick.RemoveListener(ShowShop); shopButton.onClick.AddListener(ShowShop); }
        if (inventoryButton) { inventoryButton.onClick.RemoveListener(ShowInventory); inventoryButton.onClick.AddListener(ShowInventory); }
    }

    static void Configure(Button button)
    {
        if (!button) return;
        button.interactable = true;
        button.transition = Selectable.Transition.None;
        var image = button.targetGraphic as Graphic;
        if (image) { image.raycastTarget = true; image.color = Color.white; }
    }

    public void ShowShop() { Switch(true); }
    public void ShowInventory() { Switch(false); }

    void Switch(bool showShop)
    {
        if (!shopPanel || !inventoryPanel || shopPanel == inventoryPanel)
        {
            Debug.LogWarning("[Tab Repair] Missing or identical Shop/Inventory panels. Assign them in Inspector.", this);
            return;
        }
        shopPanel.SetActive(showShop);
        inventoryPanel.SetActive(!showShop);
        Configure(shopButton);
        Configure(inventoryButton);
    }
}
