using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class HomeButton : MonoBehaviour, IPointerClickHandler
{
    public RadialMenuController menuController;

    [SerializeField] private UnityEvent _onClick;

    private bool canClick = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!canClick) return;

        canClick = false;
        _onClick?.Invoke();

        if (menuController != null)
            menuController.ToggleMenu();

        Invoke(nameof(ResetClick), 0.2f);
    }

    void ResetClick() => canClick = true;
}