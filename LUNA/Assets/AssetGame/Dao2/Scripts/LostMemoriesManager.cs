
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LostMemoriesManager : MonoBehaviour
{
    [System.Serializable]
    private class CardSetup
    {
        public MemoryCardKind Kind;
        public int PairId;
        public Sprite FaceSprite;
    }

    [Header("CARD SETUP")]
    [SerializeField] private MemoryCardView[] cardSlots;

    [SerializeField] private Sprite[] normalCardSprites;
    [SerializeField] private Sprite loseTurnTrapSprite;
    [SerializeField] private Sprite bombTrapSprite;

    [Header("MAIN ACTION BUTTON")]
    [SerializeField] private Button mainActionButton;
    [SerializeField] private Image mainActionButtonImage;

    [SerializeField] private Sprite startButtonSprite;
    [SerializeField] private Sprite flipButtonSprite;

    [Header("GAME RULES")]
    [SerializeField] private int startingTurns = 6;
    [SerializeField] private int pairsToWin = 3;
    [SerializeField] private int rewardCoins = 600;

    [Header("TRAP SETTINGS")]
    [SerializeField] private int loseTurnTrapCount = 2;
    [SerializeField] private int bombTrapCount = 2;

    [Header("TIMING")]
    [SerializeField] private float revealDuration = 0.8f;

    [Header("GAMEPLAY UI")]
    [SerializeField] private TMP_Text turnsText;
    [SerializeField] private TMP_Text pairsText;
    [SerializeField] private TMP_Text statusText;

    [Header("RESULT UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text resultDescriptionText;
    [SerializeField] private TMP_Text rewardText;

    [SerializeField] private Button retryButton;
    [SerializeField] private Button exitButton;

    [Header("SCENE NAVIGATION")]
    [SerializeField] private string returnSceneName;

    // Cards currently selected by the player.
    private readonly List<MemoryCardView> selectedCards =
        new List<MemoryCardView>();

    private int turnsRemaining;
    private int matchedPairCount;

    private bool isGameStarted;
    private bool isGameActive;
    private bool isResolving;
    private bool rewardGranted;

    // ==========================================
    // UNITY LIFECYCLE
    // ==========================================

    private void Start()
    {
        if (!ValidateSetup())
        {
            enabled = false;
            return;
        }

        if (mainActionButtonImage == null)
            mainActionButtonImage =
                mainActionButton.GetComponent<Image>();

        mainActionButton.onClick.AddListener(HandleMainAction);

        if (retryButton != null)
            retryButton.onClick.AddListener(RestartGame);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        PrepareInitialState();
    }

    private void OnDestroy()
    {
        if (mainActionButton != null)
            mainActionButton.onClick.RemoveListener(HandleMainAction);

        if (retryButton != null)
            retryButton.onClick.RemoveListener(RestartGame);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitGame);
    }

    // ==========================================
    // VALIDATION
    // ==========================================

    private bool ValidateSetup()
    {
        if (cardSlots == null || cardSlots.Length != 12)
        {
            Debug.LogError(
                "Lost Memories requires exactly 12 card slots."
            );
            return false;
        }

        if (normalCardSprites == null ||
            normalCardSprites.Length < 4)
        {
            Debug.LogError(
                "At least 4 normal card sprites are required."
            );
            return false;
        }

        if (loseTurnTrapSprite == null ||
            bombTrapSprite == null)
        {
            Debug.LogError("Trap card sprites are missing.");
            return false;
        }

        if (mainActionButton == null)
        {
            Debug.LogError("Main Action Button is missing.");
            return false;
        }

        foreach (MemoryCardView card in cardSlots)
        {
            if (card == null)
            {
                Debug.LogError("One or more Card Slots are missing.");
                return false;
            }
        }

        if (loseTurnTrapCount + bombTrapCount != 4)
        {
            Debug.LogError(
                "There must be exactly 4 trap cards."
            );
            return false;
        }

        if (startingTurns <= 0 || pairsToWin <= 0)
        {
            Debug.LogError("Game rules must be greater than zero.");
            return false;
        }

        return true;
    }

    // ==========================================
    // INITIAL STATE
    // ==========================================

    private void PrepareInitialState()
    {
        isGameStarted = false;
        isGameActive = false;
        isResolving = false;

        turnsRemaining = startingTurns;
        matchedPairCount = 0;

        selectedCards.Clear();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (mainActionButtonImage != null)
            mainActionButtonImage.sprite = startButtonSprite;

        SetAllCardsSelectable(false);

        UpdateHUD();
        SetStatus("Press Start to begin.");
    }

    // ==========================================
    // MAIN ACTION BUTTON
    // ==========================================

    private void HandleMainAction()
    {
        if (!isGameStarted)
        {
            StartGame();
            return;
        }

        if (!isGameActive || isResolving)
            return;

        ConfirmSelectedCards();
    }

    // ==========================================
    // START GAME
    // ==========================================

    private void StartGame()
    {
        StopAllCoroutines();

        turnsRemaining = startingTurns;
        matchedPairCount = 0;

        selectedCards.Clear();

        isGameStarted = true;
        isGameActive = true;
        isResolving = false;
        rewardGranted = false;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (rewardText != null)
            rewardText.gameObject.SetActive(true);

        if (mainActionButton != null)
            mainActionButton.interactable = true;

        if (mainActionButtonImage != null)
            mainActionButtonImage.sprite = flipButtonSprite;

        CreateAndShuffleDeck();

        SetAllCardsSelectable(true);

        UpdateHUD();
        SetStatus("Select two cards, then press Flip.");
    }

    // ==========================================
    // DECK GENERATION
    // ==========================================

    private void CreateAndShuffleDeck()
    {
        List<CardSetup> deck = new List<CardSetup>();

        // 4 pairs = 8 normal cards.
        for (int pairId = 0; pairId < 4; pairId++)
        {
            for (int i = 0; i < 2; i++)
            {
                deck.Add(new CardSetup
                {
                    Kind = MemoryCardKind.Element,
                    PairId = pairId,
                    FaceSprite = normalCardSprites[pairId]
                });
            }
        }

        // Lose Turn traps.
        for (int i = 0; i < loseTurnTrapCount; i++)
        {
            deck.Add(new CardSetup
            {
                Kind = MemoryCardKind.LoseTurnTrap,
                PairId = -1,
                FaceSprite = loseTurnTrapSprite
            });
        }

        // Bomb traps.
        for (int i = 0; i < bombTrapCount; i++)
        {
            deck.Add(new CardSetup
            {
                Kind = MemoryCardKind.BombTrap,
                PairId = -1,
                FaceSprite = bombTrapSprite
            });
        }

        ShuffleDeck(deck);

        for (int i = 0; i < cardSlots.Length; i++)
        {
            CardSetup data = deck[i];

            cardSlots[i].Initialize(
                this,
                data.Kind,
                data.PairId,
                data.FaceSprite
            );
        }
    }

    private void ShuffleDeck(List<CardSetup> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            CardSetup temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    // ==========================================
    // CARD SELECTION
    // ==========================================

    public void OnCardSelected(MemoryCardView card)
    {
        if (!isGameStarted || !isGameActive || isResolving)
            return;

        if (card == null || !card.CanSelect)
            return;

        // Click again to deselect.
        if (selectedCards.Contains(card))
        {
            selectedCards.Remove(card);
            card.SetSelected(false);

            SetStatus(
                $"Selected: {selectedCards.Count}/2"
            );

            return;
        }

        // Select first or second card.
        if (selectedCards.Count < 2)
        {
            selectedCards.Add(card);
            card.SetSelected(true);
        }
        else
        {
            // Replace the second selected card.
            MemoryCardView oldCard = selectedCards[1];

            oldCard.SetSelected(false);

            selectedCards[1] = card;
            card.SetSelected(true);
        }

        SetStatus($"Selected: {selectedCards.Count}/2");
    }

    // ==========================================
    // CONFIRM SELECTED CARDS
    // ==========================================

    private void ConfirmSelectedCards()
    {
        if (selectedCards.Count != 2)
        {
            SetStatus("Please select two cards first.");
            return;
        }

        MemoryCardView first = selectedCards[0];
        MemoryCardView second = selectedCards[1];

        if (first == null || second == null ||
            !first.CanSelect || !second.CanSelect)
        {
            selectedCards.Clear();
            SetStatus("Invalid card selection.");
            return;
        }

        isResolving = true;

        SetAllCardsSelectable(false);

        first.SetSelected(false);
        second.SetSelected(false);

        StartCoroutine(ResolveTurn(first, second));
    }

    // ==========================================
    // RESOLVE TURN
    // ==========================================

    private IEnumerator ResolveTurn(
        MemoryCardView first,
        MemoryCardView second)
    {
        // Reveal both selected cards.
        first.RevealCard();
        second.RevealCard();

        // Every confirmed pair costs one base turn.
        turnsRemaining--;

        UpdateHUD();

        yield return new WaitForSeconds(revealDuration);

        int trapCount = 0;

        if (first.Kind != MemoryCardKind.Element)
            trapCount++;

        if (second.Kind != MemoryCardKind.Element)
            trapCount++;

        if (trapCount > 0)
        {
            yield return HandleTrapCards(
                first,
                second,
                trapCount
            );

            if (!isGameActive)
                yield break;
        }
        else if (first.PairId == second.PairId)
        {
            HandleMatchedCards(first, second);
        }
        else
        {
            SetStatus("The memories do not match!");

            yield return new WaitForSeconds(revealDuration);

            first.HideCard();
            second.HideCard();

            ShuffleUnmatchedCards();
        }

        selectedCards.Clear();

        UpdateHUD();

        // Victory check.
        if (matchedPairCount >= pairsToWin)
        {
            EndGame(true);
            yield break;
        }

        // Defeat check.
        if (turnsRemaining <= 0)
        {
            EndGame(false);
            yield break;
        }

        isResolving = false;

        SetAllCardsSelectable(true);

        SetStatus("Select two cards, then press Flip.");
    }

    // ==========================================
    // TRAP SYSTEM
    // ==========================================

    private IEnumerator HandleTrapCards(
        MemoryCardView first,
        MemoryCardView second,
        int trapCount)
    {
        // The first selected trap determines the effect.
        MemoryCardView firstTrap =
            first.Kind != MemoryCardKind.Element
                ? first
                : second;

        int multiplier = trapCount == 2 ? 2 : 1;

        if (trapCount == 2)
            SetStatus("DOUBLE TRAP!");

        // --------------------------------------
        // LOSE TURN TRAP
        // --------------------------------------

        if (firstTrap.Kind == MemoryCardKind.LoseTurnTrap)
        {
            int extraTurns = multiplier;

            turnsRemaining -= extraTurns;

            SetStatus(
                multiplier == 2
                    ? "Double Lose Turn! -2 extra turns."
                    : "Lose Turn! -1 extra turn."
            );
        }

        // --------------------------------------
        // BOMB TRAP
        // --------------------------------------

        else if (firstTrap.Kind == MemoryCardKind.BombTrap)
        {
            // Double Bomb or Bomb + Lose Turn
            // when Bomb was selected first.
            if (multiplier == 2)
            {
                SetStatus("Double Bomb! Instant defeat!");

                yield return new WaitForSeconds(revealDuration);

                first.HideCard();
                second.HideCard();

                EndGame(false);
                yield break;
            }

            // Single Bomb:
            // Destroy one previously completed pair.
            bool destroyedPair = DestroyRandomMatchedPair();

            if (!destroyedPair)
            {
                SetStatus(
                    "Bomb! No completed pair! Instant defeat!"
                );

                yield return new WaitForSeconds(revealDuration);

                first.HideCard();
                second.HideCard();

                EndGame(false);
                yield break;
            }

            SetStatus(
                "Bomb! One completed pair was destroyed!"
            );
        }

        yield return new WaitForSeconds(revealDuration);

        first.HideCard();
        second.HideCard();

        // Shuffle all unfinished cards,
        // including the pair destroyed by the bomb.
        ShuffleUnmatchedCards();
    }

    // ==========================================
    // MATCH SYSTEM
    // ==========================================

    private void HandleMatchedCards(
        MemoryCardView first,
        MemoryCardView second)
    {
        first.SetMatched();
        second.SetMatched();

        matchedPairCount++;

        SetStatus("Memory restored!");
    }

    // ==========================================
    // BOMB: DESTROY RANDOM COMPLETED PAIR
    // ==========================================

    private bool DestroyRandomMatchedPair()
    {
        Dictionary<int, List<MemoryCardView>> pairs =
            new Dictionary<int, List<MemoryCardView>>();

        foreach (MemoryCardView card in cardSlots)
        {
            if (card == null || !card.IsMatched)
                continue;

            if (!pairs.ContainsKey(card.PairId))
            {
                pairs.Add(
                    card.PairId,
                    new List<MemoryCardView>()
                );
            }

            pairs[card.PairId].Add(card);
        }

        List<int> validPairIds = new List<int>();

        foreach (KeyValuePair<int, List<MemoryCardView>> pair in pairs)
        {
            if (pair.Value.Count == 2)
                validPairIds.Add(pair.Key);
        }

        // No completed pair to destroy.
        if (validPairIds.Count == 0)
            return false;

        int randomIndex = Random.Range(0, validPairIds.Count);

        int selectedPairId = validPairIds[randomIndex];

        List<MemoryCardView> destroyedPair =
            pairs[selectedPairId];

        // Return both cards to unfinished state.
        foreach (MemoryCardView card in destroyedPair)
        {
            card.ResetMatchedByBomb();
        }

        matchedPairCount--;

        UpdateHUD();

        return true;
    }

    // ==========================================
    // SHUFFLE UNFINISHED CARDS ONLY
    // ==========================================

    private void ShuffleUnmatchedCards()
    {
        List<MemoryCardView> movableCards =
            new List<MemoryCardView>();

        List<CardSetup> movableData =
            new List<CardSetup>();

        // Collect only unfinished cards.
        foreach (MemoryCardView card in cardSlots)
        {
            if (card == null || card.IsMatched)
                continue;

            movableCards.Add(card);

            movableData.Add(new CardSetup
            {
                Kind = card.Kind,
                PairId = card.PairId,
                FaceSprite = GetCardSprite(card.Kind, card.PairId)
            });
        }

        // Shuffle their data.
        ShuffleDeck(movableData);

        // Redistribute data only among unfinished slots.
        for (int i = 0; i < movableCards.Count; i++)
        {
            MemoryCardView card = movableCards[i];
            CardSetup data = movableData[i];

            card.SetCardData(
                data.Kind,
                data.PairId,
                data.FaceSprite
            );

            card.HideCard();
        }

        // Matched cards are never modified here.
    }

    private Sprite GetCardSprite(
        MemoryCardKind kind,
        int pairId)
    {
        switch (kind)
        {
            case MemoryCardKind.Element:
                if (pairId >= 0 &&
                    pairId < normalCardSprites.Length)
                {
                    return normalCardSprites[pairId];
                }

                Debug.LogError($"Invalid PairId: {pairId}");
                return null;

            case MemoryCardKind.LoseTurnTrap:
                return loseTurnTrapSprite;

            case MemoryCardKind.BombTrap:
                return bombTrapSprite;

            default:
                return null;
        }
    }

    // ==========================================
    // UI UPDATE
    // ==========================================

    private void SetAllCardsSelectable(bool selectable)
    {
        foreach (MemoryCardView card in cardSlots)
        {
            if (card != null)
                card.SetSelectable(selectable);
        }
    }

    private void UpdateHUD()
    {
        if (turnsText != null)
            turnsText.text = $"Turns: {turnsRemaining}";

        if (pairsText != null)
            pairsText.text =
                $"Pairs: {matchedPairCount}/{pairsToWin}";
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    // ==========================================
    // END GAME
    // ==========================================

    private void EndGame(bool won)
    {
        if (!isGameActive)
            return;

        isGameActive = false;
        isResolving = false;

        selectedCards.Clear();

        SetAllCardsSelectable(false);

        if (mainActionButton != null)
            mainActionButton.interactable = false;

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (won)
        {
            ShowVictoryResult();
        }
        else
        {
            ShowDefeatResult();
        }
    }

    private void ShowVictoryResult()
    {
        if (resultTitleText != null)
            resultTitleText.text = "VICTORY!";

        if (resultDescriptionText != null)
            resultDescriptionText.text =
                "You restored the lost memories!";

        if (rewardText != null)
        {
            rewardText.gameObject.SetActive(true);

            if (!rewardGranted)
            {
                if (MainCoinWallet.Instance != null)
                {
                    MainCoinWallet.Instance.AddCoins(rewardCoins);
                    rewardGranted = true;

                    rewardText.text = $"+{rewardCoins} Coins";
                }
                else
                {
                    rewardText.text = "Reward unavailable.";

                    Debug.LogError(
                        "MainCoinWallet.Instance is missing."
                    );
                }
            }
        }
    }

    private void ShowDefeatResult()
    {
        if (resultTitleText != null)
            resultTitleText.text = "DEFEAT";

        if (resultDescriptionText != null)
            resultDescriptionText.text =
                "The memories remain frozen...";

        if (rewardText != null)
            rewardText.gameObject.SetActive(false);
    }

    // ==========================================
    // RESULT BUTTONS
    // ==========================================

    public void RestartGame()
    {
        if (rewardText != null)
            rewardText.gameObject.SetActive(true);

        if (mainActionButton != null)
            mainActionButton.interactable = true;

        StartGame();
    }

    public void ExitGame()
    {
        if (string.IsNullOrWhiteSpace(returnSceneName))
        {
            Debug.LogWarning(
                "Return Scene Name is not assigned."
            );
            return;
        }

        SceneManager.LoadScene(returnSceneName);
    }
}
