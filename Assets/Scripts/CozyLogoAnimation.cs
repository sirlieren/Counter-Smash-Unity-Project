using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Logo altındaki TMP yazılarını sırayla yumuşakça gösterir ve hafifçe sallandırır.
/// Logo objesine eklenir; sahnedeki yazı konumlarını ve renklerini temel alır.
/// </summary>
public class CozyLogoAnimation : MonoBehaviour
{
    [Header("Giriş")]
    [SerializeField, Min(0.01f)] private float entranceDuration = 0.7f;
    [SerializeField, Min(0f)] private float lineDelay = 0.16f;
    [SerializeField, Min(0f)] private float riseDistance = 24f;
    [SerializeField, Range(0.1f, 1f)] private float entranceScale = 0.88f;

    [Header("Sakin döngü")]
    [SerializeField, Min(0.1f)] private float swayPeriod = 3.2f;
    [SerializeField, Min(0f)] private float swayDistance = 4f;
    [SerializeField, Range(0f, 0.1f)] private float scalePulse = 0.012f;

    private readonly List<Line> lines = new List<Line>();
    private float elapsed;

    private sealed class Line
    {
        public RectTransform Rect;
        public TMP_Text Text;
        public Vector2 Position;
        public Vector3 Scale;
        public Color Color;
    }

    private void OnEnable()
    {
        lines.Clear();
        elapsed = 0f;

        // Yalnızca Logo'nun doğrudan altındaki yazıları al; ileride eklenen süsleri etkileme.
        foreach (Transform child in transform)
        {
            if (child is not RectTransform rect) continue;
            if (!child.TryGetComponent(out TMP_Text label)) continue;

            lines.Add(new Line
            {
                Rect = rect,
                Text = label,
                Position = rect.anchoredPosition,
                Scale = rect.localScale,
                Color = label.color
            });
        }

        ApplyAnimation();
    }

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        ApplyAnimation();
    }

    private void OnDisable()
    {
        // Düzenleme modundaki yerleşim ve renkler her zaman asıl halleriyle kalır.
        foreach (Line line in lines)
        {
            if (line.Rect != null)
            {
                line.Rect.anchoredPosition = line.Position;
                line.Rect.localScale = line.Scale;
            }

            if (line.Text != null) line.Text.color = line.Color;
        }
    }

    private void ApplyAnimation()
    {
        float duration = Mathf.Max(0.01f, entranceDuration);
        float period = Mathf.Max(0.1f, swayPeriod);

        for (int i = 0; i < lines.Count; i++)
        {
            Line line = lines[i];
            if (line.Rect == null || line.Text == null) continue;

            float localTime = elapsed - i * lineDelay;
            float progress = Mathf.Clamp01(localTime / duration);
            float eased = EaseOutBack(progress);
            float opacity = Mathf.SmoothStep(0f, 1f, progress);

            float sway = 0f;
            float pulse = 0f;
            if (localTime > duration)
            {
                float phase = (localTime - duration) * Mathf.PI * 2f / period;
                sway = Mathf.Sin(phase) * swayDistance;
                pulse = Mathf.Sin(phase) * scalePulse;
            }

            line.Rect.anchoredPosition = line.Position + Vector2.up * (riseDistance * (eased - 1f) + sway);
            line.Rect.localScale = line.Scale * (Mathf.LerpUnclamped(entranceScale, 1f, eased) + pulse);

            Color color = line.Color;
            color.a *= opacity;
            line.Text.color = color;
        }
    }

    private static float EaseOutBack(float t)
    {
        const float overshoot = 1.2f;
        float c = overshoot + 1f;
        return 1f + c * Mathf.Pow(t - 1f, 3f) + overshoot * Mathf.Pow(t - 1f, 2f);
    }
}
