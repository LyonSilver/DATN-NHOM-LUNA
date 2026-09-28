using UnityEngine;
using TMPro;

public class UseCardButton : MonoBehaviour
{
    [Header("Enemy HP")]
    [SerializeField] private int enemyHP = 50;
    [SerializeField] private int maxEnemyHP = 50;

    [Header("Player Shield")]
    [SerializeField] private int playerShield = 0;

    [Header("UI")]
    [SerializeField] private TMP_Text enemyHPText;
    [SerializeField] private TMP_Text shieldText;

    [Header("Card Manager")]
    [SerializeField] private CardManager cardManager;

    [Header("Turn Manager")]
    [SerializeField] private TurnManager turnManager;


    public void UseCard()
    {
        if (cardManager == null || cardManager.selectedCard == null)
        {
            Debug.Log("UseCard: Chưa chọn Card.");
            return;
        }

        CardUI usedCard = cardManager.selectedCard;

        if (usedCard.cardData == null)
        {
            Debug.LogWarning("UseCard: CardData đang null.");
            return;
        }

        CardData card = usedCard.cardData;

        if (turnManager == null)
        {
            Debug.LogWarning("UseCard: Chưa gán TurnManager.");
            return;
        }

        // Kiểm tra và trừ Energy
        if (!turnManager.SpendEnergy(card.cost))
        {
            Debug.Log("UseCard: Không đủ Energy.");
            return;
        }

        // =========================
        // CARD EFFECT
        // =========================

        switch (card.effectType)
        {
            case CardEffectType.Damage:

                enemyHP -= card.baseDamage;
                enemyHP = Mathf.Max(enemyHP, 0);

                Debug.Log(
                    "Damage: Enemy mất " +
                    card.baseDamage +
                    " HP."
                );

                break;


            case CardEffectType.Shield:

                playerShield += card.shieldValue;

                Debug.Log(
                    "Shield: Player nhận " +
                    card.shieldValue +
                    " Shield."
                );

                break;


            case CardEffectType.Heal:

                Debug.Log(
                    "Heal: Player hồi " +
                    card.healValue +
                    " HP."
                );

                // Phần Heal Player HP
                // xử lý ở Player HP script riêng.

                break;


            case CardEffectType.Poison:

                Debug.Log(
                    "Poison: Áp Poison trong " +
                    card.statusDuration +
                    " lượt."
                );

                break;


            case CardEffectType.FocusTiming:

                Debug.Log(
                    "FocusTiming: +" +
                    card.extraValue +
                    "% Timing."
                );

                break;


            case CardEffectType.DoubleHit:

                Debug.Log(
                    "DoubleHit: Đánh " +
                    card.extraValue +
                    " lần."
                );

                break;


            case CardEffectType.ChaosDraw:

                Debug.Log(
                    "ChaosDraw: Rút thêm " +
                    card.extraValue +
                    " Card."
                );

                break;
        }

        // Chỉ cập nhật UI mà UseCardButton thực sự quản lý
        UpdateUI();

        // Bỏ chọn Card
        cardManager.Deselect();

        // Xóa Card đã dùng
        Destroy(usedCard.gameObject);
    }


    private void UpdateUI()
    {
        if (enemyHPText != null)
        {
            enemyHPText.text =
                "HP: " + enemyHP + "/" + maxEnemyHP;
        }

        if (shieldText != null)
        {
            shieldText.text =
                "Shield: " + playerShield;
        }
    }
}