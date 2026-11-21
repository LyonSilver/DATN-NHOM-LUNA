
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
    [SerializeField] private int startingTurns = 12;
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

    // Danh sách các thẻ người chơi đang chọn.
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
                "Lost Memories yêu cầu chính xác 12 ô thẻ."
            );
            return false;
        }

        if (normalCardSprites == null ||
            normalCardSprites.Length < 4)
        {
            Debug.LogError(
                "Cần ít nhất 4 hình ảnh thẻ thường."
            );
            return false;
        }

        if (loseTurnTrapSprite == null ||
            bombTrapSprite == null)
        {
            Debug.LogError("Chưa gán hình ảnh cho các thẻ bẫy.");
            return false;
        }

        if (mainActionButton == null)
        {
            Debug.LogError("Chưa gán nút hành động chính.");
            return false;
        }

        foreach (MemoryCardView card in cardSlots)
        {
            if (card == null)
            {
                Debug.LogError("Có một hoặc nhiều ô thẻ chưa được gán.");
                return false;
            }
        }

        if (loseTurnTrapCount + bombTrapCount != 4)
        {
            Debug.LogError(
                "Phải có chính xác 4 thẻ bẫy."
            );
            return false;
        }

        if (startingTurns <= 0 || pairsToWin <= 0)
        {
            Debug.LogError("Các thông số luật chơi phải lớn hơn 0.");
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
        SetStatus("Nhấn Bắt đầu để chơi.");
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
        SetStatus("Chọn 2 thẻ, sau đó nhấn Lật Thẻ.");
    }

    // ==========================================
    // DECK GENERATION
    // ==========================================

    private void CreateAndShuffleDeck()
    {
        List<CardSetup> deck = new List<CardSetup>();

        // 4 cặp = 8 thẻ thường.
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

        // Thẻ mất lượt.
        for (int i = 0; i < loseTurnTrapCount; i++)
        {
            deck.Add(new CardSetup
            {
                Kind = MemoryCardKind.LoseTurnTrap,
                PairId = -1,
                FaceSprite = loseTurnTrapSprite
            });
        }

        // Thẻ bom.
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

        // Nhấn lại thẻ đã chọn để bỏ chọn.
        if (selectedCards.Contains(card))
        {
            selectedCards.Remove(card);
            card.SetSelected(false);

            SetStatus(
                $"Đã chọn: {selectedCards.Count}/2"
            );

            return;
        }

        // Chọn thẻ thứ nhất hoặc thứ hai.
        if (selectedCards.Count < 2)
        {
            selectedCards.Add(card);
            card.SetSelected(true);
        }
        else
        {
            // Nếu chọn thẻ thứ ba, thay thế thẻ thứ hai.
            MemoryCardView oldCard = selectedCards[1];

            oldCard.SetSelected(false);

            selectedCards[1] = card;
            card.SetSelected(true);
        }

        SetStatus($"Đã chọn: {selectedCards.Count}/2");
    }

    // ==========================================
    // CONFIRM SELECTED CARDS
    // ==========================================

    private void ConfirmSelectedCards()
    {
        if (selectedCards.Count != 2)
        {
            SetStatus("Vui lòng chọn đủ 2 thẻ trước.");
            return;
        }

        MemoryCardView first = selectedCards[0];
        MemoryCardView second = selectedCards[1];

        if (first == null || second == null ||
            !first.CanSelect || !second.CanSelect)
        {
            selectedCards.Clear();
            SetStatus("Lựa chọn thẻ không hợp lệ.");
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
        // Lật hai thẻ đã chọn.
        first.RevealCard();
        second.RevealCard();

        // Mỗi lần xác nhận hai thẻ sẽ mất một lượt cơ bản.
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
            SetStatus("Hai ký ức không trùng khớp!");

            yield return new WaitForSeconds(revealDuration);

            first.HideCard();
            second.HideCard();

           // ShuffleUnmatchedCards();
        }

        selectedCards.Clear();

        UpdateHUD();

        // Kiểm tra chiến thắng.
        if (matchedPairCount >= pairsToWin)
        {
            EndGame(true);
            yield break;
        }

        // Kiểm tra thất bại.
        if (turnsRemaining <= 0)
        {
            EndGame(false);
            yield break;
        }

        isResolving = false;

        SetAllCardsSelectable(true);

        SetStatus("Chọn 2 thẻ, sau đó nhấn Lật Thẻ.");
    }

    // ==========================================
    // TRAP SYSTEM
    // ==========================================

    private IEnumerator HandleTrapCards(
        MemoryCardView first,
        MemoryCardView second,
        int trapCount)
    {
        // Thẻ bẫy được chọn trước sẽ quyết định hiệu ứng.
        MemoryCardView firstTrap =
            first.Kind != MemoryCardKind.Element
                ? first
                : second;

        int multiplier = trapCount == 2 ? 2 : 1;

        if (trapCount == 2)
            SetStatus("BẪY KÉP!");

        // --------------------------------------
        // THẺ MẤT LƯỢT
        // --------------------------------------

        if (firstTrap.Kind == MemoryCardKind.LoseTurnTrap)
        {
            int extraTurns = multiplier;

            turnsRemaining -= extraTurns;

            SetStatus(
                multiplier == 2
                    ? "Mất lượt kép! Trừ thêm 2 lượt."
                    : "Mất lượt! Trừ thêm 1 lượt."
            );
        }

        // --------------------------------------
        // THẺ BOM
        // --------------------------------------

        else if (firstTrap.Kind == MemoryCardKind.BombTrap)
        {
            // Hai thẻ bom hoặc bom được chọn trước thẻ mất lượt.
            if (multiplier == 2)
            {
                SetStatus("Hai thẻ bom! Bạn đã thất bại!");

                yield return new WaitForSeconds(revealDuration);

                first.HideCard();
                second.HideCard();

                EndGame(false);
                yield break;
            }

            // Bom đơn: phá hủy một cặp đã hoàn thành trước đó.
            bool destroyedPair = DestroyRandomMatchedPair();

            if (!destroyedPair)
            {
                SetStatus(
                    "Bom phát nổ! Bạn chưa có cặp thẻ hoàn thành và đã thất bại!"
                );

                yield return new WaitForSeconds(revealDuration);

                first.HideCard();
                second.HideCard();

                EndGame(false);
                yield break;
            }

            SetStatus(
                "Bom phát nổ! Một cặp thẻ đã ghép bị phá hủy!"
            );
        }

        yield return new WaitForSeconds(revealDuration);

        first.HideCard();
        second.HideCard();

        // Xáo trộn các thẻ chưa hoàn thành,
        // bao gồm cả cặp vừa bị bom phá hủy.
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

        SetStatus("Bạn đã khôi phục một ký ức!");
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

        // Không có cặp nào đã hoàn thành để phá hủy.
        if (validPairIds.Count == 0)
            return false;

        int randomIndex = Random.Range(0, validPairIds.Count);

        int selectedPairId = validPairIds[randomIndex];

        List<MemoryCardView> destroyedPair =
            pairs[selectedPairId];

        // Đưa cả hai thẻ trở lại trạng thái chưa hoàn thành.
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

        // Chỉ thu thập các thẻ chưa hoàn thành.
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

        // Xáo trộn dữ liệu thẻ.
        ShuffleDeck(movableData);

        // Phân phối lại dữ liệu vào các vị trí chưa hoàn thành.
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

        // Các thẻ đã ghép thành công không bị thay đổi.
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

                Debug.LogError($"PairId không hợp lệ: {pairId}");
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
            turnsText.text = $"Lượt còn lại: {turnsRemaining}";

        if (pairsText != null)
            pairsText.text =
                $"Số cặp: {matchedPairCount}/{pairsToWin}";
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
            resultTitleText.text = "CHIẾN THẮNG!";

        if (resultDescriptionText != null)
            resultDescriptionText.text =
                "Hơn mỗi con Bot:) \nBạn đã khôi phục được ký ức!";

        if (rewardText != null)
        {
            rewardText.gameObject.SetActive(true);

            if (!rewardGranted)
            {
                if (MainCoinWallet.Instance != null)
                {
                    MainCoinWallet.Instance.AddCoins(rewardCoins);
                    rewardGranted = true;

                    rewardText.text = $"+{rewardCoins} Xu";
                }
                else
                {
                    rewardText.text = "Không thể nhận phần thưởng.";

                    Debug.LogError(
                        "Không tìm thấy MainCoinWallet.Instance."
                    );
                }
            }
        }
    }

    private void ShowDefeatResult()
    {
        if (resultTitleText != null)
            resultTitleText.text = "THẤT BẠI";

        if (resultDescriptionText != null)
            resultDescriptionText.text =
                "Thua cả con Bot:) \nNhững ký ức vẫn còn bị đóng băng...";

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
                "Chưa thiết lập tên Scene quay về."
            );
            return;
        }

        SceneManager.LoadScene(returnSceneName);
    }
}
