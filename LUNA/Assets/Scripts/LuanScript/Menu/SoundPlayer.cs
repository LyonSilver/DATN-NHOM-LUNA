
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public enum SoundType
    {
        SFX,
        BGM
    }

    [Header("Sound Settings")]
    [SerializeField] private SoundType soundType = SoundType.SFX;

    [SerializeField] private AudioClip audioClip;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Header("Auto Play")]
    [SerializeField] private bool playOnStart = false;

    private void Start()
    {
        if (playOnStart)
        {
            PlaySound();
        }
    }

    // Phát âm thanh được gán trong Inspector
    public void PlaySound()
    {
        if (audioClip == null)
        {
            Debug.LogWarning(
                "Chưa gán AudioClip cho " + gameObject.name
            );
            return;
        }

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy AudioManager!");
            return;
        }

        switch (soundType)
        {
            case SoundType.SFX:
                AudioManager.Instance.PlaySFX(
                    audioClip,
                    volume
                );
                break;

            case SoundType.BGM:
                AudioManager.Instance.PlayBGM(audioClip);
                break;
        }
    }

    // Dừng nhạc nền
    public void StopSound()
    {
        if (AudioManager.Instance == null)
            return;

        if (soundType == SoundType.BGM)
        {
            AudioManager.Instance.StopBGM();
        }
    }
}
