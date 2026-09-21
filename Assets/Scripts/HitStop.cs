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

    private Coroutine activeFreeze;

    private void OnEnable()
    {
        BrickImpactSound.OnImpact += HandleImpact;
    }

    private void OnDisable()
    {
        BrickImpactSound.OnImpact -= HandleImpact;
        if (activeFreeze != null) Time.timeScale = 1f;
    }

    private void HandleImpact(Vector3 point, float intensity, BrickImpactSound source)
    {
        // Zaten donmuş haldeyken yeni bir tetikleme gelirse görmezden gel — büyük bir yıkımda
        // onlarca sert çarpma üst üste donmayı sürekli uzatıp oyunu kilitlemesin diye.
        if (intensity < minIntensityToTrigger || activeFreeze != null) return;
        activeFreeze = StartCoroutine(FreezeRoutine());
    }

    private IEnumerator FreezeRoutine()
    {
        Time.timeScale = freezeTimeScale;
        yield return new WaitForSecondsRealtime(freezeDuration);
        // Şu an zamanı değiştiren tek sistem bu — ileride "perfect-shot slow-motion" eklenince
        // burada sabit 1f yerine slow-motion'ın kendi hedef değerine dönmemiz gerekecek.
        Time.timeScale = 1f;
        activeFreeze = null;
    }
}
