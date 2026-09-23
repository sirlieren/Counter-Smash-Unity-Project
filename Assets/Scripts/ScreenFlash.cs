using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Perfect atışın çıkışında hafif, tuğla patlamasında güçlü bir ekran parlaması gösterir.
/// Sahnede hazır Canvas gerekmez — kendi overlay Canvas'ını ve tam ekran Image'ını çalışma anında kurar.
/// Herhangi bir objeye eklenebilir.
/// </summary>
public class ScreenFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = Color.white;
    [Tooltip("Flaşın ilk andaki en yüksek opaklığı (0-1). 1 = ekran tamamen beyaz; küçük tutmak hem daha zarif hem gözü daha az yorar.")]
    [SerializeField, Range(0f, 1f)] private float peakAlpha = 0.6f;
    [SerializeField, Range(0f, 1f)] private float launchPeakMultiplier = 0.2f;
    [Tooltip("Flaşın sönme süresi (saniye).")]
    [SerializeField] private float fadeDuration = 0.18f;
    [Tooltip("Sönme eğrisinin sertliği. 1 = doğrusal, büyüdükçe ilk anda çok parlak, sonra hızla sönen 'çakma' gibi bir his verir.")]
    [SerializeField, Range(1f, 4f)] private float fadeSharpness = 2f;
    [Tooltip("Flaş Canvas'ının çizim sırası — yüksek değer diğer UI'ların üstünde çıkar.")]
    [SerializeField] private int sortingOrder = 100;

    private Image flashImage;
    private float elapsed;
    private bool flashing;
    private float currentPeakAlpha;

    private void Awake()
    {
        BuildOverlay();
    }

    private void OnEnable()
    {
        BallLauncher.OnPerfectShot += HandlePerfectShot;
        PerfectBallExplosion.OnExploded += HandleExplosion;
    }

    private void OnDisable()
    {
        BallLauncher.OnPerfectShot -= HandlePerfectShot;
        PerfectBallExplosion.OnExploded -= HandleExplosion;
        flashing = false;
        if (flashImage != null) flashImage.enabled = false;
    }

    private void BuildOverlay()
    {
        var canvasObject = new GameObject("ScreenFlashCanvas");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        var imageObject = new GameObject("Flash");
        imageObject.transform.SetParent(canvasObject.transform, false);

        flashImage = imageObject.AddComponent<Image>();
        flashImage.raycastTarget = false; // Flaş dokunmaları yutmasın, atış girdisi engellenmesin.
        flashImage.enabled = false;

        RectTransform rect = flashImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void HandlePerfectShot()
    {
        BeginFlash(peakAlpha * launchPeakMultiplier);
    }

    private void HandleExplosion(Vector3 point)
    {
        BeginFlash(peakAlpha);
    }

    private void BeginFlash(float alpha)
    {
        elapsed = 0f;
        flashing = true;
        currentPeakAlpha = alpha;
        flashImage.enabled = true;
        SetAlpha(currentPeakAlpha);
    }

    private void Update()
    {
        if (!flashing) return;

        // Unscaled: hit-stop / slow-motion sırasında flaş takılıp kalmasın.
        elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(elapsed / fadeDuration);

        if (t >= 1f)
        {
            flashing = false;
            flashImage.enabled = false;
            return;
        }

        SetAlpha(currentPeakAlpha * Mathf.Pow(1f - t, fadeSharpness));
    }

    private void SetAlpha(float alpha)
    {
        Color color = flashColor;
        color.a = alpha;
        flashImage.color = color;
    }
}
