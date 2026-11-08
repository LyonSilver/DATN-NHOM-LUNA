
using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    [Header("UI Sounds")]
    public AudioClip buttonHover;
    public AudioClip buttonClick;
    public AudioClip cardTouch;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayHover()
    {
        PlaySound(buttonHover);
    }

    public void PlayClick()
    {
        PlaySound(buttonClick);
    }

    public void PlayCardTouch()
    {
        PlaySound(cardTouch);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }
}
