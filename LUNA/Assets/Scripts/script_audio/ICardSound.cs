
using UnityEngine;
using UnityEngine.EventSystems;

public class UICardSound : MonoBehaviour,
    IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        UIAudioManager.Instance?.PlayHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIAudioManager.Instance?.PlayCardTouch();
    }
}
