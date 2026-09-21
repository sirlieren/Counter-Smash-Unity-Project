using UnityEngine;

/// <summary>
/// Basılı tutarken atış gücüne göre kameranın FOV'unu hafifçe kaydırır, bırakınca yumuşakça
/// başlangıç değerine döner. Kamera objesine eklenir.
/// </summary>
[RequireComponent(typeof(Camera))]
public class ChargeFov : MonoBehaviour
{
    [Tooltip("Tam güçte FOV'un başlangıç değerine eklenecek miktar (derece). Pozitif = güç arttıkça açılır (60 → 63), negatif = yaklaşır (60 → 57).")]
    [SerializeField] private float fovChangeAtMaxCharge = 3f;
    [Tooltip("FOV'un hedefe yaklaşma süresi (saniye) — küçük = tepkili, büyük = yumuşak/yavaş. Güç çemberi sıfırlandığında ani sıçramayı da bu yumuşatır.")]
    [SerializeField] private float smoothTime = 0.1f;

    private Camera cam;
    private float baseFov;
    private float targetOffset;
    private float currentOffset;
    private float offsetVelocity;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        baseFov = cam.fieldOfView;
    }

    private void OnEnable()
    {
        BallLauncher.OnChargeChanged += HandleChargeChanged;
    }

    private void OnDisable()
    {
        BallLauncher.OnChargeChanged -= HandleChargeChanged;
        currentOffset = 0f;
        targetOffset = 0f;
        offsetVelocity = 0f;
        if (cam != null) cam.fieldOfView = baseFov;
    }

    private void HandleChargeChanged(float charge01)
    {
        targetOffset = charge01 * fovChangeAtMaxCharge;
    }

    private void LateUpdate()
    {
        if (currentOffset == targetOffset) return;

        // Unscaled: hit-stop sırasında FOV donup kalmasın.
        currentOffset = Mathf.SmoothDamp(currentOffset, targetOffset, ref offsetVelocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        if (Mathf.Abs(currentOffset - targetOffset) < 0.0005f && Mathf.Abs(offsetVelocity) < 0.0005f)
        {
            currentOffset = targetOffset;
            offsetVelocity = 0f;
        }

        cam.fieldOfView = baseFov + currentOffset;
    }
}
