using UnityEngine;
using UnityEngine.EventSystems;

public class InvisibleClickArea : MonoBehaviour, IPointerClickHandler
{
    public UnityEngine.Events.UnityEvent onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
        Debug.Log("Clicked invisible area");
    }
}