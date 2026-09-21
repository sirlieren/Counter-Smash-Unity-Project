using UnityEngine;

/// <summary>
/// Sabit kameraya çarpışma anlarında geçici, kendi kendine sönümlenen bir sallanma ekler.
/// "Trauma" (0-1) çarpışma şiddetiyle birikir, zamanla azalır; ekrana yansıyan sallanma trauma'nın
/// karesiyle ölçeklenir (küçük trauma'da neredeyse görünmez, büyük trauma'da belirgin).
/// </summary>
public class CameraShake : MonoBehaviour
{
    [Header("Genlik")]
    [Tooltip("Trauma = 1 iken kameranın kayabileceği maksimum mesafe (dünya birimi).")]
    [SerializeField] private float maxOffset = 0.12f;
    [Tooltip("Trauma = 1 iken maksimum dönüş açısı (derece) — hafif bir sallanma katkısı için.")]
    [SerializeField] private float maxRoll = 1.5f;
    [Tooltip("Perlin noise'un zamanla ne hızda değiştiği — yüksek değer daha titrek, düşük değer daha dalgalı bir sallanma verir.")]
    [SerializeField] private float noiseFrequency = 20f;

    [Header("Sönümlenme")]
    [Tooltip("Trauma'nın saniyede ne kadar azalacağı — 1 = trauma yaklaşık 1 saniyede sıfırlanır.")]
    [SerializeField] private float traumaDecayPerSecond = 2.5f;

    [Header("Tetikleme")]
    [Tooltip("Bu şiddetin altındaki çarpışmalar sarsıntıya katkı vermez.")]
    [SerializeField] private float minIntensityToShake = 0.05f;
    [Tooltip("Gelen şiddet trauma'ya eklenmeden önce karesi alınır (küçük darbeler neredeyse hiç katkı vermesin diye), bu değer o katkının genel çarpanı.")]
    [SerializeField] private float traumaGain = 1f;

    [Header("Mesafe etkisi")]
    [Tooltip("Kamera ile çarpma noktası arasındaki bu mesafeye (dünya birimi) kadar sarsıntı tam güçte. Yapılar kameradan ~8 birimde durduğu için varsayılan onların biraz ötesi.")]
    [SerializeField] private float fullStrengthDistance = 12f;
    [Tooltip("Bu mesafe ve ötesinde sarsıntı en zayıf (farStrength) seviyeye inmiş olur; arası yumuşak geçiş.")]
    [SerializeField] private float farDistance = 30f;
    [Tooltip("farDistance ve ötesindeki bir çarpmanın, yakındaki aynı şiddette bir çarpmaya göre gücü (0-1).")]
    [SerializeField, Range(0f, 1f)] private float farStrength = 0.2f;

    [Header("Perfect atış")]
    [Tooltip("Perfect atış çıktığında eklenen trauma (0-1). Ekrana yansıyan sarsıntı bunun karesiyle ölçeklendiği için 'minik' bir sarsıntı için düşük tut.")]
    [SerializeField, Range(0f, 1f)] private float perfectShotTrauma = 0.4f;
    [Tooltip("Perfect topun patladığı anda eklenen trauma (0-1), mesafe etkisiyle ölçeklenir.")]
    [SerializeField, Range(0f, 1f)] private float explosionTrauma = 0.5f;

    [Header("Tekrarlanan çarpma")]
    [Tooltip("Aynı tuğlanın her ek sarsıntısı bir öncekinin bu oranı kadar güçte olur: 0.5 → ilk çarpma %100, ikinci %50, üçüncü %25...")]
    [SerializeField, Range(0f, 1f)] private float repeatFalloff = 0.5f;

    private float trauma;
    private Vector3 restLocalPosition;
    private Quaternion restLocalRotation;
    private float noiseSeedX, noiseSeedY, noiseSeedRoll;

    private void Awake()
    {
        restLocalPosition = transform.localPosition;
        restLocalRotation = transform.localRotation;
        noiseSeedX = Random.Range(0f, 100f);
        noiseSeedY = Random.Range(0f, 100f);
        noiseSeedRoll = Random.Range(0f, 100f);
    }

    private void OnEnable()
    {
        BrickImpactSound.OnImpact += HandleImpact;
        BallLauncher.OnPerfectShot += HandlePerfectShot;
        PerfectBallExplosion.OnExploded += HandleExplosion;
    }

    private void OnDisable()
    {
        BrickImpactSound.OnImpact -= HandleImpact;
        BallLauncher.OnPerfectShot -= HandlePerfectShot;
        PerfectBallExplosion.OnExploded -= HandleExplosion;
        transform.localPosition = restLocalPosition;
        transform.localRotation = restLocalRotation;
    }

    /// <summary>
    /// Uzaktaki çarpmalar hafif hissedilsin: eşiğe kadar tam güç, eşikten farDistance'a doğru
    /// yumuşakça farStrength'e iner (sıfıra değil — uzak bir çöküş de hafifçe duyulsun).
    /// </summary>
    private float DistanceFactor(Vector3 point)
    {
        float distance = Vector3.Distance(transform.position, point);
        float farT = Mathf.InverseLerp(fullStrengthDistance, farDistance, distance);
        return Mathf.Lerp(1f, farStrength, Mathf.SmoothStep(0f, 1f, farT));
    }

    private void HandleExplosion(Vector3 point)
    {
        trauma = Mathf.Clamp01(trauma + explosionTrauma * DistanceFactor(point));
    }

    private void HandlePerfectShot()
    {
        // Mesafe/tekrar etkisi yok — kameranın kendi atışı, herkes için aynı.
        trauma = Mathf.Clamp01(trauma + perfectShotTrauma);
    }

    private void HandleImpact(Vector3 point, float intensity, BrickImpactSound source)
    {
        if (intensity < minIntensityToShake) return;

        float distanceFactor = DistanceFactor(point);

        // Aynı tuğla tekrar tekrar sarsıyorsa her seferinde repeatFalloff kadar zayıflasın
        // (1, 0.5, 0.25...). Sayaç yalnızca gerçekten sarsıntı üreten çarpmalarda artar —
        // eşik altı bir sürtünme teması ilk çarpma hakkını harcamasın.
        float repeatFactor = 1f;
        if (source != null)
        {
            repeatFactor = Mathf.Pow(repeatFalloff, source.ShakeHitCount);
            source.ShakeHitCount++;
        }

        trauma = Mathf.Clamp01(trauma + intensity * intensity * traumaGain * distanceFactor * repeatFactor);
    }

    private void LateUpdate()
    {
        if (trauma <= 0f)
        {
            transform.localPosition = restLocalPosition;
            transform.localRotation = restLocalRotation;
            return;
        }

        trauma = Mathf.Clamp01(trauma - traumaDecayPerSecond * Time.unscaledDeltaTime);

        float shake = trauma * trauma;
        float t = Time.unscaledTime * noiseFrequency;

        float offsetX = (Mathf.PerlinNoise(noiseSeedX, t) * 2f - 1f) * maxOffset * shake;
        float offsetY = (Mathf.PerlinNoise(noiseSeedY, t) * 2f - 1f) * maxOffset * shake;
        float roll = (Mathf.PerlinNoise(noiseSeedRoll, t) * 2f - 1f) * maxRoll * shake;

        transform.localPosition = restLocalPosition + new Vector3(offsetX, offsetY, 0f);
        transform.localRotation = restLocalRotation * Quaternion.Euler(0f, 0f, roll);
    }
}
