
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(AudioSource))]
public class ButtonClickAudio : MonoBehaviour
{
    [Header("Button cần nhấn")]
    [SerializeField] private Button targetButton;

    [Header("File âm thanh khi nhấn")]
    [SerializeField] private AudioClip clickSound;

    [Header("Âm lượng")]
    [Range(0f, 2f)]
    [SerializeField] private float volume = 1.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        if (targetButton == null)
            targetButton = GetComponent<Button>();

        if (targetButton != null)
            targetButton.onClick.AddListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        if (clickSound == null)
        {
            Debug.LogWarning(
                "Chưa gán file âm thanh cho Button: " + gameObject.name
            );
            return;
        }

        audioSource.PlayOneShot(clickSound, volume);
    }

    private void OnDestroy()
    {
        if (targetButton != null)
            targetButton.onClick.RemoveListener(PlayClickSound);
    }
}
