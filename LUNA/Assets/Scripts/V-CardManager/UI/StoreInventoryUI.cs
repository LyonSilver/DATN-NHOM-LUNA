using UnityEngine;

public class StoreInventoryUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject storeUI;
    [SerializeField] private GameObject inventoryUI;

    private void Start()
    {
        ShowStore();
    }

    public void ShowInventory()
    {
        storeUI.SetActive(false);
        inventoryUI.SetActive(true);
    }

    public void ShowStore()
    {
        inventoryUI.SetActive(false);
        storeUI.SetActive(true);
    }
}