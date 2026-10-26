
using System.Collections;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Mission System")]
    [SerializeField] private MissionRewardManager missionRewardManager;

    [Header("Managers")]
    public TurnManager turnManager;
    public HandManager handManager;
    public DeckManager deckManager;
    public CardEffectManager cardEffectManager;
    public BattleStateMachine battleStateMachine;

    [Header("Player Card Cinematic")]
    [SerializeField]
    private PlayerCardCinematic playerCardCinematic;

    [Header("Card Use Settings")]
    [SerializeField] private float useDelay = 0.1f;

    [Header("Runtime State")]
    public CardUI selectedCard;

    private bool isUsingCard = false;

    public bool IsUsingCard => isUsingCard;

    // =========================================
    // EVENTS
    // =========================================

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

        StopAllCoroutines();

        isUsingCard = false;
        selectedCard = null;
    }

    // =========================================
    // ON CARD CLICKED
    // =========================================

    public void OnCardClicked(CardInputHandler clickedCard)
    {
        // =====================================
        // 1. CHECK PLAYER TURN
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
        // 2. CHECK CURRENT ACTION
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
                "CardManager: Cinematic chưa hoàn tất!"
            );
            return;
        }

        if (clickedCard == null)
            return;

        // =====================================
        // 3. VALIDATE REFERENCES
        // =====================================

        if (turnManager == null ||
            handManager == null ||
            deckManager == null ||
            cardEffectManager == null ||
            playerCardCinematic == null)
        {
            Debug.LogError(
                "CardManager: Chưa gán đầy đủ Managers!"
            );
            return;
        }

        // =====================================
        // 4. GET CARD
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
        // 5. GET CARD DISSOLVE
        // =====================================

        // Tìm CardDissolveUI trên Card
        // hoặc GameObject cha của Card.
        CardDissolveUI dissolve =
            clickedUI.GetComponent<CardDissolveUI>();

        if (dissolve == null)
        {
            dissolve =
                clickedUI.GetComponentInParent<CardDissolveUI>();
        }

        if (dissolve == null)
        {
            Debug.LogError(
                "CardManager: Card chưa có CardDissolveUI!"
            );
            return;
        }

        if (dissolve.IsPlaying)
        {
            Debug.Log(
                "CardManager: Card đang tan biến!"
            );
            return;
        }

        // =====================================
        // 6. CHECK ENERGY
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
        // 7. SELECT CARD
        // =====================================

        selectedCard = clickedUI;

        handManager.SetSelectedCard(clickedUI);

        // Khóa tất cả thao tác sử dụng Card.
        isUsingCard = true;

        // =====================================
        // 8. START CARD SEQUENCE
        // =====================================

        StartCoroutine(
            UseCard(card, clickedUI, dissolve)
        );
    }

    // =========================================
    // USE CARD SEQUENCE
    // =========================================

    private IEnumerator UseCard(
        CardData card,
        CardUI cardUI,
        CardDissolveUI dissolve)
    {
        Debug.Log(
            "CardManager: Chuẩn bị sử dụng " +
            card.cardName
        );

        // =====================================
        // 1. PREPARE DELAY
        // =====================================

        if (useDelay > 0f)
        {
            yield return new WaitForSeconds(useDelay);
        }

        // =====================================
        // 2. VALIDATE CARD
        // =====================================

        if (!IsCardValid(card, cardUI))
        {
            CancelCardUse(dissolve);
            yield break;
        }

        if (turnManager == null ||
            !turnManager.CanAfford(card))
        {
            Debug.Log(
                "CardManager: Không đủ Mana!"
            );

            CancelCardUse(dissolve);
            yield break;
        }

        if (playerCardCinematic == null ||
            playerCardCinematic.IsPlaying)
        {
            Debug.LogWarning(
                "CardManager: Cinematic không sẵn sàng!"
            );

            CancelCardUse(dissolve);
            yield break;
        }

        // =====================================
        // 3. SPEND ENERGY
        // =====================================

        // Chỉ trừ Mana một lần.
        if (!turnManager.SpendEnergy(card.cost))
        {
            CancelCardUse(dissolve);
            yield break;
        }

        Debug.Log(
            $"CardManager: Trừ {card.cost} Mana."
        );

        // =====================================
        // 4. PLAY CARD DISSOLVE
        // =====================================

        Debug.Log(
            "CardManager: Bắt đầu hiệu ứng Dissolve!"
        );

        if (dissolve == null)
        {
            RefundEnergy(card);
            CancelCardUse();
            yield break;
        }

        // Đợi Shader tan biến hoàn toàn.
        yield return dissolve.PlayDissolve();

        if (!dissolve.LastPlaySucceeded)
        {
            Debug.LogError(
                "CardManager: Dissolve thất bại, hủy sử dụng Card!"
            );

            RefundEnergy(card);
            CancelCardUse(dissolve);
            yield break;
        }

        Debug.Log(
            "CardManager: Dissolve hoàn tất!"
        );

        // =====================================
        // 5. VALIDATE AFTER DISSOLVE
        // =====================================

        if (!IsCardValid(card, cardUI))
        {
            RefundEnergy(card);
            CancelCardUse(dissolve);
            yield break;
        }

        if (playerCardCinematic == null ||
            playerCardCinematic.IsPlaying)
        {
            RefundEnergy(card);
            CancelCardUse(dissolve);
            yield break;
        }

        // =====================================
        // 6. START PLAYER CINEMATIC
        // =====================================

        // Camera chỉ Zoom sau khi Card
        // đã tan biến xong.
        bool cinematicStarted =
            playerCardCinematic.PlayCard(card);

        if (!cinematicStarted)
        {
            Debug.LogWarning(
                "CardManager: Không thể bắt đầu Cinematic!"
            );

            // Hoàn Mana vì Card chưa được sử dụng.
            RefundEnergy(card);

            // Khôi phục Card nếu Cinematic thất bại.
            CancelCardUse(dissolve);

            yield break;
        }

        Debug.Log(
            "CardManager: Bắt đầu Camera Zoom!"
        );

        // =====================================
        // 7. WAIT FOR PLAYER CINEMATIC
        // =====================================

        // Trong PlayerCardCinematic:
        //
        // Damage:
        //   Zoom -> Run -> Attack -> Return
        //
        // Buff:
        //   Zoom -> Buff -> Zoom Out
        //
        // CardEffectManager.ExecuteCardEffect(card)
        // được gọi bên trong Cinematic.
        //
        // KHÔNG gọi lại tại đây.

        while (playerCardCinematic != null &&
               playerCardCinematic.IsPlaying)
        {
            yield return null;
        }

        if (playerCardCinematic == null)
        {
            Debug.LogError(
                "CardManager: PlayerCardCinematic đã bị hủy!"
            );

            CancelCardUse();
            yield break;
        }

        // =====================================
        // 8. RESET SELECTED CARD
        // =====================================

        ResetSelection();

        // =====================================
        // 9. REMOVE CARD FROM HAND
        // =====================================

        if (deckManager != null)
        {
            deckManager.PlayCard(card);
        }

        // Mission 2: ghi nhận loại thẻ sau khi thẻ được sử dụng thành công.
        if (missionRewardManager != null && card != null)
        {
            switch (card.cardType)
            {
                case CardType.Attack:
                    missionRewardManager.RegisterCardUsed(MissionCardType.Attack);
                    break;

                case CardType.Defense:
                    missionRewardManager.RegisterCardUsed(MissionCardType.Guard);
                    break;

                case CardType.Support:
                    missionRewardManager.RegisterCardUsed(MissionCardType.Support);
                    break;
            }
        }

        // =====================================
        // 10. FINISH CARD SEQUENCE
        // =====================================

        isUsingCard = false;

        Debug.Log(
            "CardManager: Hoàn tất sử dụng " +
            card.cardName
        );
    }

    // =========================================
    // VALIDATE CARD
    // =========================================

    private bool IsCardValid(
        CardData card,
        CardUI cardUI)
    {
        if (card == null ||
            cardUI == null ||
            cardUI.cardData != card)
        {
            Debug.LogWarning(
                "CardManager: Card không hợp lệ!"
            );

            return false;
        }

        if (battleStateMachine == null ||
            !battleStateMachine.IsPlayerTurn() ||
            battleStateMachine.IsBattleFinished())
        {
            Debug.LogWarning(
                "CardManager: Player Turn không hợp lệ!"
            );

            return false;
        }

        return true;
    }

    // =========================================
    // REFUND ENERGY
    // =========================================

    private void RefundEnergy(CardData card)
    {
        if (turnManager == null || card == null)
            return;

        turnManager.RecoverEnergy(card.cost);

        Debug.Log(
            $"CardManager: Hoàn lại {card.cost} Mana."
        );
    }

    // =========================================
    // CANCEL CARD USE
    // =========================================

    private void CancelCardUse(
        CardDissolveUI dissolve = null)
    {
        if (dissolve != null)
        {
            dissolve.ResetDissolve();
        }

        ResetSelection();

        isUsingCard = false;

        Debug.Log(
            "CardManager: Đã hủy sử dụng Card!"
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
