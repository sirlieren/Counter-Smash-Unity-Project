using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Seviye sonu paneli: başarı/başarısızlık başlığı, yıldızlar, tekrar dene / sonraki seviye butonları.
/// Bu bileşen HER ZAMAN AKTİF bir objede durmalı (örn. Canvas'ın kendisi) — panelin kendisinde değil,
/// çünkü panel kapalıyken olayı dinleyebilmesi gerekiyor.
/// </summary>
public class LevelEndUI : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Açılıp kapanacak panelin kökü. Üzerinde CanvasGroup varsa panel yumuşakça belirir.")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [Tooltip("İsteğe bağlı: 'Level 3' gibi seviye numarası yazısı.")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Button retryButton;
    [Tooltip("Sadece kazanınca görünür.")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button menuButton;

    [Header("Yıldızlar")]
    [Tooltip("Soldan sağa 3 BOŞ yıldız. Arka plandaki sabit yuvalar — panel açılınca hepsi boş görünür.")]
    [SerializeField] private RectTransform[] emptyStars;
    [Tooltip("Soldan sağa 3 DOLU yıldız. Her biri karşılık gelen boş yıldızın tam üstünde, olması gereken konumda ve boyutta durmalı. Başta gizlidirler; kazanılanlar sırayla belirir.")]
    [SerializeField] private RectTransform[] filledStars;

    [Header("Yıldız animasyonu")]
    [Tooltip("Panel açıldıktan sonra ilk yıldızın gelmesine kadar bekleme (saniye).")]
    [SerializeField] private float firstStarDelay = 0.25f;
    [Tooltip("Yıldızlar arası bekleme (saniye).")]
    [SerializeField] private float starInterval = 0.5f;
    [Tooltip("Yıldız belirdiği anda normal boyutunun kaç katı büyüklükte olsun.")]
    [SerializeField] private float starArrivalScale = 2.5f;
    [Tooltip("Büyük boyuttan normale dönüş süresi (saniye).")]
    [SerializeField] private float starSettleDuration = 0.45f;
    [Tooltip("Yerine oturma sırasındaki 'fazla küçülüp geri dönme' miktarı. 0 = hiç sekme yok, 1.7 = klasik, büyüdükçe daha sert sekme.")]
    [SerializeField] private float starOvershoot = 1.7f;

    [Header("Metinler")]
    [SerializeField] private string winTitle = "LEVEL CLEAR!";
    [SerializeField] private string loseTitle = "TRY AGAIN";
    [Tooltip("{0} yerine seviye numarası (1'den başlayarak) gelir.")]
    [SerializeField] private string levelFormat = "Level {0}";

    [Header("Panel animasyonu")]
    [SerializeField] private float panelPopDuration = 0.35f;

    [Header("Ses")]
    [Tooltip("Kazanılan i. yıldız geldiğinde i. ses çalar (giderek yükselen tonlar koyarsan pitch merdiveni olur).")]
    [SerializeField] private AudioClip[] starClips;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;
    [SerializeField] private float volume = 0.8f;

    private Vector3[] filledStarDefaultScales;
    private bool lastResultWon;

    private void Awake()
    {
        // Yıldızların sahnede ayarlanmış boyutu "olması gereken" boyut — animasyon oraya döner.
        filledStarDefaultScales = new Vector3[filledStars.Length];
        for (int i = 0; i < filledStars.Length; i++)
        {
            if (filledStars[i] == null) continue;
            filledStarDefaultScales[i] = filledStars[i].localScale;
        }

        if (panelRoot != null) panelRoot.SetActive(false);
        if (retryButton != null && retryButton != nextLevelButton) retryButton.onClick.AddListener(LevelSession.Retry);
        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(HandlePrimaryButton);
        if (menuButton != null) menuButton.onClick.AddListener(LevelSession.ReturnToMenu);
    }

    private void OnEnable()
    {
        LevelManager.OnLevelEnded += HandleLevelEnded;
    }

    private void OnDisable()
    {
        LevelManager.OnLevelEnded -= HandleLevelEnded;
    }

    private void HandleLevelEnded(bool won, int stars)
    {
        lastResultWon = won;
        if (won)
        {
            LevelSession.RecordWin(stars);
            LevelSession.CompleteCurrentLevel();
        }
        StartCoroutine(ShowRoutine(won, stars));
    }

    private void HandlePrimaryButton()
    {
        if (lastResultWon) LevelSession.NextLevel();
        else LevelSession.Retry();
    }

    private IEnumerator ShowRoutine(bool won, int stars)
    {
        if (titleText != null) titleText.text = won ? winTitle : loseTitle;
        if (levelText != null) levelText.text = string.Format(levelFormat, LevelSession.LevelIndex + 1);
        bool sharedButton = retryButton != null && retryButton == nextLevelButton;
        if (retryButton != null && !sharedButton) retryButton.gameObject.SetActive(!won);
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(won || sharedButton);
            TMP_Text buttonLabel = nextLevelButton.GetComponentInChildren<TMP_Text>(true);
            if (buttonLabel != null)
                buttonLabel.text = !won ? "TRY AGAIN" : LevelSession.IsLastLevel ? "LEVELS" : "NEXT LEVEL";
        }

        // Kazanınca 3 boş yuva görünür (hepsi boş), dolu yıldızlar gizli beklemede. Kaybedince yıldız satırı hiç görünmez.
        foreach (RectTransform empty in emptyStars)
        {
            if (empty != null) empty.gameObject.SetActive(won);
        }
        foreach (RectTransform filled in filledStars)
        {
            if (filled != null) filled.gameObject.SetActive(false);
        }

        PlayClip(won ? winClip : loseClip);

        yield return PopPanel();

        if (!won) yield break;

        yield return new WaitForSecondsRealtime(firstStarDelay);
        for (int i = 0; i < stars && i < filledStars.Length; i++)
        {
            if (i < starClips.Length) PlayClip(starClips[i]);
            if (filledStars[i] != null) StartCoroutine(StarArrival(i));
            yield return new WaitForSecondsRealtime(starInterval);
        }
    }

    private IEnumerator StarArrival(int index)
    {
        RectTransform star = filledStars[index];
        Vector3 defaultScale = filledStarDefaultScales[index];
        Vector3 startScale = defaultScale * starArrivalScale;

        star.localScale = startScale;
        star.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < starSettleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float eased = EaseOutBack(elapsed / starSettleDuration, starOvershoot);
            // LerpUnclamped: sekme sırasında eğri 1'i aşıp varış boyutunun ALTINA iner, sonra geri gelir.
            star.localScale = Vector3.LerpUnclamped(startScale, defaultScale, eased);
            yield return null;
        }

        star.localScale = defaultScale;
    }

    private IEnumerator PopPanel()
    {
        panelRoot.SetActive(true);

        CanvasGroup group = panelRoot.GetComponent<CanvasGroup>();
        Transform panel = panelRoot.transform;

        float elapsed = 0f;
        while (elapsed < panelPopDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / panelPopDuration);
            panel.localScale = Vector3.one * EaseOutBack(t, 1.70158f);
            if (group != null) group.alpha = t;
            yield return null;
        }

        panel.localScale = Vector3.one;
        if (group != null) group.alpha = 1f;
    }

    private void PlayClip(AudioClip clip)
    {
        if (AudioManager.Instance == null || clip == null) return;
        AudioManager.Instance.PlayOneShot(clip, Vector3.zero, volume, spatial: false);
    }

    private static float EaseOutBack(float t, float overshoot)
    {
        float c3 = overshoot + 1f;
        t = Mathf.Clamp01(t);
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + overshoot * Mathf.Pow(t - 1f, 2f);
    }
}
