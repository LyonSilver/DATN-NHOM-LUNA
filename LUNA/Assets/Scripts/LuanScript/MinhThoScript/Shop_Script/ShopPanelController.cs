using UnityEngine;
using UnityEngine.UI;

public class ShopPanelController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject inventoryPanel;

    [Header("Button")]
    [SerializeField] private Button toggleButton;

    private bool isShopOpen = true;

    private void Start()
    {
        UpdatePanel();
    }

    private void OnEnable()
    {
        toggleButton.onClick.AddListener(TogglePanel);
    }

    private void OnDisable()
    {
        toggleButton.onClick.RemoveListener(TogglePanel);
    }

    public void TogglePanel()
    {
        isShopOpen = !isShopOpen;
        UpdatePanel();
    }

    private void UpdatePanel()
    {
        shopPanel.SetActive(isShopOpen);
        inventoryPanel.SetActive(!isShopOpen);
    }
}