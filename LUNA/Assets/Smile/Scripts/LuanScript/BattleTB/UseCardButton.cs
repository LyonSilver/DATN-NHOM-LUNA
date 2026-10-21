using UnityEngine;

public class UseCardButton : MonoBehaviour
{
    [Header("Card Manager")]
    [SerializeField] private CardManager cardManager;

    [Header("Card Effect Manager")]
    [SerializeField] private CardEffectManager cardEffectManager;

    [Header("Turn Manager")]
    [SerializeField] private TurnManager turnManager;


    public void UseCard()
    {
        // =====================================================
        // KIỂM TRA CARD
        // =====================================================

        if (cardManager == null)
        {
            Debug.LogWarning(
                "UseCard: Chưa gán CardManager."
            );

            return;
        }


        if (cardManager.selectedCard == null)
        {
            Debug.Log(
                "UseCard: Chưa chọn Card."
            );

            return;
        }


        CardUI usedCard = cardManager.selectedCard;


        if (usedCard.cardData == null)
        {
            Debug.LogWarning(
                "UseCard: CardData đang null."
            );

            return;
        }


        CardData card = usedCard.cardData;


        // =====================================================
        // KIỂM TRA TURN MANAGER
        // =====================================================

        if (turnManager == null)
        {
            Debug.LogWarning(
                "UseCard: Chưa gán TurnManager."
            );

            return;
        }


        // =====================================================
        // KIỂM TRA CARD EFFECT MANAGER
        // =====================================================

        if (cardEffectManager == null)
        {
            Debug.LogWarning(
                "UseCard: Chưa gán CardEffectManager."
            );

            return;
        }


        // =====================================================
        // TRỪ ENERGY
        // =====================================================

        if (!turnManager.SpendEnergy(card.cost))
        {
            Debug.Log(
                "UseCard: Không đủ Energy."
            );

            return;
        }


        // =====================================================
        // THỰC HIỆN EFFECT
        // =====================================================

        cardEffectManager.ExecuteCardEffect(card);


        // =====================================================
        // BỎ CHỌN CARD
        // =====================================================

        cardManager.Deselect();


        // =====================================================
        // XÓA CARD
        // =====================================================

        Destroy(usedCard.gameObject);
    }
}