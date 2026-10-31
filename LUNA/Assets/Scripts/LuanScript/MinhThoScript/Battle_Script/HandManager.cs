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

    [Header("Bố cục Hand - Fan")]
    public float cardSpacing = 70f;      // Khoảng cách giữa tâm các lá (thử 60~90)
    public float maxAngle = 18f;         // Góc xoay tối đa của 2 lá ngoài cùng
    public float arcHeight = 30f;        // Độ cong của hàng bài (0 = thẳng)
    public float cardPositionY = 0f;     // Offset Y chung của cả hàng bài

    private List<CardUI> spawnedCards = new List<CardUI>();
    private CardUI currentSelected;

    private void OnEnable()
    {
        if (deckManager != null)
            deckManager.OnHandChanged += RefreshHand;

        if (turnManager != null)
            turnManager.OnEnergyChanged += UpdateCardAffordability;
    }

    private void OnDisable()
    {
        if (deckManager != null)
            deckManager.OnHandChanged -= RefreshHand;

        if (turnManager != null)
            turnManager.OnEnergyChanged -= UpdateCardAffordability;
    }

    private void Start()
    {
        if (deckManager != null)
            deckManager.InitializeDeck();
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
                inputHandler.Init(cardManager);

            RectTransform rt = cardGO.GetComponent<RectTransform>();
            LayoutCard(rt, i, count);

            // Sắp xếp thứ tự vẽ (lá bên phải đè lên bên trái)
            rt.SetSiblingIndex(i);

            spawnedCards.Add(cardUI);
        }

        UpdateCardAffordability();
    }

    private void LayoutCard(RectTransform rt, int index, int count)
    {
        if (count <= 0) return;

        // Chỉ có 1 lá
        if (count == 1)
        {
            rt.anchoredPosition = new Vector2(0f, cardPositionY);
            rt.localEulerAngles = Vector3.zero;
            return;
        }

        float mid = (count - 1) * 0.5f;
        float offset = index - mid;

        // X đều
        float x = offset * cardSpacing;

        // Y: giữa CAO hơn hai đầu (đổi dấu nếu muốn ngược lại)
        float y = (1f - Mathf.Abs(offset) / mid) * arcHeight;

        // Góc xoay
        float angle = offset * (maxAngle / mid);

        rt.anchoredPosition = new Vector2(x, y + cardPositionY);
        rt.localEulerAngles = new Vector3(0f, 0f, -angle);
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
                inputHandler.SetSelectedVisual(isSelected);

            card.SetDimmed(selected != null && !isSelected);
        }
    }

    private void ClearSpawnedCards()
    {
        foreach (CardUI card in spawnedCards)
        {
            if (card != null)
                Destroy(card.gameObject);
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