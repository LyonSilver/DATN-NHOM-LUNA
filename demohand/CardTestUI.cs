using UnityEngine;

public class CardTestUI : MonoBehaviour
{
    public CardManager cardManager;

    // Nút Draw Card
    public void TestDrawCard()
    {
        cardManager.DrawCard();
    }

    // Nút Play Card
    public void TestPlayCard()
    {
        if (cardManager.handList.Count == 0)
        {
            Debug.Log("Hand đang trống, không có bài để dùng.");
            return;
        }

        // Dùng lá bài đầu tiên trong Hand
        CardData cardToPlay = cardManager.handList[0];

        cardManager.PlayCard(cardToPlay);
    }
}