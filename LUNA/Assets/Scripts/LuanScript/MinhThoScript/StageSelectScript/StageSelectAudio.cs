using UnityEngine;

public class StageSelectAudio : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] NodeSelector selector;

    [Header("Nhạc nền (Tùy chọn)")]
    [SerializeField] AudioClip music;
    [SerializeField, Range(0f, 1f)] float musicVolume = 0.4f;

    [Header("Hiệu ứng (Để trống = Dùng tiếng bíp tự tạo)")]
    [SerializeField] AudioClip moveClip;
    [SerializeField] AudioClip lockedClip;
    [SerializeField] AudioClip confirmClip;
    [SerializeField] AudioClip starClip;
    [SerializeField] AudioClip unlockClip;
    [SerializeField] AudioClip hopClip;
    [SerializeField, Range(0f, 1f)] float sfxVolume = 0.8f;

    AudioSource sfxSource;
    AudioSource musicSource;
    AudioClip genMove, genLocked, genConfirm, genStar, genUnlock, genHop;

    static AudioClip MakeTone(string clipName, float f0, float f1, float duration, float volume = 0.45f)
    {
        const int rate = 44100;
        int n = Mathf.CeilToInt(rate * duration);
        float[] data = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n;
            float f = Mathf.Lerp(f0, f1, t);
            phase += 2f * Mathf.PI * f / rate;
            float attack = Mathf.Min(1f, i / (rate * 0.005f));
            data[i] = Mathf.Sin(phase) * attack * (1f - t) * volume;
        }
        AudioClip c = AudioClip.Create(clipName, n, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }

    void Awake()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;

        genMove = MakeTone("gen_move", 520f, 640f, 0.08f);
        genLocked = MakeTone("gen_locked", 190f, 120f, 0.18f);
        genConfirm = MakeTone("gen_confirm", 600f, 920f, 0.15f);
        genStar = MakeTone("gen_star", 880f, 1320f, 0.22f);
        genUnlock = MakeTone("gen_unlock", 400f, 1200f, 0.5f);
        genHop = MakeTone("gen_hop", 300f, 700f, 0.25f);
    }

    void Start()
    {
        if (music != null)
        {
            musicSource.clip = music;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    void OnEnable()
    {
        if (selector == null) return;
        selector.Moved += PlayMove;
        selector.LockedClicked += PlayLocked;
        selector.StageChosen += OnChosen;
    }

    void OnDisable()
    {
        if (selector == null) return;
        selector.Moved -= PlayMove;
        selector.LockedClicked -= PlayLocked;
        selector.StageChosen -= OnChosen;
    }

    void Play(AudioClip clip, AudioClip generated, float pitch = 1f)
    {
        if (sfxSource == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip != null ? clip : generated, sfxVolume);
    }

    void PlayMove() { Play(moveClip, genMove); }
    void PlayLocked() { Play(lockedClip, genLocked); }
    void OnChosen(StageData data) { Play(confirmClip, genConfirm); }

    public void PlayStar(int index) { Play(starClip, genStar, 1f + 0.2f * index); }
    public void PlayUnlock() { Play(unlockClip, genUnlock); }
    public void PlayHop() { Play(hopClip, genHop); }
}