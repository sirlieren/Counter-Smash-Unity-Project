using System.Collections;
using UnityEngine;

/// <summary>
/// Yalnızca en sert çarpmalarda çok kısa bir "freeze-frame" — Time.timeScale'i anlık düşürüp
/// geri toparlar. Ucuz ama etkisi büyük bir game feel hilesi.
/// </summary>
public class HitStop : MonoBehaviour
{
    [Header("Tetikleme")]
    [Tooltip("Bu şiddetin altındaki çarpışmalar donma tetiklemez — sadece en sert çarpmalar için.")]
    [SerializeField] private float minIntensityToTrigger = 0.6f;

    [Header("Donma")]
    [SerializeField] private float freezeDuration = 0.03f;
    [SerializeField] private float freezeTimeScale = 0.02f;

    [Header("Perfect çarpışma ağır çekimi")]
    [SerializeField] private float perfectSlowDuration = 0.22f;
    [SerializeField] private float perfectSlowTimeScale = 0.35f;
    [SerializeField] private float recoveryDuration = 0.12f;

    private Coroutine activeFreeze;
    private float restoreTimeScale = 1f;

    private void OnEnable()
    {
        BrickImpactSound.OnImpact += HandleImpact;
        PerfectBallExplosion.OnExploded += HandlePerfectExplosion;
    }

    private void OnDisable()
    {
        BrickImpactSound.OnImpact -= HandleImpact;
        PerfectBallExplosion.OnExploded -= HandlePerfectExplosion;
        if (activeFreeze != null)
        {
            StopCoroutine(activeFreeze);
            Time.timeScale = restoreTimeScale;
            activeFreeze = null;
        }
    }

    private void HandleImpact(Vector3 point, float intensity, BrickImpactSound source)
    {
        // Zaten donmuş haldeyken yeni bir tetikleme gelirse görmezden gel — büyük bir yıkımda
        // onlarca sert çarpma üst üste donmayı sürekli uzatıp oyunu kilitlemesin diye.
        if (intensity < minIntensityToTrigger || activeFreeze != null) return;
        restoreTimeScale = Time.timeScale;
        activeFreeze = StartCoroutine(FreezeRoutine(false));
    }

    private void HandlePerfectExplosion(Vector3 point)
    {
        if (activeFreeze != null) StopCoroutine(activeFreeze);
        else restoreTimeScale = Time.timeScale;
        activeFreeze = StartCoroutine(FreezeRoutine(true));
    }

    private IEnumerator FreezeRoutine(bool perfect)
    {
        Time.timeScale = freezeTimeScale;
        yield return new WaitForSecondsRealtime(freezeDuration);
        if (perfect)
        {
            Time.timeScale = perfectSlowTimeScale;
            yield return new WaitForSecondsRealtime(perfectSlowDuration);
            float elapsed = 0f;
            while (elapsed < recoveryDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                Time.timeScale = Mathf.Lerp(perfectSlowTimeScale, restoreTimeScale,
                    Mathf.Clamp01(elapsed / recoveryDuration));
                yield return null;
            }
        }
        Time.timeScale = restoreTimeScale;
        activeFreeze = null;
    }
}
