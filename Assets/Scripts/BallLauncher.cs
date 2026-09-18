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

    private Rigidbody currentBall;
    private bool isCharging;
    private float chargeValue;
    private float pressStartTime;
    private float cooldownTimer;
    private int remainingAmmo;

    public int RemainingAmmo => remainingAmmo;
    public static event System.Action<int> OnAmmoChanged;
    public static event System.Action OnAmmoDepleted;

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
        chargeValue = 0f;
        pressStartTime = Time.time;
        ShowChargeVisual();
        UpdateChargeVisual();
    }

    private void AdvanceCharge()
    {
        // Testere dişi döngü: sınıra ulaşınca sıçrayarak sıfırlanır, sürüklemeden bağımsız.
        chargeValue += Time.deltaTime / chargeCycleDuration;
        if (chargeValue >= 1f) chargeValue -= 1f;
        UpdateChargeVisual();
    }

    private void FireBall(Vector2 releaseScreenPos)
    {
        isCharging = false;
        HideChargeVisual();

        float holdDuration = Time.time - pressStartTime;
        bool validShot = !requireMinimumHoldTime || holdDuration >= minimumHoldTime;

        if (validShot)
        {
            Vector3 targetPoint = ScreenPointToAimPlane(releaseScreenPos);
            Vector3 direction = targetPoint - spawnPoint.position;
            if (direction.sqrMagnitude < 0.0001f) direction = aimCamera.transform.forward;
            direction.Normalize();

            float speed = Mathf.Lerp(minShotSpeed, maxShotSpeed, chargeValue);

            Debug.Log($"[BallLauncher] screenPos={releaseScreenPos}, screenSize=({Screen.width},{Screen.height}), camPos={aimCamera.transform.position}, camRot={aimCamera.transform.eulerAngles}, targetPoint={targetPoint}, spawn={spawnPoint.position}, direction={direction}");

            float spinSpeed = Mathf.Lerp(minSpinSpeed, maxSpinSpeed, chargeValue) * Mathf.Deg2Rad;

            currentBall.isKinematic = false;
            currentBall.linearVelocity = direction * speed;
            currentBall.angularVelocity = Random.onUnitSphere * spinSpeed;
            currentBall = null;

            remainingAmmo--;
            OnAmmoChanged?.Invoke(remainingAmmo);
            if (remainingAmmo <= 0) OnAmmoDepleted?.Invoke();
        }

        cooldownTimer = shotCooldown;
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
