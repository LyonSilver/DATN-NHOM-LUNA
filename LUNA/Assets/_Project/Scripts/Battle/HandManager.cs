using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    [Header("Tham chiếu")]
    public DeckManager deckManager;
    public TurnManager turnManager;
    public CardManager cardManager;
    public RectTransform handContainer;
    public GameObject cardPrefab;

    [Header("Bố cục Hand (Đơn vị Pixel, màn hình 1920x1080)")]
    public float cardSpacing = 300f;
    public float cardPositionY = 0f;

    private List<CardUI> spawnedCards = new List<CardUI>();
    private CardUI currentSelected;

    private void OnEnable()
    {
        if (deckManager != null)
        {
            deckManager.OnHandChanged += RefreshHand;
        }

        if (turnManager != null)
        {
            turnManager.OnEnergyChanged += UpdateCardAffordability;
        }
    }

    private void OnDisable()
    {
        if (deckManager != null)
        {
            deckManager.OnHandChanged -= RefreshHand;
        }

        if (turnManager != null)
        {
            turnManager.OnEnergyChanged -= UpdateCardAffordability;
        }
    }

    private void Start()
    {
        if (deckManager != null)
        {
            deckManager.InitializeDeck();
        }
    }

    public void RefreshHand()
    {
        ClearSpawnedCards();

        List<CardData> hand = deckManager.hand;
        int count = hand.Count;

        for (int i = 0; i < count; i++)
        {
            GameObject cardGO = Instantiate(cardPrefab, handContainer);
            CardUI cardUI = cardGO.GetComponent<CardUI>();

            if (cardUI == null)
            {
                Debug.LogError("HandManager: cardPrefab không có component CardUI. Kiểm tra lại Card.prefab.");
                continue;
            }

            cardUI.Setup(hand[i]);

            CardInputHandler inputHandler = cardGO.GetComponent<CardInputHandler>();
            if (inputHandler != null)
            {
                inputHandler.Init(cardManager);
            }

            RectTransform rt = cardGO.GetComponent<RectTransform>();
            float xPos = GetXPositionForIndex(i, count);
            rt.anchoredPosition = new Vector2(xPos, cardPositionY);

            spawnedCards.Add(cardUI);
        }

        UpdateCardAffordability();
    }

    public void UpdateCardAffordability()
    {
        if (turnManager == null) return;

        foreach (CardUI card in spawnedCards)
        {
            if (card == null || card.cardData == null) continue;
            if (card == currentSelected) continue;

            bool affordable = turnManager.CanAfford(card.cardData);
            card.SetDisabled(!affordable);
            card.SetInteractable(affordable);
        }
    }

    public void SetSelectedCard(CardUI selected)
    {
        currentSelected = selected;

        foreach (CardUI card in spawnedCards)
        {
            if (card == null) continue;

            CardInputHandler inputHandler = card.GetComponent<CardInputHandler>();
            bool isSelected = card == selected;

            if (inputHandler != null)
            {
                inputHandler.SetSelectedVisual(isSelected);
            }

            card.SetDimmed(selected != null && !isSelected);
        }
    }

    private float GetXPositionForIndex(int index, int count)
    {
        float offsetFromCenter = index - (count - 1) / 2f;
        return offsetFromCenter * cardSpacing;
    }

    private void ClearSpawnedCards()
    {
        foreach (CardUI card in spawnedCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        spawnedCards.Clear();
        currentSelected = null;
    }

    [ContextMenu("TEST - Play First Affordable Card In Hand")]
    private void TestPlayFirstAffordableCard()
    {
        if (deckManager == null || turnManager == null)
        {
            Debug.LogWarning("HandManager TEST: Thiếu tham chiếu DeckManager hoặc TurnManager.");
            return;
        }

        CardData cardToPlay = null;

        foreach (CardData card in deckManager.hand)
        {
            if (turnManager.CanAfford(card))
            {
                cardToPlay = card;
                break;
            }
        }

        if (cardToPlay == null)
        {
            Debug.Log("HandManager TEST: Không có Card nào đủ Energy để chơi lúc này.");
            return;
        }

        turnManager.SpendEnergy(cardToPlay.cost);
        deckManager.PlayCard(cardToPlay);
        deckManager.DiscardCard(cardToPlay);

        Debug.Log("HandManager TEST: Đã chơi và discard Card '" + cardToPlay.cardName +
                   "' (Cost " + cardToPlay.cost + "). Energy còn lại: " + turnManager.currentEnergy + "/" + turnManager.maxEnergy);
    }
}