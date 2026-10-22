
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonPlayAudio : MonoBehaviour, IPointerClickHandler
{
    [Header("Âm thanh khi nhấn")]
    [SerializeField] private ObjectAudio audioObject;

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClickSound();
    }

    public void PlayClickSound()
    {
        if (audioObject != null)
        {
            audioObject.PlaySound();
        }
        else
        {
            Debug.LogWarning("Chưa gắn ObjectAudio vào Button: " + gameObject.name);
        }
    }
}
