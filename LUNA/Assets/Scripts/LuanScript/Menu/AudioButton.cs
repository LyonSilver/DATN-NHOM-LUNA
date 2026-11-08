using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioButton : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip audioEffect;

    [Header("Buttons")]
    [SerializeField] private List<Button> buttons = new List<Button>();

    private AudioSource audioSource;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = 1f;
        audioSource.spatialBlend = 0f;

        foreach (Button button in buttons)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(PlayAudio);
                button.onClick.AddListener(PlayAudio);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (Button button in buttons)
        {
            if (button != null)
                button.onClick.RemoveListener(PlayAudio);
        }
    }

    private void PlayAudio()
    {
        if (audioEffect == null)
            return;

        audioSource.PlayOneShot(audioEffect);
    }
}