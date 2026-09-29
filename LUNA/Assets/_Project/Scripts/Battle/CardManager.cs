using System.Collections;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Tham chiếu")]
    public TurnManager turnManager;
    public HandManager handManager;
    public DeckManager deckManager;
    public CardEffectManager cardEffectManager;
    public TimingManager timingManager;

    [Header("Runtime State (Chỉ để xem, không tự sửa)")]
    public CardUI selectedCard;

    public bool IsBusy { get; private set; }

    private void OnEnable()
    {
        if (deckManager != null)
        {
            deckManager.OnHandChanged += HandleHandChanged;
        }
    }

    private void OnDisable()
    {
        if (deckManager != null)
        {
            deckManager.OnHandChanged -= HandleHandChanged;
        }
    }

    public void OnCardClicked(CardInputHandler clickedCard)
    {
        if (IsBusy) return;

        CardUI clickedUI = clickedCard.GetCardUI();

        if (clickedUI == null || clickedUI.cardData == null)
        {
            return;
        }

        if (!turnManager.CanAfford(clickedUI.cardData))
        {
            Debug.Log("CardManager: Không đủ Energy để dùng Card '" + clickedUI.cardData.cardName + "'.");
            return;
        }

        if (timingManager == null || cardEffectManager == null)
        {
            Debug.LogError("CardManager: Thiếu tham chiếu TimingManager hoặc CardEffectManager trong Inspector.");
            return;
        }

        selectedCard = clickedUI;
        handManager.SetSelectedCard(clickedUI);

        StartCoroutine(PlayCardRoutine(clickedUI.cardData));
    }

    private IEnumerator PlayCardRoutine(CardData card)
    {
        IsBusy = true;

        Debug.Log("CardManager: Bắt đầu dùng Card '" + card.cardName + "'.");

        turnManager.SpendEnergy(card.cost);

        int timingCount = GetTimingCount(card);
        TimingResult[] results = new TimingResult[Mathf.Max(1, timingCount)];

        for (int i = 0; i < results.Length; i++)
        {
            results[i] = TimingResult.Good;
        }

        float bonusPercent = 0f;
        if (timingCount > 0)
        {
            bonusPercent = cardEffectManager.pendingTimingWindowBonusPercent;
            cardEffectManager.pendingTimingWindowBonusPercent = 0f;
        }

        for (int i = 0; i < timingCount; i++)
        {
            bool finished = false;
            TimingResult hitResult = TimingResult.Miss;

            string hitLabel = timingCount > 1 ? " (Hit " + (i + 1) + "/" + timingCount + ")" : "";

            timingManager.StartTiming(card, bonusPercent, hitLabel, (r) =>
            {
                hitResult = r;
                finished = true;
            });

            yield return new WaitUntil(() => finished);

            results[i] = hitResult;
        }

        deckManager.PlayCard(card);
        cardEffectManager.ResolveCard(card, results);
        deckManager.DiscardCard(card);

        selectedCard = null;
        IsBusy = false;
    }

    private int GetTimingCount(CardData card)
    {
        switch (card.effectType)
        {
            case CardEffectType.Damage:
            case CardEffectType.Poison:
                return 1;

            case CardEffectType.DoubleHit:
                return Mathf.Max(1, card.extraValue);

            default:
                return 0;
        }
    }

    private void HandleHandChanged()
    {
        selectedCard = null;
    }
}