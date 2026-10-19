using System.Collections;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Tham chieu")]
    public TurnManager turnManager;
    public HandManager handManager;
    public DeckManager deckManager;
    public CardEffectManager cardEffectManager;
    public BattleStateMachine battleStateMachine;

    [Header("Card Use")]
    [SerializeField] private float useDelay = 0.5f;

    [Header("Runtime State")]
    public CardUI selectedCard;

    private bool isUsingCard = false;

    private void OnEnable()
    {
        if (deckManager != null)
            deckManager.OnHandChanged += HandleHandChanged;
    }

    private void OnDisable()
    {
        if (deckManager != null)
            deckManager.OnHandChanged -= HandleHandChanged;
    }

    public void OnCardClicked(CardInputHandler clickedCard)
    {
        // Chỉ được dùng bài trong lượt người chơi.
        if (battleStateMachine == null)
        {
            Debug.LogError(
                "CardManager: Chưa gán BattleStateMachine!"
            );
            return;
        }

        if (!battleStateMachine.IsPlayerTurn())
        {
            Debug.Log("CardManager: Hiện không phải lượt người chơi.");
            return;
        }

        if (isUsingCard)
        {
            Debug.Log("CardManager: Đang sử dụng bài.");
            return;
        }

        if (clickedCard == null)
            return;

        if (turnManager == null ||
            handManager == null ||
            deckManager == null ||
            cardEffectManager == null)
        {
            Debug.LogError(
                "CardManager: Chưa gán đủ các Manager trong Inspector!"
            );
            return;
        }

        CardUI clickedUI = clickedCard.GetCardUI();

        if (clickedUI == null || clickedUI.cardData == null)
            return;

        CardData card = clickedUI.cardData;

        // Kiểm tra Energy trước khi bắt đầu sử dụng.
        if (!turnManager.CanAfford(card))
        {
            Debug.Log(
                "CardManager: Không đủ Energy cho " + card.cardName
            );
            return;
        }

        selectedCard = clickedUI;
        handManager.SetSelectedCard(clickedUI);

        StartCoroutine(UseCard(card, clickedUI));
    }

    private IEnumerator UseCard(CardData card, CardUI cardUI)
    {
        isUsingCard = true;

        Debug.Log(
            "CardManager: Chuẩn bị sử dụng " + card.cardName
        );

        // Giữ khoảng chờ để sau này thêm animation.
        yield return new WaitForSeconds(useDelay);

        // Kiểm tra lại trạng thái sau khoảng chờ.
        if (battleStateMachine == null ||
            !battleStateMachine.IsPlayerTurn())
        {
            ResetSelection();
            isUsingCard = false;
            yield break;
        }

        // Đảm bảo đủ Energy tại thời điểm thực sự dùng bài.
        if (!turnManager.CanAfford(card))
        {
            ResetSelection();
            isUsingCard = false;
            yield break;
        }

        // Trừ Energy đúng một lần.
        if (!turnManager.SpendEnergy(card.cost))
        {
            ResetSelection();
            isUsingCard = false;
            yield break;
        }

        // Thực hiện hiệu ứng bài.
        cardEffectManager.ExecuteCardEffect(card);

        // Đưa bài đã sử dụng ra khỏi hand.
        deckManager.PlayCard(card);

        ResetSelection();
        isUsingCard = false;

        Debug.Log(
            "CardManager: Đã sử dụng xong " + card.cardName
        );
    }

    private void ResetSelection()
    {
        selectedCard = null;

        if (handManager != null)
            handManager.SetSelectedCard(null);
    }

    public void Deselect()
    {
        if (isUsingCard)
            return;

        ResetSelection();
    }

    private void HandleHandChanged()
    {
        if (!isUsingCard)
            selectedCard = null;
    }
}