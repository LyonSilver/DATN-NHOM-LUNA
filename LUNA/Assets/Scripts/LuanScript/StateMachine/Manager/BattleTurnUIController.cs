
using UnityEngine;

public class BattleTurnUIController : MonoBehaviour
{
    [Header("Battle Systems")]
    [SerializeField]
    private BattleStateMachine battleStateMachine;

    [SerializeField]
    private OneCardTurnController oneCardTurnController;

    [Header("Card System")]
    [SerializeField] private CardManager cardManager;

    [Header("UI")]
    [SerializeField] private CanvasGroup handCardGroup;
    [SerializeField] private CanvasGroup endTurnButtonGroup;

    // Chỉ ẩn Hand khi CardManager báo
    // hiệu ứng Dissolve đã hoàn tất.
    private bool hideHandAfterDissolve;

    private void LateUpdate()
    {
        if (battleStateMachine == null ||
            oneCardTurnController == null ||
            cardManager == null)
            return;

        bool isPlayerTurn =
            battleStateMachine.IsPlayerTurn() &&
            !battleStateMachine.IsBattleFinished();

        bool canUseCard =
            isPlayerTurn &&
            oneCardTurnController.CanUseCard();

        bool isUsingCard = cardManager.IsUsingCard;

        // Khi bắt đầu Player Turn mới
        // hoặc Card trước bị hủy,
        // mở lại Hand.
        if (canUseCard && !isUsingCard)
        {
            hideHandAfterDissolve = false;
        }

        // HAND CARD:
        // Trong Dissolve vẫn hiện Hand.
        // Chỉ ẩn khi Dissolve hoàn tất.
        bool showHand =
            isPlayerTurn &&
            !hideHandAfterDissolve;

        // END TURN:
        // Ẩn ngay lúc đang dùng Card.
        bool showEndTurn =
            canUseCard && !isUsingCard;

        // Hand vẫn hiển thị trong Dissolve
        // nhưng không cho click lá thứ hai.
        bool handInteractable =
            showHand && !isUsingCard;

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

    // CardManager gọi hàm này sau
    // khi Dissolve kết thúc thành công.
    public void NotifyDissolveFinished()
    {
        hideHandAfterDissolve = true;

        SetVisible(handCardGroup, false, false);

        Debug.Log(
            "BattleTurnUI: Dissolve xong, đã ẩn Hand."
        );
    }

    // Có thể dùng khi Card bị hủy.
    public void NotifyCardCancelled()
    {
        hideHandAfterDissolve = false;
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