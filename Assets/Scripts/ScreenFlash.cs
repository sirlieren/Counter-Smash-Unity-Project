using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Perfect atış çıktığı anda ekranı kısa süreliğine renkle (varsayılan beyaz) parlatıp söndürür.
/// Sahnede hazır Canvas gerekmez — kendi overlay Canvas'ını ve tam ekran Image'ını çalışma anında kurar.
/// Herhangi bir objeye eklenebilir.
/// </summary>
public class ScreenFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = Color.white;
    [Tooltip("Flaşın ilk andaki en yüksek opaklığı (0-1). 1 = ekran tamamen beyaz; küçük tutmak hem daha zarif hem gözü daha az yorar.")]
    [SerializeField, Range(0f, 1f)] private float peakAlpha = 0.6f;
    [Tooltip("Flaşın sönme süresi (saniye).")]
    [SerializeField] private float fadeDuration = 0.18f;
    [Tooltip("Sönme eğrisinin sertliği. 1 = doğrusal, büyüdükçe ilk anda çok parlak, sonra hızla sönen 'çakma' gibi bir his verir.")]
    [SerializeField, Range(1f, 4f)] private float fadeSharpness = 2f;
    [Tooltip("Flaş Canvas'ının çizim sırası — yüksek değer diğer UI'ların üstünde çıkar.")]
    [SerializeField] private int sortingOrder = 100;

    private Image flashImage;
    private float elapsed;
    private bool flashing;

    private void Awake()
    {
        BuildOverlay();
    }

    private void OnEnable()
    {
        BallLauncher.OnPerfectShot += HandlePerfectShot;
    }

    private void OnDisable()
    {
        BallLauncher.OnPerfectShot -= HandlePerfectShot;
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
        elapsed = 0f;
        flashing = true;
        flashImage.enabled = true;
        SetAlpha(peakAlpha);
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

        SetAlpha(peakAlpha * Mathf.Pow(1f - t, fadeSharpness));
    }

    private void SetAlpha(float alpha)
    {
        Color color = flashColor;
        color.a = alpha;
        flashImage.color = color;
    }
}
