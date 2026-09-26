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
    [SerializeField] private List<CardInventoryEntry> cards =
        new List<CardInventoryEntry>();

    public List<CardInventoryEntry> GetAllCards()
    {
        return cards;
    }

    public int GetCardQuantity(CardData card)
    {
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

        int currentQuantity = GetCardQuantity(card);

        return currentQuantity < card.maxQuantity;
    }

    public bool AddCard(CardData card)
    {
        if (!CanAddCard(card))
        {
            return false;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].card == card)
            {
                cards[i].quantity++;
                return true;
            }
        }

        CardInventoryEntry newEntry =
            new CardInventoryEntry(card, 1);

        cards.Add(newEntry);

        return true;
    }
}