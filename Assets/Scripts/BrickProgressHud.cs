using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>HUD üzerinde kalan tuğlaları ve kısa yıkım serilerini gösterir.</summary>
public class BrickProgressHud : MonoBehaviour
{
    [SerializeField] private TMP_Text remainingText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_FontAsset font;
    [Header("Otomatik etiket yerleşimi (üst orta noktaya göre)")]
    [SerializeField] private Vector2 remainingOffset = new Vector2(0f, -150f);
    [SerializeField] private Vector2 comboOffset = new Vector2(0f, -220f);
    [SerializeField] private float comboWindow = 0.8f;
    [SerializeField] private float comboVisibleDuration = 0.7f;
    [SerializeField] private float punchScale = 1.2f;
    [SerializeField] private float punchDuration = 0.2f;

    private int total;
    private int combo;
    private float lastClearTime = -100f;
    private Vector3 baseScale;
    private Coroutine punchRoutine;
    private Coroutine comboRoutine;

    private void Awake()
    {
        if (remainingText == null) remainingText = CreateLabel("BrickProgress", remainingOffset, 42f);
        if (comboText == null) comboText = CreateLabel("BrickCombo", comboOffset, 34f);
        if (remainingText != null) baseScale = remainingText.transform.localScale;
        if (comboText != null) comboText.gameObject.SetActive(false);
    }

    private TMP_Text CreateLabel(string labelName, Vector2 offset, float fontSize)
    {
        var labelObject = new GameObject(labelName, typeof(RectTransform));
        labelObject.transform.SetParent(transform, false);
        RectTransform rect = (RectTransform)labelObject.transform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = offset;
        rect.sizeDelta = new Vector2(600f, 70f);
        var label = labelObject.AddComponent<TextMeshProUGUI>();
        label.font = font != null ? font : TMP_Settings.defaultFontAsset;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    private void OnEnable()
    {
        LevelGenerator.OnLevelGenerated += HandleLevelGenerated;
        ClearableBrick.OnCleared += HandleCleared;
        LevelManager.OnLevelEnded += HandleLevelEnded;
    }

    private void OnDisable()
    {
        LevelGenerator.OnLevelGenerated -= HandleLevelGenerated;
        ClearableBrick.OnCleared -= HandleCleared;
        LevelManager.OnLevelEnded -= HandleLevelEnded;
    }

    private void HandleLevelGenerated()
    {
        StartCoroutine(InitializeAfterFrame());
    }

    private IEnumerator InitializeAfterFrame()
    {
        yield return null;
        total = ClearableBrick.CountRemaining();
        combo = 0;
        if (remainingText != null)
        {
            remainingText.gameObject.SetActive(total > 0);
            remainingText.text = $"BRICKS  {total}/{total}";
        }
        if (comboText != null) comboText.gameObject.SetActive(false);
    }

    private void HandleCleared(ClearableBrick brick)
    {
        if (total <= 0) return;
        int remaining = ClearableBrick.CountRemaining();
        if (remainingText != null)
        {
            remainingText.text = remaining == 0 ? "CLEAR!" : $"BRICKS  {remaining}/{total}";
            if (punchRoutine != null) StopCoroutine(punchRoutine);
            punchRoutine = StartCoroutine(Punch());
        }

        combo = Time.unscaledTime - lastClearTime <= comboWindow ? combo + 1 : 1;
        lastClearTime = Time.unscaledTime;
        if (comboText != null && combo >= 3 && remaining > 0)
        {
            comboText.text = $"x{combo} COMBO";
            comboText.gameObject.SetActive(true);
            if (comboRoutine != null) StopCoroutine(comboRoutine);
            comboRoutine = StartCoroutine(HideCombo());
        }
    }

    private IEnumerator Punch()
    {
        Transform target = remainingText.transform;
        float elapsed = 0f;
        while (elapsed < punchDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / punchDuration);
            target.localScale = baseScale * (1f + (punchScale - 1f) * Mathf.Sin(t * Mathf.PI));
            yield return null;
        }
        target.localScale = baseScale;
        punchRoutine = null;
    }

    private IEnumerator HideCombo()
    {
        yield return new WaitForSecondsRealtime(comboVisibleDuration);
        if (comboText != null) comboText.gameObject.SetActive(false);
        comboRoutine = null;
    }

    private void HandleLevelEnded(bool won, int stars)
    {
        if (remainingText != null) remainingText.gameObject.SetActive(false);
        if (comboText != null) comboText.gameObject.SetActive(false);
    }
}
