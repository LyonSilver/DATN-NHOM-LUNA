
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ObjectAudio : MonoBehaviour
{
    [Header("Âm thanh của Object")]
    [SerializeField] private AudioClip sound;

    [Range(0f, 2f)]
    [SerializeField] private float volume = 1f;

    [SerializeField] private bool playOnStart = false;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
    }

    private void Start()
    {
        if (playOnStart)
            PlaySound();
    }

    public void PlaySound()
    {
        if (sound == null)
        {
            Debug.LogWarning("ObjectAudio: Chưa gán âm thanh cho " + gameObject.name);
            return;
        }

        source.PlayOneShot(sound, volume);
    }
}
