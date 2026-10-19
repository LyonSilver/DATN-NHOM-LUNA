using UnityEngine;

public class Reset : MonoBehaviour
{
    [Header("Deck")]
    [SerializeField] private DeckManager deckManager;

    public void ResetDeck()
    {
        if (deckManager == null)
        {
            Debug.LogWarning("Reset: Chưa gán DeckManager.");
            return;
        }

        // Reset toàn bộ Deck
        deckManager.InitializeDeck();

        Debug.Log("Reset: Đã reset toàn bộ Card và random lại Deck.");
    }
}