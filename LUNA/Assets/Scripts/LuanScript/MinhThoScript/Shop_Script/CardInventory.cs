using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardInventoryEntry
{
    public CardData card;
    public int quantity;

    public CardInventoryEntry(CardData card, int quantity)
    {
        this.card = card;
        this.quantity = quantity;
    }
}

public class CardInventory : MonoBehaviour
{
    [Header("Owned Cards")]
    [SerializeField] private List<CardInventoryEntry> cards = new List<CardInventoryEntry>();

    public List<CardInventoryEntry> GetAllCards()
    {
        return cards;
    }

    public int GetCardQuantity(CardData card)
    {
        if (card == null)
        {
            return 0;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].card == card)
            {
                return cards[i].quantity;
            }
        }

        return 0;
    }

    public bool CanAddCard(CardData card)
    {
        if (card == null)
        {
            return false;
        }

        return GetCardQuantity(card) < card.maxQuantity;
    }

    public bool AddCard(CardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("CardInventory: Không thể thêm Card null.");
            return false;
        }

        if (!CanAddCard(card))
        {
            Debug.LogWarning("CardInventory: Không thể thêm Card: " + card.cardName);
            return false;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].card == card)
            {
                cards[i].quantity++;

                Debug.Log("CardInventory: Đã thêm " + card.cardName + " | Quantity: " + cards[i].quantity);
                return true;
            }
        }

        cards.Add(new CardInventoryEntry(card, 1));

        Debug.Log("CardInventory: Đã thêm Card mới: " + card.cardName + " | Quantity: 1");
        return true;
    }
}