
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

    [Header("Rage Manager")]
    [SerializeField] private RageManager rageManager;

    [Header("NPC Ultimate")]
    [SerializeField]
    private NalaUltimateController nalaUltimateController;

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

        if (nalaUltimateController == null)
        {
            nalaUltimateController =
                FindFirstObjectByType<NalaUltimateController>();
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
    // CHECK NPC ULTIMATE
    // =====================================

    private bool IsNPCUsingUltimate()
    {
        return nalaUltimateController != null &&
               nalaUltimateController.IsUsingUltimate;
    }

    // Kiểm tra Player có thể dùng Ultimate không.
    public bool CanStartUltimate()
    {
        return CanUseCard() &&
               HasLivingEnemy() &&
               !playerCardCinematic.IsPlaying;
    }

    // =====================================
    // CARD CLICK
    // =====================================

    public void OnCardClicked(CardInputHandler clickedCard)
    {
        Debug.Log(
            "[CARD MANAGER] OnCardClicked được gọi!",
            this
        );

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
            "[CARD MANAGER] Card được chọn: " +
            card.cardName
        );

        // Kiểm tra Enemy nếu là Attack Card.
        if (IsAttackCard(card) && !HasLivingEnemy())
        {
            Debug.LogWarning(
                "CardManager: Không có Enemy để tấn công!"
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

        // Khóa sử dụng Card ngay lập tức.
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

        if (rageManager == null)
        {
            Debug.LogError(
                "CardManager: Thiếu RageManager!"
            );

            missingReference = true;
        }

        if (missingReference)
            return false;

        if (Time.timeScale <= 0f || isUsingCard)
            return false;

        // Không cho sử dụng Card khi NPC đang Ultimate.
        if (IsNPCUsingUltimate())
        {
            Debug.Log(
                "[CARD MANAGER] NPC đang sử dụng Ultimate!"
            );

            return false;
        }

        if (playerCardCinematic.IsPlaying)
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
            !oneCardTurnController.CanUseCard())
        {
            CancelCardUse(dissolve);
            yield break;
        }

        // =====================================
        // 2. CARD DISSOLVE
        // =====================================

        yield return dissolve.PlayDissolve();

        if (!dissolve.LastPlaySucceeded)
        {
            CancelCardUse(dissolve);
            yield break;
        }

        // =====================================
        // 3. VALIDATE AGAIN
        // =====================================

        if (!IsCardValid(card, cardUI) ||
            !oneCardTurnController.CanUseCard())
        {
            CancelCardUse(dissolve);
            yield break;
        }

        // =====================================
        // 4. START CINEMATIC
        // =====================================

        bool cinematicStarted =
            playerCardCinematic.PlayCard(card);

        if (!cinematicStarted)
        {
            Debug.LogError(
                "[CARD MANAGER] Không thể chạy Cinematic!"
            );

            CancelCardUse(dissolve);
            yield break;
        }

        // Đánh dấu đã sử dụng Card trong lượt.
        oneCardTurnController.NotifyCardStarted();

        // =====================================
        // 5. WAIT FOR CINEMATIC
        // =====================================

        while (playerCardCinematic != null &&
               playerCardCinematic.IsPlaying)
        {
            yield return null;
        }

        // =====================================
        // 6. REMOVE CARD FROM HAND
        // =====================================

        ResetSelection();

        deckManager.PlayCard(card);

        // =====================================
        // 7. ADD RAGE
        // =====================================

        if (rageManager != null)
        {
            rageManager.OnCardUsed();

            Debug.Log(
                "[CARD MANAGER] Rage hiện tại: " +
                rageManager.CurrentRage + "/" +
                rageManager.MaxRage
            );
        }

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

        // OneCardTurnController tự xử lý chuyển lượt.
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

        // Không cho dùng Card trong Ultimate.
        if (IsNPCUsingUltimate())
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
    // CANCEL
    // =====================================

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
