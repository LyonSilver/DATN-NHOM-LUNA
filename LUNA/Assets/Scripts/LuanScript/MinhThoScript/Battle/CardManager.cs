
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

    [Header("Turn Controller")]
    [SerializeField]
    private OneCardTurnController oneCardTurnController;

    [Header("Cinematic")]
    [SerializeField]
    private PlayerCardCinematic playerCardCinematic;

    [Header("Enemy")]
    [SerializeField]
    private EnemySpawner enemySpawner;

    [Header("Card Settings")]
    [SerializeField]
    private float useDelay = 0.1f;

    [Header("Runtime State")]
    public CardUI selectedCard;

    private bool isUsingCard;

    public bool IsUsingCard => isUsingCard;

    // =====================================
    // INITIALIZATION
    // =====================================

    private void Awake()
    {
        if (playerCardCinematic == null)
        {
            playerCardCinematic =
                FindFirstObjectByType<PlayerCardCinematic>();
        }
    }

    private void OnEnable()
    {
        if (deckManager != null)
            deckManager.OnHandChanged += HandleHandChanged;
    }

    private void OnDisable()
    {
        if (deckManager != null)
            deckManager.OnHandChanged -= HandleHandChanged;

        StopAllCoroutines();

        isUsingCard = false;
        selectedCard = null;
    }

    // =====================================
    // CARD CLICK
    // =====================================

    public void OnCardClicked(CardInputHandler clickedCard)
    {
        Debug.Log("[CARD MANAGER] OnCardClicked được gọi!", this);

        if (clickedCard == null || !CanUseCard())
            return;

        CardUI cardUI = clickedCard.GetCardUI();

        if (cardUI == null || cardUI.cardData == null)
        {
            Debug.LogWarning(
                "CardManager: CardData không hợp lệ!"
            );
            return;
        }

        CardData card = cardUI.cardData;

        Debug.Log(
            "[CARD MANAGER] Card được chọn: " + card.cardName
        );

        // Kiểm tra Enemy nếu là Attack Card.
        if (IsAttackCard(card) && !HasLivingEnemy())
        {
            Debug.LogWarning(
                "CardManager: Không có Enemy để tấn công!"
            );
            return;
        }

        // Kiểm tra Mana.
        if (!turnManager.CanAfford(card))
        {
            Debug.Log(
                "CardManager: Không đủ Mana để dùng " +
                card.cardName
            );
            return;
        }

        // Tìm CardDissolveUI.
        CardDissolveUI dissolve =
            cardUI.GetComponent<CardDissolveUI>();

        if (dissolve == null)
        {
            dissolve =
                cardUI.GetComponentInParent<CardDissolveUI>();
        }

        if (dissolve == null)
        {
            Debug.LogError(
                "CardManager: Thiếu CardDissolveUI!"
            );
            return;
        }

        if (dissolve.IsPlaying)
            return;

        // Khóa click Card ngay lập tức.
        isUsingCard = true;

        selectedCard = cardUI;

        handManager.SetSelectedCard(cardUI);

        StartCoroutine(
            UseCard(card, cardUI, dissolve)
        );
    }

    // =====================================
    // CHECK PERMISSION
    // =====================================

    private bool CanUseCard()
    {
        bool missingReference = false;

        if (turnManager == null)
        {
            Debug.LogError(
                "CardManager: Thiếu TurnManager!"
            );
            missingReference = true;
        }

        if (handManager == null)
        {
            Debug.LogError(
                "CardManager: Thiếu HandManager!"
            );
            missingReference = true;
        }

        if (deckManager == null)
        {
            Debug.LogError(
                "CardManager: Thiếu DeckManager!"
            );
            missingReference = true;
        }

        if (cardEffectManager == null)
        {
            Debug.LogError(
                "CardManager: Thiếu CardEffectManager!"
            );
            missingReference = true;
        }

        if (battleStateMachine == null)
        {
            Debug.LogError(
                "CardManager: Thiếu BattleStateMachine!"
            );
            missingReference = true;
        }

        if (oneCardTurnController == null)
        {
            Debug.LogError(
                "CardManager: Thiếu OneCardTurnController!"
            );
            missingReference = true;
        }

        if (playerCardCinematic == null)
        {
            Debug.LogError(
                "CardManager: Thiếu PlayerCardCinematic!"
            );
            missingReference = true;
        }

        if (missingReference)
            return false;

        if (Time.timeScale <= 0f || isUsingCard)
            return false;

        if (!battleStateMachine.IsPlayerTurn() ||
            battleStateMachine.IsBattleFinished())
            return false;

        return oneCardTurnController.CanUseCard();
    }

    // =====================================
    // USE CARD
    // =====================================

    private IEnumerator UseCard(
        CardData card,
        CardUI cardUI,
        CardDissolveUI dissolve)
    {
        Debug.Log(
            "[CARD MANAGER] Bắt đầu sử dụng: " +
            card.cardName,
            this
        );

        // =====================================
        // 1. PREPARE
        // =====================================

        if (useDelay > 0f)
            yield return new WaitForSeconds(useDelay);

        if (!IsCardValid(card, cardUI) ||
            !oneCardTurnController.CanUseCard() ||
            !turnManager.CanAfford(card))
        {
            CancelCardUse(dissolve);
            yield break;
        }

        // =====================================
        // 2. SPEND ENERGY
        // =====================================

        if (!turnManager.SpendEnergy(card.cost))
        {
            CancelCardUse(dissolve);
            yield break;
        }

        // =====================================
        // 3. CARD DISSOLVE
        // =====================================

        Debug.Log("[CARD MANAGER] CARD DISSOLVE: BẮT ĐẦU");

        yield return dissolve.PlayDissolve();

        Debug.Log(
            "[CARD MANAGER] CARD DISSOLVE: KẾT THÚC | Success = " +
            dissolve.LastPlaySucceeded
        );

        if (!dissolve.LastPlaySucceeded)
        {
            RefundEnergy(card);
            CancelCardUse(dissolve);
            yield break;
        }

        // =====================================
        // 4. VALIDATE AGAIN
        // =====================================

        if (!IsCardValid(card, cardUI) ||
            !oneCardTurnController.CanUseCard())
        {
            RefundEnergy(card);
            CancelCardUse(dissolve);
            yield break;
        }

        // =====================================
        // 5. START CINEMATIC
        // =====================================

        Debug.Log(
            "[CARD MANAGER] Đang gọi PlayerCardCinematic.PlayCard()"
        );

        bool cinematicStarted =
            playerCardCinematic.PlayCard(card);

        if (!cinematicStarted)
        {
            Debug.LogError(
                "[CARD MANAGER] Không thể khởi chạy Cinematic!"
            );

            RefundEnergy(card);
            CancelCardUse(dissolve);
            yield break;
        }

        // Chỉ đánh dấu đã sử dụng Card
        // sau khi Cinematic chấp nhận chạy thành công.
        oneCardTurnController.NotifyCardStarted();

        Debug.Log(
            "[CARD MANAGER] Cinematic đã bắt đầu!"
        );

        // =====================================
        // 6. WAIT FOR CINEMATIC
        // =====================================

        while (playerCardCinematic != null &&
               playerCardCinematic.IsPlaying)
        {
            yield return null;
        }

        Debug.Log(
            "[CARD MANAGER] Cinematic hoàn tất!"
        );

        // =====================================
        // 7. REMOVE CARD FROM HAND
        // =====================================

        ResetSelection();

        deckManager.PlayCard(card);

        // =====================================
        // 8. FLUSH PENDING DRAW
        // =====================================

        cardEffectManager.FlushPendingCardDraws();

        // =====================================
        // 9. FINISH
        // =====================================

        isUsingCard = false;

        Debug.Log(
            "[CARD MANAGER] Hoàn thành Card: " +
            card.cardName +
            ". Chuyển sang Enemy Turn."
        );

        // Không gọi EndTurn tại đây.
        // OneCardTurnController tự xử lý.
    }

    // =====================================
    // CARD CLASSIFICATION
    // =====================================

    private bool IsAttackCard(CardData card)
    {
        if (card == null)
            return false;

        int type = (int)card.effectType;

        // 0 = Damage
        // 3 = Poison
        // 5 = MultiHit

        return type == 0 ||
               type == 3 ||
               type == 5;
    }

    // =====================================
    // ENEMY VALIDATION
    // =====================================

    private bool HasLivingEnemy()
    {
        if (enemySpawner == null)
            return false;

        EnemyHealth enemy =
            enemySpawner.GetCurrentEnemyHealth();

        return enemy != null && !enemy.IsDead;
    }

    // =====================================
    // CARD VALIDATION
    // =====================================

    private bool IsCardValid(
        CardData card,
        CardUI cardUI)
    {
        if (card == null ||
            cardUI == null ||
            cardUI.cardData != card)
            return false;

        if (battleStateMachine == null ||
            !battleStateMachine.IsPlayerTurn() ||
            battleStateMachine.IsBattleFinished())
            return false;

        if (IsAttackCard(card) && !HasLivingEnemy())
            return false;

        return true;
    }

    // =====================================
    // REFUND / CANCEL
    // =====================================

    private void RefundEnergy(CardData card)
    {
        if (turnManager == null || card == null)
            return;

        turnManager.RecoverEnergy(card.cost);
    }

    private void CancelCardUse(
        CardDissolveUI dissolve = null)
    {
        if (dissolve != null)
            dissolve.ResetDissolve();

        ResetSelection();

        isUsingCard = false;

        Debug.Log(
            "[CARD MANAGER] Đã hủy sử dụng Card!"
        );
    }

    // =====================================
    // SELECTION
    // =====================================

    private void ResetSelection()
    {
        selectedCard = null;

        if (handManager != null)
            handManager.SetSelectedCard(null);
    }

    public void Deselect()
    {
        if (!isUsingCard)
            ResetSelection();
    }

    private void HandleHandChanged()
    {
        if (!isUsingCard)
            selectedCard = null;
    }
}