using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BrickImpactSound : MonoBehaviour
{
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

        float volume = Mathf.Lerp(minVolume, 1f, Mathf.InverseLerp(minImpactSpeed, maxImpactSpeed, impactSpeed));

        if (AudioManager.Instance != null)
        {
            AudioClip clip = AudioManager.PickRandom(impactClips);
            AudioManager.Instance.PlayOneShot(clip, collision.GetContact(0).point, volume);
        }

        lastPlayTime = Time.time;
    }
}
