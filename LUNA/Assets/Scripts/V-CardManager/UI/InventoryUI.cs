using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private CardInventory cardInventory;

    [Header("UI")]
    [SerializeField] private Transform inventoryCardGrid;
    [SerializeField] private GameObject inventoryCardPrefab;

    private void OnEnable()
    {
        RefreshInventory();
    }

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

        for (int i = 0; i < cards.Count; i++)
        {
            CardInventoryEntry entry = cards[i];

            GameObject cardObject =
                Instantiate(inventoryCardPrefab, inventoryCardGrid);

            InventoryCardUI cardUI =
                cardObject.GetComponent<InventoryCardUI>();

            cardUI.Setup(entry);
        }
    }

    private void ClearInventoryCards()
    {
        if (inventoryCardGrid == null)
        {
            return;
        }

        for (int i = inventoryCardGrid.childCount - 1; i >= 0; i--)
        {
            Destroy(inventoryCardGrid.GetChild(i).gameObject);
        }
    }
}