using UnityEngine;
using TMPro;

public class EndButton : MonoBehaviour
{
    [Header("Player HP")]
    [SerializeField] private int playerHP = 100;
    [SerializeField] private int maxPlayerHP = 100;

    [Header("HP Text")]
    [SerializeField] private TMP_Text playerHPText;

    [Header("Card")]
    [SerializeField] private DeckManager deckManager;

    [Header("Turn")]
    [SerializeField] private TurnManager turnManager;


    public void EndTurn()
    {
        // Player mất 10 HP
        playerHP -= 10;
        playerHP = Mathf.Max(playerHP, 0);

        if (playerHPText != null)
        {
            playerHPText.text = "HP: " + playerHP + "/" + maxPlayerHP;
        }

        // Random lại toàn bộ Card
        if (deckManager != null)
        {
            deckManager.InitializeDeck();
        }

        // Reset Energy
        if (turnManager != null)
        {
            turnManager.StartTurn();
        }

        Debug.Log("End: Đã kết thúc lượt và tạo Hand mới.");
    }


    private void RefreshCards()
    {
        // Đưa toàn bộ card đang có trong Hand
        // trở lại Draw Pile
        deckManager.drawPile.AddRange(deckManager.hand);

        // Xóa Hand hiện tại
        deckManager.hand.Clear();

        // Xáo lại toàn bộ Draw Pile
        deckManager.ShuffleDeck();

        // Rút lại số card bằng Hand Limit
        for (int i = 0; i < deckManager.handLimit; i++)
        {
            deckManager.DrawCard();
        }

        Debug.Log(
            "End: Đã random lại Card. Hand hiện tại: " +
            deckManager.hand.Count
        );
    }
}