using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Tham chieu")]
    public TurnManager turnManager;
    public HandManager handManager;
    public DeckManager deckManager;

    [Header("Runtime State (Chỉ để xem, không chỉnh sửa)")]
    public CardUI selectedCard;

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
        CardUI clickedUI = clickedCard.GetCardUI();

        if (clickedUI == null || clickedUI.cardData == null)
        {
            return;
        }

        if (selectedCard == clickedUI)
        {
            Deselect();
            return;
        }

        if (!turnManager.CanAfford(clickedUI.cardData))
        {
            Debug.Log("CardManager: Không đủ Energy để chọn Card '" + clickedUI.cardData.cardName + "'.");
            return;
        }

        selectedCard = clickedUI;
        handManager.SetSelectedCard(clickedUI);

        Debug.Log("CardManager: Đã chọn Card '" + clickedUI.cardData.cardName +
                   "'. Cho kết nối Timing/Effect.");

        // TO-DO List:
        // 1. turnManager.SpendEnergy(clickedUI.cardData.cost);
        // 2. TimingManager.StartTiming(clickedUI.cardData);
        // 3. Sau khi co Timing Result -> CardEffectManager.ResolveCard(...)
        // 4. deckManager.PlayCard(...) + deckManager.DiscardCard(...)
    }

    public void Deselect()
    {
        selectedCard = null;
        handManager.SetSelectedCard(null);
    }

    private void HandleHandChanged()
    {
        selectedCard = null;
    }
}