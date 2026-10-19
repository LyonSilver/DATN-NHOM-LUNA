
using System.Collections;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Managers")]
    public TurnManager turnManager;
    public HandManager handManager;
    public DeckManager deckManager;
    public CardEffectManager cardEffectManager;
    public BattleStateMachine battleStateMachine;

    [Header("Player Card Cinematic")]
    [SerializeField]
    private PlayerCardCinematic playerCardCinematic;

    [Header("Card Use")]
    [SerializeField] private float useDelay = 0.5f;

    [Header("Runtime State")]
    public CardUI selectedCard;

    private bool isUsingCard = false;

    // Cho phép các Manager khác kiểm tra
    // Player có đang thực hiện Card hay không.
    public bool IsUsingCard => isUsingCard;

    // =========================================
    // ON ENABLE
    // =========================================

    private void OnEnable()
    {
        if (deckManager != null)
        {
            deckManager.OnHandChanged += HandleHandChanged;
        }
    }

    // =========================================
    // ON DISABLE
    // =========================================

    private void OnDisable()
    {
        if (deckManager != null)
        {
            deckManager.OnHandChanged -= HandleHandChanged;
        }

        // Coroutine trên GameObject bị tắt
        // không thể tiếp tục chờ Cinematic.
        StopAllCoroutines();

        isUsingCard = false;
        selectedCard = null;
    }

    // =========================================
    // CARD CLICK
    // =========================================

    public void OnCardClicked(CardInputHandler clickedCard)
    {
        // =====================================
        // 1. CHECK BATTLE STATE
        // =====================================

        if (battleStateMachine == null)
        {
            Debug.LogError(
                "CardManager: Chưa gán BattleStateMachine!"
            );
            return;
        }

        if (!battleStateMachine.IsPlayerTurn() ||
            battleStateMachine.IsBattleFinished())
        {
            Debug.Log(
                "CardManager: Hiện không phải Player Turn!"
            );
            return;
        }

        // =====================================
        // 2. CHECK CARD ACTION
        // =====================================

        if (isUsingCard)
        {
            Debug.Log(
                "CardManager: Đang thực hiện Card!"
            );
            return;
        }

        if (playerCardCinematic != null &&
            playerCardCinematic.IsPlaying)
        {
            Debug.Log(
                "CardManager: Cinematic chưa hoàn thành!"
            );
            return;
        }

        if (clickedCard == null)
            return;

        // =====================================
        // 3. VALIDATE MANAGERS
        // =====================================

        if (turnManager == null ||
            handManager == null ||
            deckManager == null ||
            cardEffectManager == null ||
            playerCardCinematic == null)
        {
            Debug.LogError(
                "CardManager: Chưa gán đầy đủ References!"
            );
            return;
        }

        // =====================================
        // 4. GET CARD DATA
        // =====================================

        CardUI clickedUI = clickedCard.GetCardUI();

        if (clickedUI == null ||
            clickedUI.cardData == null)
        {
            Debug.LogWarning(
                "CardManager: CardUI hoặc CardData bị null!"
            );
            return;
        }

        CardData card = clickedUI.cardData;

        // =====================================
        // 5. CHECK ENERGY
        // =====================================

        if (!turnManager.CanAfford(card))
        {
            Debug.Log(
                "CardManager: Không đủ Mana cho " +
                card.cardName
            );
            return;
        }

        // =====================================
        // 6. SELECT CARD
        // =====================================

        selectedCard = clickedUI;

        handManager.SetSelectedCard(clickedUI);

        // Khóa thao tác ngay khi nhấn Card.
        isUsingCard = true;

        // =====================================
        // 7. BEGIN USE CARD
        // =====================================

        StartCoroutine(
            UseCard(card, clickedUI)
        );
    }

    // =========================================
    // USE CARD COROUTINE
    // =========================================

    private IEnumerator UseCard(
        CardData card,
        CardUI cardUI)
    {
        Debug.Log(
            "CardManager: Chuẩn bị sử dụng " +
            card.cardName
        );

        // =====================================
        // 1. WAIT BEFORE CARD ACTION
        // =====================================

        yield return new WaitForSeconds(
            Mathf.Max(0f, useDelay)
        );

        // =====================================
        // 2. VALIDATE PLAYER TURN
        // =====================================

        if (battleStateMachine == null ||
            !battleStateMachine.IsPlayerTurn() ||
            battleStateMachine.IsBattleFinished())
        {
            CancelCardUse();
            yield break;
        }

        // Card có thể đã bị thay đổi hoặc xóa
        // trong lúc đang chờ.
        if (card == null ||
            cardUI == null ||
            cardUI.cardData != card)
        {
            Debug.LogWarning(
                "CardManager: Card không còn hợp lệ!"
            );

            CancelCardUse();
            yield break;
        }

        // =====================================
        // 3. CHECK ENERGY AGAIN
        // =====================================

        if (turnManager == null ||
            !turnManager.CanAfford(card))
        {
            Debug.Log(
                "CardManager: Không đủ Mana để sử dụng!"
            );

            CancelCardUse();
            yield break;
        }

        if (playerCardCinematic == null ||
            playerCardCinematic.IsPlaying)
        {
            Debug.LogWarning(
                "CardManager: Cinematic không sẵn sàng!"
            );

            CancelCardUse();
            yield break;
        }

        // =====================================
        // 4. SPEND ENERGY
        // =====================================

        if (!turnManager.SpendEnergy(card.cost))
        {
            CancelCardUse();
            yield break;
        }

        Debug.Log(
            $"CardManager: Đã trừ {card.cost} Mana."
        );

        // =====================================
        // 5. START CARD CINEMATIC
        // =====================================

        bool cinematicStarted =
            playerCardCinematic.PlayCard(card);

        if (!cinematicStarted)
        {
            Debug.LogWarning(
                "CardManager: Không thể bắt đầu Cinematic!"
            );

            // Hoàn lại Mana nếu chưa thực hiện Card.
            turnManager.RecoverEnergy(card.cost);

            CancelCardUse();
            yield break;
        }

        Debug.Log(
            "CardManager: Bắt đầu Cinematic " +
            card.cardName
        );

        // =====================================
        // 6. WAIT FOR CINEMATIC
        // =====================================

        // CardEffectManager.ExecuteCardEffect(card)
        // được gọi trong PlayerCardCinematic.
        //
        // KHÔNG gọi ExecuteCardEffect tại đây.

        while (playerCardCinematic != null &&
               playerCardCinematic.IsPlaying)
        {
            yield return null;
        }

        if (playerCardCinematic == null)
        {
            Debug.LogError(
                "CardManager: Cinematic bị mất!"
            );

            CancelCardUse();
            yield break;
        }

        // =====================================
        // 7. RESET CARD SELECTION
        // =====================================

        // Bỏ chọn trước khi xóa Card khỏi Hand.
        ResetSelection();

        // =====================================
        // 8. REMOVE USED CARD
        // =====================================

        if (deckManager != null)
        {
            deckManager.PlayCard(card);
        }

        // =====================================
        // 9. COMPLETE CARD ACTION
        // =====================================

        isUsingCard = false;

        Debug.Log(
            "CardManager: Hoàn thành Card " +
            card.cardName
        );
    }

    // =========================================
    // CANCEL CARD USE
    // =========================================

    private void CancelCardUse()
    {
        ResetSelection();

        isUsingCard = false;

        Debug.Log(
            "CardManager: Đã hủy sử dụng Card."
        );
    }

    // =========================================
    // RESET SELECTION
    // =========================================

    private void ResetSelection()
    {
        selectedCard = null;

        if (handManager != null)
        {
            handManager.SetSelectedCard(null);
        }
    }

    // =========================================
    // DESELECT CARD
    // =========================================

    public void Deselect()
    {
        if (isUsingCard)
            return;

        ResetSelection();
    }

    // =========================================
    // HANDLE HAND CHANGED
    // =========================================

    private void HandleHandChanged()
    {
        if (!isUsingCard)
        {
            selectedCard = null;
        }
    }
}
