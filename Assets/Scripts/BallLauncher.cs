using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BallLauncher : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform innerChargeCircle;
    [SerializeField] private Transform outerBorderCircle;
    [SerializeField] private Camera aimCamera;

    [Header("Güç çemberi")]
    [SerializeField] private float chargeCycleDuration = 1.2f;
    [SerializeField] private float minCircleScale = 0.15f;
    [SerializeField] private float maxCircleScale = 1f;
    [Tooltip("Güç eğrisinin eğimi. 1 = doğrusal (eskisi gibi). Büyüdükçe çember başta hızlı dolar, maksimuma yakın değerlerde daha uzun kalır — zayıf atış yapmak zorlaşır.")]
    [SerializeField, Range(1f, 4f)] private float chargeCurvePower = 1.8f;

    [Header("Atış gücü")]
    [SerializeField] private float minShotSpeed = 4f;
    [SerializeField] private float maxShotSpeed = 14f;

    [Header("Fırlatma sallanması")]
    [Tooltip("Topun fırlatılırken etrafında döneceği açısal hız aralığı (derece/saniye) — gerçek bir cismin elden çıkarken düzensiz dönmesi hissi için. Ekseni her atışta rastgele.")]
    [SerializeField] private float minSpinSpeed = 90f;
    [SerializeField] private float maxSpinSpeed = 360f;

    [Header("Nişan düzlemi")]
    [Tooltip("Kameradan, tuğla yapılarının bulunduğu derinliğe olan mesafe. Topun kendi derinliği DEĞİL.")]
    [SerializeField] private float aimPlaneDistance = 10f;

    [Header("Dokunma filtresi")]
    [SerializeField] private bool requireMinimumHoldTime = false;
    [SerializeField] private float minimumHoldTime = 0.05f;

    [Header("Zamanlama")]
    [SerializeField] private float shotCooldown = 0.25f;

    [Header("Mermi")]
    [SerializeField] private int startingAmmo = 3;

    [Header("Ses")]
    [SerializeField] private AudioClip[] launchClips;
    [SerializeField] private float launchVolume = 0.9f;

    [Header("Perfect atış")]
    [Tooltip("Güç değeri (0-1, eğriden geçmiş hâli) bu eşiğe ulaşınca atış 'perfect' sayılır.")]
    [SerializeField, Range(0.5f, 1f)] private float perfectThreshold = 0.98f;
    [Tooltip("Perfect atışta topun hızı normal hesaplanan hızın kaç katı olsun (1.25 = %25 daha hızlı).")]
    [SerializeField] private float perfectSpeedMultiplier = 1.25f;
    [Tooltip("Normal fırlatma sesine EK olarak, perfect atışta üst üste çalınan ikinci ses.")]
    [SerializeField] private AudioClip[] perfectLaunchClips;
    [SerializeField] private float perfectLaunchVolume = 0.9f;

    private Rigidbody currentBall;
    private bool isCharging;
    private float chargeValue;
    private float chargeProgress;
    private float pressStartTime;
    private float cooldownTimer;
    private int remainingAmmo;

    public int RemainingAmmo => remainingAmmo;
    public static event System.Action<int> OnAmmoChanged;
    public static event System.Action OnAmmoDepleted;
    /// <summary>Basılı tutarken her karede güncel 0-1 güç değeriyle, atış/bırakma anında 0 ile tetiklenir.</summary>
    public static event System.Action<float> OnChargeChanged;
    /// <summary>Perfect bir atış çıktığı anda tetiklenir (sarsıntı gibi tüketiciler için).</summary>
    public static event System.Action OnPerfectShot;

    private void Start()
    {
        if (aimCamera == null) aimCamera = Camera.main;
        HideChargeVisual();
        remainingAmmo = startingAmmo;
        OnAmmoChanged?.Invoke(remainingAmmo);
        if (remainingAmmo > 0) SpawnBall();
    }

    private void Update()
    {
        HandleRespawnCooldown();

        Pointer pointer = Pointer.current;
        if (pointer == null || currentBall == null) return;

        if (pointer.press.wasPressedThisFrame && !IsOverUI(pointer))
        {
            StartCharging();
        }

        if (isCharging)
        {
            Vector2 currentScreenPos = pointer.position.ReadValue();
            UpdateCrosshairPosition(ScreenPointAtDepth(currentScreenPos, SpawnForwardDistance()));
            AdvanceCharge();

            if (pointer.press.wasReleasedThisFrame)
            {
                FireBall(currentScreenPos);
            }
        }
    }

    private void HandleRespawnCooldown()
    {
        if (currentBall != null || cooldownTimer <= 0f) return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f && remainingAmmo > 0) SpawnBall();
    }

    private void StartCharging()
    {
        isCharging = true;
        chargeProgress = 0f;
        chargeValue = 0f;
        pressStartTime = Time.time;
        ShowChargeVisual();
        UpdateChargeVisual();
    }

    private void AdvanceCharge()
    {
        // Testere dişi döngü: sınıra ulaşınca sıçrayarak sıfırlanır, sürüklemeden bağımsız.
        // chargeProgress zamanla doğrusal ilerler; chargeValue (çember + atış gücü) onun ease-out
        // eğrisi: başta hızlı yükselir, sona doğru yavaşlayıp maksimuma yakın daha uzun kalır.
        chargeProgress += Time.deltaTime / chargeCycleDuration;
        if (chargeProgress >= 1f) chargeProgress -= 1f;
        chargeValue = 1f - Mathf.Pow(1f - chargeProgress, chargeCurvePower);
        UpdateChargeVisual();
        OnChargeChanged?.Invoke(chargeValue);
    }

    private void FireBall(Vector2 releaseScreenPos)
    {
        isCharging = false;
        HideChargeVisual();
        OnChargeChanged?.Invoke(0f);

        float holdDuration = Time.time - pressStartTime;
        bool validShot = !requireMinimumHoldTime || holdDuration >= minimumHoldTime;

        if (validShot)
        {
            Vector3 targetPoint = ScreenPointToAimPlane(releaseScreenPos);
            Vector3 direction = targetPoint - spawnPoint.position;
            if (direction.sqrMagnitude < 0.0001f) direction = aimCamera.transform.forward;
            direction.Normalize();

            float speed = Mathf.Lerp(minShotSpeed, maxShotSpeed, chargeValue);
            bool isPerfect = chargeValue >= perfectThreshold;
            if (isPerfect) speed *= perfectSpeedMultiplier;

            Debug.Log($"[BallLauncher] screenPos={releaseScreenPos}, screenSize=({Screen.width},{Screen.height}), camPos={aimCamera.transform.position}, camRot={aimCamera.transform.eulerAngles}, targetPoint={targetPoint}, spawn={spawnPoint.position}, direction={direction}");

            float spinSpeed = Mathf.Lerp(minSpinSpeed, maxSpinSpeed, chargeValue) * Mathf.Deg2Rad;

            currentBall.isKinematic = false;
            currentBall.linearVelocity = direction * speed;
            currentBall.angularVelocity = Random.onUnitSphere * spinSpeed;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOneShot(AudioManager.PickRandom(launchClips), spawnPoint.position, launchVolume);
                if (isPerfect)
                {
                    AudioManager.Instance.PlayOneShot(AudioManager.PickRandom(perfectLaunchClips), spawnPoint.position, perfectLaunchVolume);
                }
            }

            if (isPerfect)
            {
                PerfectBallSkin skin = currentBall.GetComponent<PerfectBallSkin>();
                if (skin != null) skin.Activate();
                PerfectBallExplosion explosion = currentBall.GetComponent<PerfectBallExplosion>();
                if (explosion != null) explosion.Activate();
                OnPerfectShot?.Invoke();
            }

            currentBall = null;

            remainingAmmo--;
            OnAmmoChanged?.Invoke(remainingAmmo);
            if (remainingAmmo <= 0) OnAmmoDepleted?.Invoke();
        }

        cooldownTimer = shotCooldown;
    }

    /// <summary>
    /// Seviye bittiğinde çağrılır: devam eden basılı tutmayı iptal eder, çemberleri gizler ve
    /// bu bileşeni kapatır — bitiş ekranı açıkken yeni atış çıkmasın.
    /// </summary>
    public void DisableInput()
    {
        isCharging = false;
        HideChargeVisual();
        OnChargeChanged?.Invoke(0f);
        enabled = false;
    }

    private Vector3 ScreenPointToAimPlane(Vector2 screenPos)
    {
        return ScreenPointAtDepth(screenPos, aimPlaneDistance);
    }

    private Vector3 ScreenPointAtDepth(Vector2 screenPos, float depth)
    {
        Vector3 screenPoint = new Vector3(screenPos.x, screenPos.y, depth);
        return aimCamera.ScreenToWorldPoint(screenPoint);
    }

    private float SpawnForwardDistance()
    {
        return Vector3.Dot(spawnPoint.position - aimCamera.transform.position, aimCamera.transform.forward);
    }

    private void SpawnBall()
    {
        cooldownTimer = 0f;
        GameObject instance = Instantiate(ballPrefab, spawnPoint.position, spawnPoint.rotation);
        currentBall = instance.GetComponent<Rigidbody>();
        currentBall.isKinematic = true;
    }

    private void UpdateCrosshairPosition(Vector3 worldPos)
    {
        if (innerChargeCircle != null) innerChargeCircle.position = worldPos;
        if (outerBorderCircle != null) outerBorderCircle.position = worldPos;
    }

    private void UpdateChargeVisual()
    {
        if (innerChargeCircle == null) return;
        float scale = Mathf.Lerp(minCircleScale, maxCircleScale, chargeValue);
        innerChargeCircle.localScale = Vector3.one * scale;
    }

    private void ShowChargeVisual()
    {
        if (innerChargeCircle != null) innerChargeCircle.gameObject.SetActive(true);
        if (outerBorderCircle != null) outerBorderCircle.gameObject.SetActive(true);
    }

    private void HideChargeVisual()
    {
        if (innerChargeCircle != null) innerChargeCircle.gameObject.SetActive(false);
        if (outerBorderCircle != null) outerBorderCircle.gameObject.SetActive(false);
    }

    private bool IsOverUI(Pointer pointer)
    {
        if (EventSystem.current == null) return false;

        // Touch'ta EventSystem'in dokunmayı doğru eşleştirmesi için touch id gerekiyor,
        // mouse'ta parametresiz hâli yeterli.
        if (pointer is Touchscreen touchscreen)
        {
            return EventSystem.current.IsPointerOverGameObject(touchscreen.primaryTouch.touchId.ReadValue());
        }

        return EventSystem.current.IsPointerOverGameObject();
    }
}
