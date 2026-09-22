using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Seviyenin kazanma/kaybetme durumunu izler. Kazanma: tüm tuğlalar temizlendi. Kaybetme: son mermi
/// atıldı, her şey durulduktan sonra hâlâ tuğla kaldı. Sonucu (kazandı mı, kaç yıldız) olayla duyurur —
/// arayüz (LevelEndUI) bunu dinler.
/// </summary>
public class LevelManager : MonoBehaviour
{
    /// <summary>Seviye bitti: (kazanıldı mı, yıldız sayısı 0-3; kaybedilince 0).</summary>
    public static event Action<bool, int> OnLevelEnded;

    [Header("Referanslar")]
    [Tooltip("Boş bırakılırsa sahnede otomatik aranır.")]
    [SerializeField] private BallLauncher launcher;
    [Tooltip("Oyunun oynandığı masanın collider'ı. Sınırları dışına çıkan parçalar temizlenir.")]
    [SerializeField] private Collider tableSurface;

    [Header("Masa sınırı")]
    [Tooltip("Parça merkezinin masa kenarını geçmesine izin verilen küçük pay.")]
    [SerializeField] private float edgeMargin = 0.15f;
    [Tooltip("Parça merkezi masa üstünden bu kadar aşağıdaysa temizlenir.")]
    [SerializeField] private float dropDistance = 0.25f;

    [Header("Yıldız kuralı (harcanan mermiye göre)")]
    [Tooltip("Bu kadar veya daha az mermiyle kazanılırsa 3 yıldız.")]
    [SerializeField] private int threeStarMaxShots = 1;
    [Tooltip("Bu kadar veya daha az mermiyle kazanılırsa 2 yıldız. Daha fazlası 1 yıldız.")]
    [SerializeField] private int twoStarMaxShots = 2;

    [Header("Bitiş zamanlaması")]
    [Tooltip("Sonuç belli olduktan sonra bitiş ekranı açılmadan önceki bekleme (saniye) — son tuğlanın düşüşü ve juice görülsün.")]
    [SerializeField] private float endScreenDelay = 0.8f;

    [Header("Son mermi sonrası bekleme")]
    [Tooltip("Son mermi atıldıktan sonra en az bu kadar bekle — yavaş bir top hedefe henüz varmamış olabilir.")]
    [SerializeField] private float minSettleWait = 1f;
    [Tooltip("Tuğlalar ve top bu süre boyunca durgunsa 'her şey durdu' sayılır.")]
    [SerializeField] private float calmDuration = 0.8f;
    [Tooltip("Ne olursa olsun bu süre dolunca sonuç ilan edilir (bir şey sonsuza kadar yuvarlanıp kilitlemesin diye).")]
    [SerializeField] private float maxSettleWait = 8f;
    [Tooltip("Bu hızın altındaki hareket 'durgun' sayılır.")]
    [SerializeField] private float calmSpeed = 0.15f;

    private enum State { WaitingForLevel, Playing, Settling, Ended }

    private State state = State.WaitingForLevel;
    private int startingAmmo = -1;
    private int remainingAmmo;
    private float settleElapsed;
    private float calmTimer;

    private void Start()
    {
        if (launcher == null) launcher = FindFirstObjectByType<BallLauncher>();
        if (tableSurface == null)
            Debug.LogWarning("[LevelManager] Table Surface atanmamış; masa dışına çıkan parçalar otomatik temizlenemez.", this);
    }

    private void OnEnable()
    {
        LevelGenerator.OnLevelGenerated += HandleLevelGenerated;
        BallLauncher.OnAmmoChanged += HandleAmmoChanged;
        BallLauncher.OnAmmoDepleted += HandleAmmoDepleted;
    }

    private void OnDisable()
    {
        LevelGenerator.OnLevelGenerated -= HandleLevelGenerated;
        BallLauncher.OnAmmoChanged -= HandleAmmoChanged;
        BallLauncher.OnAmmoDepleted -= HandleAmmoDepleted;
    }

    private void HandleLevelGenerated()
    {
        StopAllCoroutines();
        state = State.WaitingForLevel;
        StartCoroutine(BeginAfterFrame());
    }

    private IEnumerator BeginAfterFrame()
    {
        // Yeni seviyeden önceki tuğlalar Destroy ile bir kare sonra silinir; kayıt temizlensin diye bekle.
        yield return null;

        if (ClearableBrick.CountRemaining() == 0)
        {
            Debug.LogWarning("[LevelManager] Üretilen seviyede hiç tuğla yok — seviye takibi başlatılmadı.");
            yield break;
        }

        state = State.Playing;
    }

    private void HandleAmmoChanged(int remaining)
    {
        if (startingAmmo < 0) startingAmmo = remaining;
        remainingAmmo = remaining;
    }

    private void HandleAmmoDepleted()
    {
        if (state != State.Playing) return;
        state = State.Settling;
        settleElapsed = 0f;
        calmTimer = 0f;
    }

    private void Update()
    {
        if (state == State.Playing || state == State.Settling) ClearOutsideTable();

        switch (state)
        {
            case State.Playing:
                if (ClearableBrick.CountRemaining() == 0) Finish(won: true);
                break;
            case State.Settling:
                UpdateSettling();
                break;
        }
    }

    private void UpdateSettling()
    {
        // Son atışın kendisi son tuğlayı da alabilir — bu hâlâ kazanmadır.
        if (ClearableBrick.CountRemaining() == 0)
        {
            Finish(won: true);
            return;
        }

        float dt = Time.unscaledDeltaTime;
        settleElapsed += dt;

        bool moving = ClearableBrick.AnyMoving(calmSpeed) || BallProjectile.AnyMoving(calmSpeed);
        calmTimer = moving ? 0f : calmTimer + dt;

        bool everythingStopped = settleElapsed >= minSettleWait && calmTimer >= calmDuration;
        if (everythingStopped || settleElapsed >= maxSettleWait) Finish(won: false);
    }

    private void Finish(bool won)
    {
        state = State.Ended;
        if (launcher != null) launcher.DisableInput();
        StartCoroutine(EndRoutine(won, won ? ComputeStars() : 0));
    }

    private IEnumerator EndRoutine(bool won, int stars)
    {
        yield return new WaitForSecondsRealtime(endScreenDelay);
        if (!won)
        {
            ClearOutsideTable();
            if (ClearableBrick.CountRemaining() == 0)
            {
                won = true;
                stars = ComputeStars();
            }
        }
        OnLevelEnded?.Invoke(won, stars);
    }

    private void ClearOutsideTable()
    {
        if (tableSurface == null || !tableSurface.enabled || !tableSurface.gameObject.activeInHierarchy) return;
        ClearableBrick.ClearOutsidePlayArea(tableSurface.bounds, edgeMargin, dropDistance);
    }

    private int ComputeStars()
    {
        int shotsUsed = startingAmmo - remainingAmmo;
        if (shotsUsed <= threeStarMaxShots) return 3;
        if (shotsUsed <= twoStarMaxShots) return 2;
        return 1;
    }
}
