using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryScreen : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private CardInventory cardInventory;
    [SerializeField] private int maxCapacity = 250;

    [Header("Card List")]
    [SerializeField] private InventoryCardItem cardPrefab;
    [SerializeField] private Transform content;
    [SerializeField] private TMP_Text capacityText;

    [Header("Card Detail")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image artworkImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text elementText;
    [SerializeField] private TMP_Text tierText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text statsText;

    [Header("Element Buttons")]
    [SerializeField] private Button metalButton;
    [SerializeField] private Button woodButton;
    [SerializeField] private Button waterButton;
    [SerializeField] private Button fireButton;
    [SerializeField] private Button earthButton;

    private CardElement currentElement = CardElement.None;
    private CardData selectedCard;

    private void Awake()
    {
        if (metalButton != null)
            metalButton.onClick.AddListener(
                () => FilterElement(CardElement.Metal));

        if (woodButton != null)
            woodButton.onClick.AddListener(
                () => FilterElement(CardElement.Wood));

        if (waterButton != null)
            waterButton.onClick.AddListener(
                () => FilterElement(CardElement.Water));

        if (fireButton != null)
            fireButton.onClick.AddListener(
                () => FilterElement(CardElement.Fire));

        if (earthButton != null)
            earthButton.onClick.AddListener(
                () => FilterElement(CardElement.Earth));
    }

    private void OnEnable()
    {
        RefreshInventory();
    }

    // ==============================
    // REFRESH INVENTORY
    // ==============================

    public void RefreshInventory()
    {
        if (cardInventory == null ||
            cardPrefab == null ||
            content == null)
        {
            Debug.LogWarning(
                "InventoryScreen: Chưa gán đủ Inspector.");

            return;
        }

        ClearContent();

        int totalCards = 0;
        CardData firstCard = null;
        bool selectedVisible = false;

        foreach (CardInventoryEntry entry
                 in cardInventory.GetAllCards())
        {
            if (entry == null ||
                entry.card == null ||
                entry.quantity <= 0)
                continue;

            totalCards += entry.quantity;

            if (currentElement != CardElement.None &&
                entry.card.element != currentElement)
                continue;

            InventoryCardItem item =
                Instantiate(cardPrefab, content);

            item.Setup(entry.card, entry.quantity, this);

            if (firstCard == null)
                firstCard = entry.card;

            if (entry.card == selectedCard)
                selectedVisible = true;
        }

        if (capacityText != null)
        {
            capacityText.text =
                $"CAPACITY {totalCards}/{maxCapacity}";
        }

        if (selectedVisible)
            SelectCard(selectedCard);
        else
            SelectCard(firstCard);
    }

    // ==============================
    // SELECT CARD
    // ==============================

    public void SelectCard(CardData card)
    {
        selectedCard = card;

        if (detailPanel != null)
            detailPanel.SetActive(card != null);

        if (card == null)
            return;

        if (artworkImage != null)
            artworkImage.sprite = card.artwork;

        if (cardNameText != null)
            cardNameText.text = card.cardName;

        if (elementText != null)
            elementText.text = card.element.ToString();

        if (tierText != null)
            tierText.text = card.tier.ToString();

        if (descriptionText != null)
            descriptionText.text = card.description;

        if (statsText != null)
        {
            statsText.text =
                $"Damage: {card.baseDamage}\n" +
                $"Shield: {card.shieldValue}\n" +
                $"Heal: {card.healValue}\n" +
                $"Status: {card.statusEffect}\n" +
                $"Duration: {card.statusDuration}\n" +
                $"Owned: {cardInventory.GetCardQuantity(card)}";
        }
    }

    // ==============================
    // FILTER ELEMENT
    // ==============================

    public void FilterElement(CardElement element)
    {
        // Nhấn lại cùng hệ để hiện tất cả thẻ.
        currentElement = currentElement == element
            ? CardElement.None
            : element;

        RefreshInventory();
    }

    public void ShowAllCards()
    {
        currentElement = CardElement.None;
        RefreshInventory();
    }

    // ==============================
    // CLEAR CONTENT
    // ==============================

    private void ClearContent()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject child = content.GetChild(i).gameObject;

            child.SetActive(false);
            Destroy(child);
        }
    }
}