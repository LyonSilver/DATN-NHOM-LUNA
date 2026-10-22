
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandManager : MonoBehaviour
{
    [Header("Tham chiếu")]
    public DeckManager deckManager;
    public TurnManager turnManager;
    public CardManager cardManager;
    public RectTransform handContainer;
    public GameObject cardPrefab;

    [Header("Bố cục Hand - Fan")]
    public float cardSpacing = 70f;
    public float maxAngle = 18f;
    public float arcHeight = 30f;
    public float cardPositionY = 0f;

    [Header("Card Audio")]
    [SerializeField] private AudioClip cardClickClip;

    [Range(0f, 1f)]
    [SerializeField] private float cardClickVolume = 1f;

    private List<CardUI> spawnedCards =
        new List<CardUI>();

    // =========================
    // UNITY LIFECYCLE
    // =========================

    private void OnEnable()
    {
        if (deckManager != null)
            deckManager.OnHandChanged += RefreshHand;

        if (turnManager != null)
            turnManager.OnEnergyChanged +=
                UpdateCardAffordability;
    }

    private void OnDisable()
    {
        if (deckManager != null)
            deckManager.OnHandChanged -= RefreshHand;

        if (turnManager != null)
            turnManager.OnEnergyChanged -=
                UpdateCardAffordability;
    }

    private void Start()
    {
        if (deckManager != null)
            deckManager.InitializeDeck();
    }

    // =========================
    // REFRESH HAND
    // =========================

    public void RefreshHand()
    {
        if (deckManager == null ||
            handContainer == null ||
            cardPrefab == null)
        {
            Debug.LogWarning(
                "HandManager: Thiếu tham chiếu!"
            );
            return;
        }

        ClearSpawnedCards();

        List<CardData> hand = deckManager.hand;

        if (hand == null)
            return;

        int count = hand.Count;

        for (int i = 0; i < count; i++)
        {
            GameObject cardGO = Instantiate(
                cardPrefab,
                handContainer
            );

            CardUI cardUI =
                cardGO.GetComponent<CardUI>();

            if (cardUI == null)
            {
                Debug.LogError(
                    "HandManager: Card Prefab thiếu CardUI!"
                );

                Destroy(cardGO);
                continue;
            }

            // Gán dữ liệu Card
            cardUI.Setup(hand[i]);

            // Khởi tạo Input Card như cũ
            CardInputHandler inputHandler =
                cardGO.GetComponent<CardInputHandler>();

            if (inputHandler != null)
            {
                inputHandler.Init(cardManager);
            }

            // Tự động gắn sự kiện Click Audio
            RegisterCardClickSound(cardGO, cardUI);

            // Bố trí Card hình quạt
            RectTransform rt =
                cardGO.GetComponent<RectTransform>();

            if (rt != null)
            {
                LayoutCard(rt, i, count);
                rt.SetSiblingIndex(i);
            }

            spawnedCards.Add(cardUI);
        }

        UpdateCardAffordability();
    }

    // =========================
    // CARD CLICK AUDIO
    // =========================

    private void RegisterCardClickSound(
        GameObject cardGO,
        CardUI cardUI)
    {
        EventTrigger trigger =
            cardGO.GetComponent<EventTrigger>();

        if (trigger == null)
        {
            trigger =
                cardGO.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entry =
            new EventTrigger.Entry();

        entry.eventID = EventTriggerType.PointerClick;

        entry.callback.AddListener((eventData) =>
        {
            PointerEventData pointerData =
                eventData as PointerEventData;

            if (pointerData == null)
                return;

            // Chỉ phát khi Click chuột trái
            if (pointerData.button !=
                PointerEventData.InputButton.Left)
                return;

            if (cardUI == null ||
                cardUI.cardData == null)
                return;

            // Card không đủ Energy thì không phát
            if (turnManager != null &&
                !turnManager.CanAfford(cardUI.cardData))
                return;

            PlayCardClickSound();
        });

        trigger.triggers.Add(entry);
    }

    public void PlayCardClickSound()
    {
        if (cardClickClip == null)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                cardClickClip,
                cardClickVolume
            );
        }
        else
        {
            Debug.LogWarning(
                "HandManager: Không tìm thấy AudioManager!"
            );
        }
    }

    // =========================
    // FAN LAYOUT
    // =========================

    private void LayoutCard(
        RectTransform rt,
        int index,
        int count)
    {
        if (rt == null || count <= 0)
            return;

        if (count == 1)
        {
            rt.anchoredPosition =
                new Vector2(0f, cardPositionY);

            rt.localEulerAngles = Vector3.zero;
            return;
        }

        float mid = (count - 1) * 0.5f;
        float offset = index - mid;

        float x = offset * cardSpacing;

        float y =
            (1f - Mathf.Abs(offset) / mid)
            * arcHeight;

        float angle =
            offset * (maxAngle / mid);

        rt.anchoredPosition =
            new Vector2(x, y + cardPositionY);

        rt.localEulerAngles =
            new Vector3(0f, 0f, -angle);
    }

    // =========================
    // ENERGY
    // =========================

    public void UpdateCardAffordability()
    {
        if (turnManager == null)
            return;

        foreach (CardUI card in spawnedCards)
        {
            if (card == null ||
                card.cardData == null)
                continue;

            bool affordable =
                turnManager.CanAfford(card.cardData);

            card.SetDisabled(!affordable);
            card.SetInteractable(affordable);
        }
    }

    // =========================
    // SELECT CARD
    // =========================

    public void SetSelectedCard(CardUI selected)
    {
        foreach (CardUI card in spawnedCards)
        {
            if (card == null)
                continue;

            CardInputHandler inputHandler =
                card.GetComponent<CardInputHandler>();

            bool isSelected = card == selected;

            if (inputHandler != null)
            {
                inputHandler.SetSelectedVisual(
                    isSelected
                );
            }

            card.SetDimmed(
                selected != null && !isSelected
            );
        }
    }

    // =========================
    // CLEAR CARDS
    // =========================

    private void ClearSpawnedCards()
    {
        foreach (CardUI card in spawnedCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        spawnedCards.Clear();
    }

    // =========================
    // TEST CARD
    // =========================

    [ContextMenu("TEST - Play First Affordable Card In Hand")]
    private void TestPlayFirstAffordableCard()
    {
        if (deckManager == null ||
            turnManager == null)
        {
            Debug.LogWarning(
                "HandManager TEST: Thiếu Manager!"
            );
            return;
        }

        CardData cardToPlay = null;

        foreach (CardData card in deckManager.hand)
        {
            if (card != null &&
                turnManager.CanAfford(card))
            {
                cardToPlay = card;
                break;
            }
        }

        if (cardToPlay == null)
        {
            Debug.Log(
                "Không có Card đủ Energy."
            );
            return;
        }

        turnManager.SpendEnergy(cardToPlay.cost);

        deckManager.PlayCard(cardToPlay);
        deckManager.DiscardCard(cardToPlay);

        Debug.Log(
            "Đã chơi Card: " +
            cardToPlay.cardName +
            " | Energy còn: " +
            turnManager.currentEnergy + "/" +
            turnManager.maxEnergy
        );
    }
}
