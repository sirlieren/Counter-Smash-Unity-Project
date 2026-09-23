using UnityEngine;

/// <summary>
/// Perfect atışta top ilk temasında küçük bir patlama yaratıp kaybolur. Ball prefabına eklenir;
/// başta kapalıdır, BallLauncher perfect atışta Activate() çağırır.
/// </summary>
public class PerfectBallExplosion : MonoBehaviour
{
    /// <summary>Patlama anında tetiklenir: (patlama noktası). Sarsıntı gibi tüketiciler için.</summary>
    public static event System.Action<Vector3> OnExploded;

    [Header("Patlama")]
    [Tooltip("Patlamadan etkilenecek yarıçap (dünya birimi). 'Küçük patlama' için düşük tut.")]
    [SerializeField] private float radius = 1.5f;
    [Tooltip("Merkezdeki tuğlaya verilen itme (Impulse — 1 kütlelik tuğlada yaklaşık hız değişimi). Kenara doğru doğrusal olarak azalır.")]
    [SerializeField] private float force = 6f;
    [Tooltip("İtmeyi yukarı doğru kaydırır — sabit kameradan bakınca tuğlalar arkaya değil, yukarı/öne dağılır ve daha görünür olur.")]
    [SerializeField] private float upwardsModifier = 0.5f;

    [Header("Efekt ve ses")]
    [Tooltip("Patlama noktasında yaratılacak parçacık prefabı (tek seferlik). Boşsa sadece fizik + ses + sarsıntı olur.")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private AudioClip[] explosionClips;
    [SerializeField] private float explosionVolume = 0.9f;

    private static readonly Collider[] overlapBuffer = new Collider[64];

    private Rigidbody ownBody;
    private BallProjectile projectile;
    private bool armed;
    private bool exploded;

    private void Awake()
    {
        ownBody = GetComponent<Rigidbody>();
        projectile = GetComponent<BallProjectile>();
    }

    private void Start()
    {
        GameplayEffectPool.Prewarm(explosionEffectPrefab, 1);
    }

    public void ResetForReuse()
    {
        armed = false;
        exploded = false;
    }

    /// <summary>Bu atış perfect mi (Activate() çağrıldı mı) — başka efektlerin çakışmaması için.</summary>
    public bool IsArmed => armed;

    public void Activate()
    {
        armed = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!armed || exploded || collision.collider.GetComponentInParent<ClearableBrick>() == null) return;
        exploded = true;

        Vector3 center = collision.GetContact(0).point;
        Explode(center);
        if (projectile != null) projectile.Despawn();
        else Destroy(gameObject);
    }

    private void Explode(Vector3 center)
    {
        int count = Physics.OverlapSphereNonAlloc(center, radius, overlapBuffer, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            Rigidbody body = overlapBuffer[i].attachedRigidbody;
            if (body == null || body == ownBody || body.isKinematic) continue;
            body.AddExplosionForce(force, center, radius, upwardsModifier, ForceMode.Impulse);
        }

        GameplayEffectPool.Spawn(explosionEffectPrefab, center, Quaternion.identity);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot(AudioManager.PickRandom(explosionClips), center, explosionVolume);
        }

        OnExploded?.Invoke(center);
    }
}
