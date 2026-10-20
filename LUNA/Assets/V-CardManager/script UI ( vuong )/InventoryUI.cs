using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Inventory Data")]
    [SerializeField] private CardInventory cardInventory;

    [Header("UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform inventoryCardGrid;
    [SerializeField] private GameObject inventoryCardPrefab;
    [SerializeField] private GameObject storeUI;

    [Header("Store")]
    [SerializeField] private GameObject storePanel;

    // OPEN INVENTORY

    public void OpenInventory()
    {
        // Tắt Store
        if (storeUI != null)
        {
            storeUI.SetActive(false);
        }

        // Bật Inventory
        gameObject.SetActive(true);

        // Bật panel Inventory
        inventoryPanel.SetActive(true);

        // Cập nhật Card
        RefreshInventory();
    }


    // CLOSE INVENTORY

    public void CloseInventory()
    {
        // Tắt Inventory
        gameObject.SetActive(false);

        // Bật Store
        if (storeUI != null)
        {
            storeUI.SetActive(true);
        }
    }

    // REFRESH INVENTORY

    public void RefreshInventory()
    {
        ClearInventoryCards();

        if (cardInventory == null)
        {
            Debug.LogError("InventoryUI: CardInventory chưa được gán.");
            return;
        }

        if (inventoryCardGrid == null)
        {
            Debug.LogError("InventoryUI: InventoryCardGrid chưa được gán.");
            return;
        }

        if (inventoryCardPrefab == null)
        {
            Debug.LogError("InventoryUI: InventoryCardPrefab chưa được gán.");
            return;
        }

        var cards = cardInventory.GetAllCards();

        Debug.Log("InventoryUI: Số Card lấy được = " + cards.Count);

        for (int i = 0; i < cards.Count; i++)
        {
            CardInventoryEntry entry = cards[i];

            Debug.Log(
                "InventoryUI: Đang tạo UI cho Card = " +
                entry.card.cardName +
                " | Quantity = " +
                entry.quantity
            );

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

                Debug.Log(
                    "InventoryUI: Tạo UI thành công cho " +
                    entry.card.cardName
                );
            }
            else
            {
                Debug.LogError(
                    "InventoryUI: Prefab KHÔNG có InventoryCardUI!"
                );
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