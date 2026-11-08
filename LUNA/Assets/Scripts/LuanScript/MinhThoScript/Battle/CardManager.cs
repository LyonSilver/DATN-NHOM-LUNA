
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

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator playerAnimator;
    [SerializeField]
    private PlayerAnimationController playerAnimation;

    [Header("Enemy")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Card Settings")]
    [SerializeField] private float useDelay = 0.1f;

    [Header("Attack Movement")]
    [SerializeField] private bool dashToEnemy = true;
    [SerializeField] private float dashDuration = 0.4f;
    [SerializeField] private float returnDuration = 0.4f;
    [SerializeField] private float stoppingDistance = 1.5f;

    [Header("Animation Parameters")]
    [SerializeField] private string runningParameter = "IsRunning";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string healTrigger = "";
    [SerializeField] private string shieldTrigger = "";
    [SerializeField] private string buffTrigger = "";

    [Header("Action Duration")]
    [SerializeField] private float attackDuration = 0.8f;
    [SerializeField] private float buffDuration = 0.7f;

    [Header("Runtime State")]
    public CardUI selectedCard;

    private bool isUsingCard;
    private bool isMovingPlayer;
    private Vector3 playerStartPosition;

    public bool IsUsingCard => isUsingCard;

    // =====================================
    // INITIALIZATION
    // =====================================

    private void Awake()
    {
        if (player == null)
            return;

        if (playerAnimator == null)
        {
            playerAnimator =
                player.GetComponentInChildren<Animator>(true);
        }

        if (playerAnimation == null)
        {
            playerAnimation =
                player.GetComponentInChildren<
                    PlayerAnimationController>(true);
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

        SetRunning(false);

        if (isMovingPlayer && player != null)
            player.position = playerStartPosition;

        isMovingPlayer = false;
        isUsingCard = false;
        selectedCard = null;
    }

    // =====================================
    // CARD CLICK
    // =====================================

    public void OnCardClicked(CardInputHandler clickedCard)
    {
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

        // Card tấn công phải có Enemy còn sống.
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

        // Khóa click thêm Card ngay lập tức.
        isUsingCard = true;

        selectedCard = cardUI;
        handManager.SetSelectedCard(cardUI);

        StartCoroutine(UseCard(card, cardUI, dissolve));
    }

    // =====================================
    // CHECK PERMISSION
    // =====================================

    private bool CanUseCard()
    {
        if (turnManager == null ||
            handManager == null ||
            deckManager == null ||
            cardEffectManager == null ||
            battleStateMachine == null ||
            oneCardTurnController == null)
        {
            Debug.LogError(
                "CardManager: Thiếu Manager Reference!"
            );
            return false;
        }

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
            "CardManager: Sử dụng " + card.cardName
        );

        // 1. Chuẩn bị.
        if (useDelay > 0f)
            yield return new WaitForSeconds(useDelay);

        if (!IsCardValid(card, cardUI) ||
            !oneCardTurnController.CanUseCard() ||
            !turnManager.CanAfford(card))
        {
            CancelCardUse(dissolve);
            yield break;
        }

        // 2. Trừ Mana.
        if (!turnManager.SpendEnergy(card.cost))
        {
            CancelCardUse(dissolve);
            yield break;
        }

        // 3. Card Dissolve.
        Debug.Log("CARD DISSOLVE: BẮT ĐẦU");

        yield return dissolve.PlayDissolve();

        Debug.Log(
            "CARD DISSOLVE: KẾT THÚC | Success = " +
            dissolve.LastPlaySucceeded
        );

        if (!dissolve.LastPlaySucceeded)
        {
            RefundEnergy(card);
            CancelCardUse(dissolve);
            yield break;
        }

        // 4. Kiểm tra lại sau Dissolve.
        if (!IsCardValid(card, cardUI) ||
            !oneCardTurnController.CanUseCard())
        {
            RefundEnergy(card);
            CancelCardUse(dissolve);
            yield break;
        }

        // 5. Chính thức sử dụng Card.
        // Từ thời điểm này, không dùng được Card thứ hai.
        oneCardTurnController.NotifyCardStarted();

        // 6. Animation và hiệu ứng.
        // KHÔNG di chuyển Camera, KHÔNG Zoom.
        if (IsAttackCard(card))
        {
            yield return ExecuteAttackCard(card);
        }
        else
        {
            yield return ExecuteBuffCard(card);
        }

        // 7. Xóa Card đã dùng khỏi Hand.
        ResetSelection();

        deckManager.PlayCard(card);

        // Xử lý các yêu cầu rút bài còn chờ,
        // ví dụ Chaos Draw.
        cardEffectManager.FlushPendingCardDraws();

        // 8. Kết thúc.
        isUsingCard = false;

        Debug.Log(
            "CardManager: Hoàn thành " +
            card.cardName +
            ". Chuyển sang Enemy Turn."
        );

        // KHÔNG gọi EndTurn tại đây.
        // OneCardTurnController sẽ tự xử lý.
    }

    // =====================================
    // ATTACK CARD
    // =====================================

    private IEnumerator ExecuteAttackCard(CardData card)
    {
        EnemyHealth enemy =
            enemySpawner != null
                ? enemySpawner.GetCurrentEnemyHealth()
                : null;

        if (enemy == null || enemy.IsDead)
            yield break;

        Vector3 originalPosition = Vector3.zero;
        bool moved = false;

        if (player != null)
        {
            originalPosition = player.position;
            playerStartPosition = originalPosition;
        }

        // 1. Player chạy tới Enemy nếu bật Dash.
        if (dashToEnemy && player != null)
        {
            Vector3 targetPosition = originalPosition;

            float distanceX =
                enemy.transform.position.x -
                originalPosition.x;

            if (Mathf.Abs(distanceX) > stoppingDistance)
            {
                float direction = Mathf.Sign(distanceX);

                targetPosition.x =
                    enemy.transform.position.x -
                    direction * stoppingDistance;
            }

            targetPosition.y = originalPosition.y;
            targetPosition.z = originalPosition.z;

            if (Vector3.Distance(
                    player.position,
                    targetPosition) > 0.01f)
            {
                moved = true;
                isMovingPlayer = true;

                SetRunning(true);

                yield return MovePlayer(
                    targetPosition,
                    dashDuration
                );

                SetRunning(false);
            }
        }

        // 2. Attack Animation.
        PlayAttackAnimation();

        // 3. Damage + VFX.
        // Chỉ thực hiện Card một lần.
        cardEffectManager.ExecuteCardEffect(card);

        // 4. Chờ Animation.
        yield return new WaitForSeconds(
            Mathf.Max(0f, attackDuration)
        );

        // 5. Player trở về vị trí ban đầu.
        if (moved && player != null)
        {
            SetRunning(true);

            yield return MovePlayer(
                originalPosition,
                returnDuration
            );

            SetRunning(false);
        }

        isMovingPlayer = false;
    }

    // =====================================
    // BUFF CARD
    // =====================================

    private IEnumerator ExecuteBuffCard(CardData card)
    {
        int type = (int)card.effectType;

        switch (type)
        {
            case 1: // Shield / Guard
                SetTriggerIfExists(shieldTrigger);
                break;

            case 2: // Heal
                SetTriggerIfExists(healTrigger);
                break;

            default: // Focus / Chaos Draw
                SetTriggerIfExists(buffTrigger);
                break;
        }

        // Buff không Dash và không Zoom.
        cardEffectManager.ExecuteCardEffect(card);

        yield return new WaitForSeconds(
            Mathf.Max(0f, buffDuration)
        );
    }

    // =====================================
    // MOVEMENT
    // =====================================

    private IEnumerator MovePlayer(
        Vector3 target,
        float duration)
    {
        if (player == null)
            yield break;

        Vector3 start = player.position;

        if (duration <= 0f)
        {
            player.position = target;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / duration)
            );

            player.position = Vector3.Lerp(
                start,
                target,
                t
            );

            yield return null;
        }

        player.position = target;
    }

    // =====================================
    // ANIMATION
    // =====================================

    private void PlayAttackAnimation()
    {
        if (playerAnimation != null)
        {
            playerAnimation.PlayAttack();
            return;
        }

        SetTriggerIfExists(attackTrigger);
    }

    private void SetRunning(bool running)
    {
        if (HasParameter(
                runningParameter,
                AnimatorControllerParameterType.Bool))
        {
            playerAnimator.SetBool(runningParameter, running);
        }
    }

    private void SetTriggerIfExists(string triggerName)
    {
        if (HasParameter(
                triggerName,
                AnimatorControllerParameterType.Trigger))
        {
            playerAnimator.SetTrigger(triggerName);
        }
    }

    private bool HasParameter(
        string name,
        AnimatorControllerParameterType type)
    {
        if (playerAnimator == null ||
            string.IsNullOrEmpty(name))
            return false;

        foreach (AnimatorControllerParameter parameter
                 in playerAnimator.parameters)
        {
            if (parameter.name == name &&
                parameter.type == type)
                return true;
        }

        return false;
    }

    // =====================================
    // CARD VALIDATION
    // =====================================

    private bool IsAttackCard(CardData card)
    {
        if (card == null)
            return false;

        int type = (int)card.effectType;

        // Damage, Poison, MultiHit.
        return type == 0 || type == 3 || type == 5;
    }

    private bool HasLivingEnemy()
    {
        if (enemySpawner == null)
            return false;

        EnemyHealth enemy =
            enemySpawner.GetCurrentEnemyHealth();

        return enemy != null && !enemy.IsDead;
    }

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
            "CardManager: Đã hủy sử dụng Card!"
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
        if (!isUsingCard)
            ResetSelection();
    }

    private void HandleHandChanged()
    {
        if (!isUsingCard)
            selectedCard = null;
    }
}