using UnityEngine;

/// <summary>
/// Top ilk temasında (perfect atış değilse) çarpma noktasında bir parçacık efekti yaratır.
/// Perfect atışlarda bu efekt atlanır — PerfectBallExplosion zaten kendi efektini basıyor.
/// Ball prefabına eklenir.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BallHitEffect : MonoBehaviour
{
    [Tooltip("İlk çarpışma noktasında yaratılacak parçacık prefabı. Stop Action = Destroy olarak ayarlanmışsa kendini otomatik temizler.")]
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("Vuruş şiddetine göre ölçek")]
    [Tooltip("Bu hızda ve altındaki çarpışmalarda efekt en küçük ölçekte (minScale) çıkar.")]
    [SerializeField] private float minImpactSpeed = 0.4f;
    [Tooltip("Bu hızda ve üzerindeki çarpışmalarda efekt en büyük ölçekte (maxScale) çıkar.")]
    [SerializeField] private float maxImpactSpeed = 8f;
    [SerializeField] private float minScale = 0.2f;
    [SerializeField] private float maxScale = 0.75f;

    private PerfectBallExplosion explosion;
    private bool hasHit;

    private void Awake()
    {
        explosion = GetComponent<PerfectBallExplosion>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        if (explosion != null && explosion.IsArmed) return;
        if (hitEffectPrefab == null) return;

        float normalizedIntensity = Mathf.InverseLerp(minImpactSpeed, maxImpactSpeed, collision.relativeVelocity.magnitude);
        float scale = Mathf.Lerp(minScale, maxScale, normalizedIntensity);

        GameObject effect = Instantiate(hitEffectPrefab, collision.GetContact(0).point, Quaternion.identity);
        effect.transform.localScale = Vector3.one * scale;
    }
}
