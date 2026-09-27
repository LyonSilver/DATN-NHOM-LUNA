using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class StatusIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI valueText;

    private StatusInstance currentStatus;

    public void Setup(StatusInstance status, Sprite iconSprite)
    {
        currentStatus = status;
        iconImage.sprite = iconSprite;
        valueText.text = status.stackType == StackType.Duration ? $"{status.duration}T" : $"{status.stacks}x";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        string desc = GetDescription(currentStatus);
        TooltipUI.Instance?.Show(currentStatus.type.ToString(), desc);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance?.Hide();
    }

    private string GetDescription(StatusInstance status) => status.type switch
    {
        StatusType.Poison => $"Độc tố: Trừ {status.stacks * 5} HP mỗi lượt ({status.duration} lượt).",
        StatusType.Burn   => $"Bốc cháy: Trừ {status.stacks * 5} HP mỗi lượt ({status.duration} lượt).",
        StatusType.Weak   => $"Yếu ớt: Giảm 30% sát thương gây ra ({status.duration} lượt).",
        StatusType.Stun   => $"Choáng: Bỏ qua lượt hành động tiếp theo.",
        _ => ""
    };
}