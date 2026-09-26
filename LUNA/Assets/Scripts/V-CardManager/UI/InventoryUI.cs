using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Inventory Data")]
    [SerializeField] private CardInventory cardInventory;

    [Header("UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform inventoryCardGrid;
    [SerializeField] private GameObject inventoryCardPrefab;

    [Header("Store")]
    [SerializeField] private GameObject storePanel;

    // OPEN INVENTORY

    public void OpenInventory()
    {
        // Tắt Store
        if (storePanel != null)
        {
            storePanel.SetActive(false);
        }

        // Bật Inventory
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }

        // Cập nhật Card
        RefreshInventory();
    }


    // CLOSE INVENTORY

    public void CloseInventory()
    {
        // Tắt Inventory
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        // Bật lại Store
        if (storePanel != null)
        {
            storePanel.SetActive(true);
        }
    }

    // REFRESH INVENTORY

    public void RefreshInventory()
    {
        ClearInventoryCards();

        if (cardInventory == null)
        {
            Debug.LogError(
                "InventoryUI: CardInventory chưa được gán."
            );

            return;
        }

        if (inventoryCardGrid == null)
        {
            Debug.LogError(
                "InventoryUI: InventoryCardGrid chưa được gán."
            );

            return;
        }

        if (inventoryCardPrefab == null)
        {
            Debug.LogError(
                "InventoryUI: InventoryCardPrefab chưa được gán."
            );

            return;
        }


        var cards = cardInventory.GetAllCards();


        for (int i = 0; i < cards.Count; i++)
        {
            CardInventoryEntry entry = cards[i];


            GameObject cardObject =
                Instantiate(
                    inventoryCardPrefab,
                    inventoryCardGrid
                );


            InventoryCardUI cardUI =
                cardObject.GetComponent<InventoryCardUI>();


            if (cardUI != null)
            {
                cardUI.Setup(entry);
            }
        }
    }


 
    // CLEAR OLD CARDS
   

    private void ClearInventoryCards()
    {
        if (inventoryCardGrid == null)
        {
            return;
        }


        for (int i = inventoryCardGrid.childCount - 1; i >= 0; i--)
        {
            Destroy(
                inventoryCardGrid.GetChild(i).gameObject
            );
        }
    }
}