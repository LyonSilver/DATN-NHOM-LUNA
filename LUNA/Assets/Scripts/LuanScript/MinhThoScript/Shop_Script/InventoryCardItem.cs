using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class InventoryCardItem : MonoBehaviour
{
    [Header("Card UI")]
    [SerializeField] private Image cardArtwork;
    [SerializeField] private Image cardTypeIcon;
    [SerializeField] private TMP_Text tierLabel;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text quantityText;

    [Header("Button")]
    [SerializeField] private Button selectButton;

    public CardData Card { get; private set; }

    private InventoryScreen inventoryScreen;

    public void Setup(CardData card, int quantity,
                      InventoryScreen owner)
    {
        Card = card;
        inventoryScreen = owner;

        if (card == null)
            return;

        if (cardArtwork != null)
            cardArtwork.sprite = card.artwork;

        if (cardTypeIcon != null)
            cardTypeIcon.sprite = card.typeIcon;

        if (tierLabel != null)
            tierLabel.text = card.tier.ToString();

        if (cardName != null)
            cardName.text = card.cardName;

        if (quantityText != null)
            quantityText.text = "x" + quantity;

        if (selectButton == null)
            selectButton = GetComponent<Button>();

        selectButton.onClick.RemoveListener(OnSelected);
        selectButton.onClick.AddListener(OnSelected);
    }

    private void OnSelected()
    {
        if (inventoryScreen != null && Card != null)
        {
            inventoryScreen.SelectCard(Card);
        }
    }

    private void OnDestroy()
    {
        if (selectButton != null)
            selectButton.onClick.RemoveListener(OnSelected);
    }
}