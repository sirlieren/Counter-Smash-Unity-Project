using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoHud : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("İkonların dizileceği kapsayıcı. Icon'ların otomatik yan yana dizilmesi için üstüne bir Horizontal Layout Group ekle.")]
    [SerializeField] private RectTransform iconContainer;
    [SerializeField] private GameObject ammoIconPrefab;

    [Header("Juice")]
    [Tooltip("Bir mermi harcandığında ikonun küçülüp kaybolma süresi.")]
    [SerializeField] private float popDuration = 0.15f;
    [Tooltip("Mermi harcanınca çerçevenin yaptığı küçük sıçrama büyüklüğü (1 = sıçramasız).")]
    [SerializeField] private float consumePunchScale = 1.15f;
    [Tooltip("Seviye başında ikonların tek tek belirme süresi.")]
    [SerializeField] private float appearDuration = 0.2f;
    [Tooltip("İkonlar arasındaki belirme gecikmesi — kademeli/cascade bir açılış için.")]
    [SerializeField] private float appearStagger = 0.04f;

    private readonly List<GameObject> icons = new List<GameObject>();
    private int nextIconToConsume;
    private bool initialized;

    private void OnEnable()
    {
        initialized = false;
        BallLauncher.OnAmmoChanged += HandleAmmoChanged;
    }

    private void OnDisable()
    {
        BallLauncher.OnAmmoChanged -= HandleAmmoChanged;
    }

    private void HandleAmmoChanged(int remaining)
    {
        if (!initialized)
        {
            initialized = true;
            BuildIcons(remaining);
        }
        else
        {
            ConsumeOneIcon();
        }
    }

    private void BuildIcons(int count)
    {
        foreach (GameObject icon in icons) Destroy(icon);
        icons.Clear();

        for (int i = 0; i < count; i++)
        {
            GameObject icon = Instantiate(ammoIconPrefab, iconContainer);
            icon.transform.localScale = Vector3.zero;
            icons.Add(icon);
            StartCoroutine(AppearAnimation(icon.transform, i * appearStagger));
        }

        nextIconToConsume = icons.Count - 1;
    }

    private void ConsumeOneIcon()
    {
        if (nextIconToConsume < 0) return;

        GameObject icon = icons[nextIconToConsume];
        nextIconToConsume--;

        // BallTab çerçevesinin tek çocuğu top ikonu — mermi harcanınca çerçeve kalır, sadece bu gizlenir.
        Transform ballIcon = icon.transform.GetChild(0);
        StartCoroutine(ConsumeAnimation(icon.transform, ballIcon));
    }

    private IEnumerator AppearAnimation(Transform iconTransform, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        while (elapsed < appearDuration)
        {
            elapsed += Time.deltaTime;
            iconTransform.localScale = Vector3.one * EaseOutBack(elapsed / appearDuration);
            yield return null;
        }

        iconTransform.localScale = Vector3.one;
    }

    private IEnumerator ConsumeAnimation(Transform frame, Transform ballIcon)
    {
        Image ballImage = ballIcon.GetComponent<Image>();
        Color startColor = ballImage != null ? ballImage.color : Color.white;
        Vector3 ballStartScale = ballIcon.localScale;
        Vector3 frameBaseScale = frame.localScale;

        float elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;

            ballIcon.localScale = Vector3.Lerp(ballStartScale, Vector3.zero, t);
            if (ballImage != null) ballImage.color = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0f), t);

            float punch = 1f + (consumePunchScale - 1f) * Mathf.Sin(t * Mathf.PI);
            frame.localScale = frameBaseScale * punch;

            yield return null;
        }

        ballIcon.gameObject.SetActive(false);
        frame.localScale = frameBaseScale;
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        t = Mathf.Clamp01(t);
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
