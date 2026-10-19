using TMPro;
using UnityEngine;

public class InventoryCardUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardTypeText;
    [SerializeField] private TMP_Text quantityText;

    public void Setup(CardInventoryEntry entry)
    {
        if (entry == null || entry.card == null)
        {
            return;
        }

        cardNameText.text = entry.card.cardName;
        cardTypeText.text = entry.card.cardType.ToString();

        quantityText.text =
            "x" + entry.quantity +
            " / " + entry.card.maxQuantity;
    }
}