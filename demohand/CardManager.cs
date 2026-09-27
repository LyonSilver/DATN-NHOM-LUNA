using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    // Danh sách các lá bài
    public List<CardData> deckList = new List<CardData>();
    public List<CardData> handList = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();

    // Giới hạn số bài trên tay
    public int maxHandSize = 4;

    // Năng lượng hiện tại
    public int CurrentEnergy = 6;

    // =========================
    // RÚT BÀI
    // =========================
    public void DrawCard()
    {
        // Kiểm tra tay đã đủ 4 lá chưa
        if (handList.Count >= maxHandSize)
        {
            Debug.Log("Không thể rút thêm! Hand đã đủ 4 lá.");
            return;
        }

        // Nếu deck hết bài
        if (deckList.Count == 0)
        {
            // Nếu discard không có bài thì không thể rút
            if (discardPile.Count == 0)
            {
                Debug.Log("Không còn bài để rút!");
                return;
            }

            // Chuyển discard sang deck
            deckList.AddRange(discardPile);
            discardPile.Clear();

            // Xáo bài
            ShuffleDeck();

            Debug.Log("Deck đã hết! Xáo discardPile thành deck mới.");
        }

        // Chọn một vị trí ngẫu nhiên trong deck
        int randomIndex = Random.Range(0, deckList.Count);

        // Lấy lá bài
        CardData drawnCard = deckList[randomIndex];

        // Xóa khỏi deck
        deckList.RemoveAt(randomIndex);

        // Thêm vào hand
        handList.Add(drawnCard);

        Debug.Log("Rút bài: " + drawnCard.cardName);

        PrintCardStatus();
    }

    // =========================
    // XÁO BÀI
    // =========================
    private void ShuffleDeck()
    {
        for (int i = 0; i < deckList.Count; i++)
        {
            int randomIndex = Random.Range(0, deckList.Count);

            CardData temp = deckList[i];
            deckList[i] = deckList[randomIndex];
            deckList[randomIndex] = temp;
        }
    }

    // =========================
    // DÙNG BÀI
    // =========================
    public void PlayCard(CardData card)
    {
        // Kiểm tra card có trong hand không
        if (!handList.Contains(card))
        {
            Debug.Log("Không thể dùng bài! Lá bài không có trong Hand.");
            return;
        }

        // Kiểm tra Energy
        if (CurrentEnergy < card.cost)
        {
            Debug.Log(
                "Không đủ Energy để dùng " +
                card.cardName +
                ". Cần: " +
                card.cost +
                " | Hiện tại: " +
                CurrentEnergy
            );

            return;
        }

        // Trừ Energy
        CurrentEnergy -= card.cost;

        // Thực hiện hiệu ứng
        ApplyCardEffect(card);

        // Xóa bài khỏi hand
        handList.Remove(card);

        // Đưa vào discard
        discardPile.Add(card);

        Debug.Log(
            "Đã dùng bài: " +
            card.cardName +
            " | Energy còn lại: " +
            CurrentEnergy
        );

        PrintCardStatus();
    }

    // =========================
    // HIỆU ỨNG BÀI
    // =========================
    private void ApplyCardEffect(CardData card)
    {
        switch (card.cardType)
        {
            case CardType.Attack:

                Debug.Log(
                    card.cardName +
                    " gây " +
                    card.baseValue +
                    " Damage."
                );

                break;

            case CardType.Guard:

                Debug.Log(
                    card.cardName +
                    " tạo " +
                    card.baseValue +
                    " Shield."
                );

                break;

            case CardType.Effect:

                Debug.Log(
                    card.cardName +
                    " kích hoạt Effect với giá trị " +
                    card.baseValue
                );

                break;

            case CardType.Support:

                Debug.Log(
                    card.cardName +
                    " kích hoạt Support với giá trị " +
                    card.baseValue
                );

                break;

            case CardType.Chaos:

                Debug.Log(
                    card.cardName +
                    " kích hoạt Chaos Effect."
                );

                break;
        }
    }

    // =========================
    // HIỂN THỊ TRẠNG THÁI
    // =========================
    public void PrintCardStatus()
    {
        Debug.Log(
            "===== CARD STATUS ====="
        );

        Debug.Log(
            "Deck: " +
            deckList.Count +
            " | Hand: " +
            handList.Count +
            " | Discard: " +
            discardPile.Count +
            " | Energy: " +
            CurrentEnergy
        );

        // In các lá trong Hand
        Debug.Log("----- HAND -----");

        for (int i = 0; i < handList.Count; i++)
        {
            Debug.Log(
                i +
                ": " +
                handList[i].cardName +
                " | Cost: " +
                handList[i].cost
            );
        }

        Debug.Log(
            "======================"
        );
    }
}