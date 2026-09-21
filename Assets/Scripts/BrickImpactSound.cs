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

    private void OnCollisionEnter(Collision collision)
    {
        float impactSpeed = collision.relativeVelocity.magnitude;
        Debug.Log($"[BrickImpactSound] {gameObject.name} <- {collision.collider.name}, hız={impactSpeed:F2} (eşik={minImpactSpeed})");

        if (Time.time - lastPlayTime < retriggerCooldown) return;
        if (impactSpeed < minImpactSpeed) return;

        float normalizedIntensity = Mathf.InverseLerp(minImpactSpeed, maxImpactSpeed, impactSpeed);
        float volume = Mathf.Lerp(minVolume, 1f, normalizedIntensity);
        Vector3 contactPoint = collision.GetContact(0).point;

        if (AudioManager.Instance != null)
        {
            AudioClip clip = AudioManager.PickRandom(impactClips);
            AudioManager.Instance.PlayOneShot(clip, contactPoint, volume);
        }

        OnImpact?.Invoke(contactPoint, normalizedIntensity, this);

        lastPlayTime = Time.time;
    }
}
