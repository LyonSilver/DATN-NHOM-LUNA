
using UnityEngine;

public class BattleTurnUIController : MonoBehaviour
{
    [Header("Battle Systems")]
    [SerializeField] private BattleStateMachine battleStateMachine;
    [SerializeField] private OneCardTurnController oneCardTurnController;

    [Header("Card System")]
    [SerializeField] private CardManager cardManager;

    [SerializeField] private PlayerCardCinematic playerCardCinematic;

    [Header("UI")]
    [SerializeField] private CanvasGroup handCardGroup;
    [SerializeField] private CanvasGroup endTurnButtonGroup;

    // Chỉ ẩn Hand sau khi Dissolve hoàn tất.
    private bool hideHandAfterDissolve;

    private void LateUpdate()
    {
        if (battleStateMachine == null ||
            oneCardTurnController == null ||
            cardManager == null)
        {
            return;
        }

        // Kiểm tra Player Turn.
        bool isPlayerTurn =
            battleStateMachine.IsPlayerTurn() &&
            !battleStateMachine.IsBattleFinished();

        // Kiểm tra khả năng sử dụng Card.
        bool canUseCard =
            isPlayerTurn &&
            oneCardTurnController.CanUseCard();

        // Kiểm tra CardManager.
        bool isUsingCard = cardManager.IsUsingCard;

        // Kiểm tra Player Cinematic.
        bool isPlayingCinematic =
            playerCardCinematic != null &&
            playerCardCinematic.IsPlaying;

        // Khi Cinematic bắt đầu,
        // đảm bảo Hand không xuất hiện lại sau đó.
        if (isPlayingCinematic)
        {
            hideHandAfterDissolve = true;
        }

        // Khi Player Turn mới bắt đầu,
        // mở lại Hand nếu không còn hành động nào đang chạy.
        if (canUseCard &&
            !isUsingCard &&
            !isPlayingCinematic)
        {
            hideHandAfterDissolve = false;
        }

        // HAND CARD:
        // Vẫn hiện trong lúc Dissolve.
        // Ẩn sau Dissolve hoặc khi Cinematic chạy.
        bool showHand =
            isPlayerTurn &&
            !hideHandAfterDissolve &&
            !isPlayingCinematic;

        // END TURN:
        // Chỉ hiện khi Player có thể kết thúc lượt.
        bool showEndTurn =
            canUseCard &&
            !isUsingCard &&
            !isPlayingCinematic;

        // Không cho click Card khi đang sử dụng Card.
        bool handInteractable =
            showHand &&
            !isUsingCard &&
            !isPlayingCinematic;

        SetVisible(
            handCardGroup,
            showHand,
            handInteractable
        );

        SetVisible(
            endTurnButtonGroup,
            showEndTurn,
            showEndTurn
        );
    }

    // CardManager gọi sau khi Dissolve hoàn tất thành công.
    public void NotifyDissolveFinished()
    {
        hideHandAfterDissolve = true;

        SetVisible(handCardGroup, false, false);

        Debug.Log(
            "BattleTurnUI: Dissolve hoàn tất, đã ẩn Hand."
        );
    }

    // Gọi khi Card bị hủy.
    public void NotifyCardCancelled()
    {
        hideHandAfterDissolve = false;

        Debug.Log(
            "BattleTurnUI: Card bị hủy, mở lại Hand."
        );
    }

    private void SetVisible(
        CanvasGroup group,
        bool visible,
        bool interactable)
    {
        if (group == null)
            return;

        group.alpha = visible ? 1f : 0f;
        group.interactable = interactable;
        group.blocksRaycasts = interactable;
    }
}