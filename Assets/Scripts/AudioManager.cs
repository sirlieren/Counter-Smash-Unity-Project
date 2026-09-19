using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Ses kaynağı havuzu")]
    [Tooltip("Aynı anda çalabilecek maksimum ses sayısı. Bir yapı yıkıldığında onlarca çarpışma birden olabileceği için sınırsız değil — havuz dolunca en eski ses kesilip yeniden kullanılır.")]
    [SerializeField] private int voiceCount = 10;
    [SerializeField] private float defaultPitchVariance = 0.08f;

    [Header("Mermi bitti sesi")]
    [Tooltip("BallLauncher.OnAmmoDepleted tetiklendiğinde çalınır.")]
    [SerializeField] private AudioClip[] ammoDepletedClips;
    [SerializeField] private float ammoDepletedVolume = 0.8f;

    private AudioSource[] voices;
    private int nextVoice;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        voices = new AudioSource[voiceCount];
        for (int i = 0; i < voiceCount; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            voices[i] = source;
        }
    }

    private void OnEnable()
    {
        BallLauncher.OnAmmoDepleted += HandleAmmoDepleted;
    }

    private void OnDisable()
    {
        BallLauncher.OnAmmoDepleted -= HandleAmmoDepleted;
    }

    private void HandleAmmoDepleted()
    {
        PlayOneShot(PickRandom(ammoDepletedClips), transform.position, ammoDepletedVolume, spatial: false);
    }

    /// <summary>
    /// Havuzdaki bir sonraki ses kaynağını kullanarak clip'i çalar.
    /// spatial=true iken pozisyona göre 3D ses (uzaklaşınca kısılır), false iken UI benzeri sabit seviyeli ses.
    /// </summary>
    public void PlayOneShot(AudioClip clip, Vector3 position, float volume = 1f, float pitchVariance = -1f, bool spatial = true)
    {
        if (clip == null) return;

        AudioSource source = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;

        source.transform.position = position;
        source.spatialBlend = spatial ? 1f : 0f;
        float variance = pitchVariance >= 0f ? pitchVariance : defaultPitchVariance;
        source.pitch = 1f + Random.Range(-variance, variance);
        source.volume = Mathf.Clamp01(volume);
        source.clip = clip;
        source.Play();
    }

    public static AudioClip PickRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}
