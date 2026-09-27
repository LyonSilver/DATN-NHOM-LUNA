using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    [Header("Tham chiếu")]
    public DeckManager deckManager;
    public RectTransform handContainer;
    public GameObject cardPrefab;

    [Header("Bố cục Hand (Đơn vị Pixel, màn hình 1920x1080)")]
    public float cardSpacing = 300f;
    public float cardPositionY = 0f;

    private List<CardUI> spawnedCards = new List<CardUI>();

    private void OnEnable()
    {
        if (deckManager != null)
        {
            deckManager.OnHandChanged += RefreshHand;
        }
    }

    private void OnDisable()
    {
        if (deckManager != null)
        {
            deckManager.OnHandChanged -= RefreshHand;
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

            RectTransform rt = cardGO.GetComponent<RectTransform>();
            float xPos = GetXPositionForIndex(i, count);
            rt.anchoredPosition = new Vector2(xPos, cardPositionY);

            spawnedCards.Add(cardUI);
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
    }
}