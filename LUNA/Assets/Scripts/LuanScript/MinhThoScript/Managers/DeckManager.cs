using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("Master Card Pool - Kéo các CardData asset vào đây")]
    public List<CardData> masterDeck = new List<CardData>();

    [Header("Cấu hình Hand")]
    public int handLimit = 4;

    [Header("Runtime State (Chỉ để xem, không tự sửa trong Inspector)")]
    public List<CardData> drawPile = new List<CardData>();
    public List<CardData> hand = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();

    public event System.Action OnHandChanged;

    public void InitializeDeck()
    {
        drawPile.Clear();
        hand.Clear();
        discardPile.Clear();

        // Kiểm tra dữ liệu Card trước khi thêm vào Deck
        for (int i = 0; i < masterDeck.Count; i++)
        {
            CardData card = masterDeck[i];

            if (card == null)
            {
                Debug.LogError(
                    $"DeckManager: MasterDeck[{i}] đang NULL! " +
                    "Kiểm tra CardData asset trong Inspector.",
                    this
                );

                continue;
            }

            drawPile.Add(card);
        }

        ShuffleDeck();

        // Rút bài khởi đầu, chưa gọi RefreshHand
        int drawCount = Mathf.Min(
            Mathf.Max(0, handLimit),
            drawPile.Count
        );

        for (int i = 0; i < drawCount; i++)
        {
            CardData card = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(card);
        }

        // Chỉ cập nhật Hand một lần sau khi khởi tạo
        OnHandChanged?.Invoke();

        Debug.Log(
            $"DeckManager: Khởi tạo thành công. " +
            $"Hand: {hand.Count}/{handLimit} | " +
            $"DrawPile: {drawPile.Count}"
        );
    }

    public void ShuffleDeck()
    {
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            CardData temp = drawPile[i];
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }
    }

    public void DrawCard()
    {
        if (hand.Count >= handLimit)
        {
            Debug.Log("DeckManager: Hand đã đầy (" + handLimit + " Card), không thể rút thêm.");
            return;
        }

        if (drawPile.Count == 0)
        {
            ReshuffleDiscardIntoDeck();
        }

        if (drawPile.Count == 0)
        {
            Debug.Log("DeckManager: Không còn Card nào để rút (Draw Pile và Discard Pile đều rỗng).");
            return;
        }

        CardData drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        hand.Add(drawnCard);

        OnHandChanged?.Invoke();
    }

    public void PlayCard(CardData card)
    {
        if (!hand.Contains(card))
        {
            Debug.LogWarning("DeckManager.PlayCard: Card này không nằm trong Hand.");
            return;
        }

        hand.Remove(card);
        OnHandChanged?.Invoke();
    }

    public void DiscardCard(CardData card)
    {
        discardPile.Add(card);
    }

    public void ReshuffleDiscardIntoDeck()
    {
        if (discardPile.Count == 0)
        {
            return;
        }

        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck();

        Debug.Log("DeckManager: Đã xáo Discard Pile vào lại Draw Pile.");
    }

    [ContextMenu("TEST - Initialize Deck")]
    public void TestInitializeDeck()
    {
        InitializeDeck();
        LogState();
    }

    [ContextMenu("TEST - Draw 1 Card")]
    public void TestDrawCard()
    {
        DrawCard();
        LogState();
    }

    [ContextMenu("TEST - Play & Discard First Card In Hand")]
    public void TestPlayAndDiscardFirstCard()
    {
        if (hand.Count == 0)
        {
            Debug.Log("DeckManager TEST: Hand đang rỗng, không có Card để chơi.");
            return;
        }

        CardData card = hand[0];
        PlayCard(card);
        DiscardCard(card);
        LogState();
    }

    [ContextMenu("TEST - Force Empty Draw Pile")]
    public void TestForceEmptyDrawPile()
    {
        drawPile.Clear();
        Debug.Log("DeckManager TEST: Đã làm rỗng Draw Pile để test Reshuffle.");
    }

    private void LogState()
    {
        string handNames = "";
        foreach (CardData card in hand)
        {
            handNames += card.cardName + ", ";
        }

        Debug.Log("Hand (" + hand.Count + "/" + handLimit + "): " + handNames +
                   " | Draw Pile: " + drawPile.Count +
                   " | Discard Pile: " + discardPile.Count);
    }
}