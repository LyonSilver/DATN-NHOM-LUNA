
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour,
    IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        UIAudioManager.Instance?.PlayHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIAudioManager.Instance?.PlayClick();
    }
}
