using System.Collections;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Tham chieu")]
    public TurnManager turnManager;
    public HandManager handManager;
    public DeckManager deckManager;
    public CardEffectManager cardEffectManager;

    [Header("Card Use")]
    [SerializeField] private float useDelay = 0.5f;

    [Header("Runtime State (Chỉ để xem, không chỉnh sửa)")]
    public CardUI selectedCard;

    private bool isUsingCard = false;

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

    // =====================================================
    // CLICK CARD
    // =====================================================

    public void OnCardClicked(CardInputHandler clickedCard)
    {
        if (isUsingCard)
        {
            Debug.Log("CardManager: Đang sử dụng Card.");
            return;
        }

        if (clickedCard == null)
            return;

        CardUI clickedUI = clickedCard.GetCardUI();

        if (clickedUI == null || clickedUI.cardData == null)
            return;

        CardData card = clickedUI.cardData;

        // ---------------------------------------------
        // KIỂM TRA ENERGY
        // ---------------------------------------------

        if (!turnManager.CanAfford(card))
        {
            Debug.Log(
                "CardManager: Không đủ Energy để sử dụng Card '" +
                card.cardName +
                "'."
            );

            return;
        }

        // ---------------------------------------------
        // KIỂM TRA CARD EFFECT MANAGER
        // ---------------------------------------------

        if (cardEffectManager == null)
        {
            Debug.LogError(
                "CardManager: Chưa gán CardEffectManager!"
            );

            return;
        }

        // ---------------------------------------------
        // CHỌN CARD
        // ---------------------------------------------

        selectedCard = clickedUI;

        handManager.SetSelectedCard(clickedUI);

        Debug.Log(
            "CardManager: Đã chọn Card '" +
            card.cardName +
            "'."
        );

        // ---------------------------------------------
        // BẮT ĐẦU SỬ DỤNG
        // ---------------------------------------------

        StartCoroutine(UseCard(card));
    }

    // =====================================================
    // USE CARD
    // =====================================================

    private IEnumerator UseCard(CardData card)
    {
        isUsingCard = true;

        Debug.Log(
            "CardManager: Đang chờ " +
            useDelay +
            " giây trước khi sử dụng Card."
        );

        // =================================================
        // KHOẢNG CHỜ
        // Sau này đặt animation ở đây
        // =================================================

        yield return new WaitForSeconds(useDelay);

        // =================================================
        // TRỪ ENERGY
        // =================================================

        turnManager.SpendEnergy(card.cost);

        Debug.Log(
            "CardManager: Đã trừ " +
            card.cost +
            " Energy."
        );

        // =================================================
        // THỰC HIỆN EFFECT
        // =================================================

        Debug.Log(
            "CardManager: Đang ExecuteCardEffect cho Card '" +
            card.cardName +
            "'."
        );

        cardEffectManager.ExecuteCardEffect(card);

        // =================================================
        // XÓA CARD KHỎI HAND
        // =================================================

        if (deckManager != null)
        {
            deckManager.PlayCard(card);
        }

        // =================================================
        // RESET
        // =================================================

        selectedCard = null;

        if (handManager != null)
        {
            handManager.SetSelectedCard(null);
        }

        isUsingCard = false;

        Debug.Log(
            "CardManager: Đã sử dụng xong Card '" +
            card.cardName +
            "'."
        );
    }

    // =====================================================
    // DESELECT
    // =====================================================

    public void Deselect()
    {
        if (isUsingCard)
            return;

        selectedCard = null;

        if (handManager != null)
        {
            handManager.SetSelectedCard(null);
        }
    }

    // =====================================================
    // HAND CHANGED
    // =====================================================

    private void HandleHandChanged()
    {
        if (!isUsingCard)
        {
            selectedCard = null;
        }
    }
}