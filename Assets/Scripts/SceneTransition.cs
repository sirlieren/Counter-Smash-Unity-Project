using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Sahne değişirken kısa bir perdeyle eski ve yeni sahneyi yumuşakça bağlar.</summary>
public class SceneTransition : MonoBehaviour
{
    private const float FadeDuration = 0.35f;
    private static SceneTransition instance;

    private CanvasGroup curtain;

    public static bool Load(string sceneName)
    {
        if (instance != null) return false;

        GameObject root = new GameObject("Scene Transition", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        SceneTransition transition = root.AddComponent<SceneTransition>();
        transition.CreateCurtain();
        transition.StartCoroutine(transition.LoadRoutine(sceneName));
        return true;
    }

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void CreateCurtain()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = short.MaxValue;

        GameObject overlay = new GameObject("Fade Curtain", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        overlay.transform.SetParent(transform, false);
        RectTransform rect = (RectTransform)overlay.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = overlay.GetComponent<Image>();
        image.color = new Color(0.16f, 0.12f, 0.21f, 1f);
        image.raycastTarget = true;

        curtain = overlay.GetComponent<CanvasGroup>();
        curtain.alpha = 0f;
        curtain.blocksRaycasts = true;
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        yield return Fade(0f, 1f);

        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneName);
        if (loading != null)
        {
            while (!loading.isDone) yield return null;
            // Yeni sahnedeki Canvas ve kamera ilk karede yerleşsin.
            yield return null;
        }
        else
        {
            Debug.LogError($"[SceneTransition] Could not load scene: {sceneName}");
        }

        yield return Fade(1f, 0f);
        curtain.blocksRaycasts = false;
        Destroy(gameObject);
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < FadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / FadeDuration));
            curtain.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        curtain.alpha = to;
    }
}
