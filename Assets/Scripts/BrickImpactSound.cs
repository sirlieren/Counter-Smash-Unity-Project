using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BrickImpactSound : MonoBehaviour
{
    /// <summary>
    /// Eşiği geçen her çarpışmada tetiklenir: (temas noktası, 0-1 arası şiddet, çarpan tuğla).
    /// Bu, sesin volume'undan farklı — sesin duyulabilirlik için bir tabanı (minVolume) var,
    /// bu şiddet ise eşikte tam 0'dan başlar; sarsıntı/hit-stop gibi tüketiciler ufak
    /// çarpışmalarda gerçekten neredeyse sıfır tepki istiyor.
    /// </summary>
    public static event System.Action<Vector3, float, BrickImpactSound> OnImpact;

    /// <summary>
    /// Bu tuğla kamera sarsıntısını kaç kez tetikledi. Tüketici (CameraShake) sayar; sayaç
    /// tuğlanın üzerinde durduğu için tuğla yok olunca kendiliğinden gider.
    /// </summary>
    public int ShakeHitCount { get; set; }


    [Header("Çarpışma sesleri")]
    [Tooltip("Rastgele seçilir — tekrarlanan tek bir sesin monoton durmaması için birkaç varyasyon iste.")]
    [SerializeField] private AudioClip[] impactClips;

    [Header("Topun tuğlaya ilk teması")]
    [Tooltip("Top üzerindeyken kullanılır. Boşsa normal çarpışma sesleri çalınır.")]
    [SerializeField] private AudioClip[] firstBrickImpactClips;
    [SerializeField, Range(0f, 2f)] private float firstBrickVolumeMultiplier = 1.2f;
    [SerializeField, Range(0f, 1f)] private float laterBrickVolumeMultiplier = 0.4f;

    [Header("Şiddet eşiği ve ölçekleme")]
    [Tooltip("Bu hızın altındaki çarpışmalar sessiz kalır — sekerken/yuvarlanırken oluşan hafif sürtünme gürültüsünü eler.")]
    [SerializeField] private float minImpactSpeed = 1.5f;
    [Tooltip("Bu hızda ve üzerinde ses tam seviyede (volume = 1) çalar.")]
    [SerializeField] private float maxImpactSpeed = 8f;
    [SerializeField] private float minVolume = 0.15f;

    [Header("Zamanlama")]
    [Tooltip("Aynı tuğlanın art arda ses tetiklemesini engelleyen minimum süre — sekerken makineli tüfek gibi durmasın diye.")]
    [SerializeField] private float retriggerCooldown = 0.08f;

    private float lastPlayTime = -999f;
    private bool hasHitBrick;
    private BallProjectile ball;

    private void Awake()
    {
        ball = GetComponent<BallProjectile>();
    }

    public void ResetForReuse()
    {
        lastPlayTime = -999f;
        hasHitBrick = false;
        ShakeHitCount = 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        bool hitBall = collision.collider.GetComponentInParent<BallProjectile>() != null;
        if (ball == null && hitBall) return; // Top-tuğla teması yalnızca top tarafından seslendirilir.

        bool hitBrick = ball != null && collision.collider.GetComponentInParent<ClearableBrick>() != null;
        if (ball != null && GetComponent<PerfectBallExplosion>() is PerfectBallExplosion explosion && explosion.IsArmed)
            return; // Perfect patlamanın ayrı sesi ve sarsıntısı var.

        float impactSpeed = collision.relativeVelocity.magnitude;

        bool firstBrickHit = hitBrick && !hasHitBrick;
        if (!firstBrickHit && Time.time - lastPlayTime < retriggerCooldown) return;
        if (impactSpeed < minImpactSpeed) return;

        float normalizedIntensity = Mathf.InverseLerp(minImpactSpeed, maxImpactSpeed, impactSpeed);
        if (hitBrick) hasHitBrick = true;
        float volume = Mathf.Lerp(minVolume, 1f, normalizedIntensity);
        if (hitBrick) volume *= firstBrickHit ? firstBrickVolumeMultiplier : laterBrickVolumeMultiplier;
        Vector3 contactPoint = collision.GetContact(0).point;

        if (AudioManager.Instance != null)
        {
            AudioClip[] clips = firstBrickHit && firstBrickImpactClips != null && firstBrickImpactClips.Length > 0
                ? firstBrickImpactClips : impactClips;
            AudioClip clip = AudioManager.PickRandom(clips);
            AudioManager.Instance.PlayOneShot(clip, contactPoint, volume);
        }

        OnImpact?.Invoke(contactPoint, normalizedIntensity, this);

        lastPlayTime = Time.time;
    }
}
