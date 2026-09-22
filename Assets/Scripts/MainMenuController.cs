using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private const string VolumeKey = "MasterVolume";

    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplySavedVolume()
    {
        AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));
    }

    private void Awake()
    {
        if (playButton == null || settingsButton == null || mainMenuPanel == null || settingsPanel == null)
        {
            Debug.LogError("[MainMenuController] Menu references are missing.", this);
            enabled = false;
            return;
        }

        BuildSettingsPanel();
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        playButton.onClick.AddListener(Play);
        settingsButton.onClick.AddListener(OpenSettings);
    }

    private void OnDestroy()
    {
        if (playButton != null) playButton.onClick.RemoveListener(Play);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
    }

    private void Play()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Arena");
    }

    private void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    private void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private static void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }

    private void BuildSettingsPanel()
    {
        RectTransform panel = settingsPanel.GetComponent<RectTransform>();
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = Vector2.zero;

        Image panelImage = settingsPanel.GetComponent<Image>();
        if (panelImage != null) panelImage.color = new Color(0.18f, 0.12f, 0.21f, 0.98f);

        CreateText("Title", panel, "SETTINGS", 90, new Vector2(0.5f, 0.68f));
        CreateText("Volume Label", panel, "Volume", 54, new Vector2(0.5f, 0.54f));

        RectTransform sliderRect = CreateRect("Volume Slider", panel, new Vector2(0.2f, 0.45f), new Vector2(0.8f, 0.45f), new Vector2(0f, 38f));
        Image track = sliderRect.gameObject.AddComponent<Image>();
        track.color = new Color(0.35f, 0.31f, 0.38f);
        Slider slider = sliderRect.gameObject.AddComponent<Slider>();

        RectTransform fillRect = CreateRect("Fill", sliderRect, Vector2.zero, Vector2.one, Vector2.zero);
        fillRect.gameObject.AddComponent<Image>().color = new Color(0.96f, 0.73f, 0.38f);
        RectTransform handleRect = CreateRect("Handle", sliderRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(44f, 64f));
        handleRect.gameObject.AddComponent<Image>().color = Color.white;

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleRect.GetComponent<Image>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(AudioListener.volume);
        slider.onValueChanged.AddListener(SetVolume);

        RectTransform backRect = CreateRect("Back", panel, new Vector2(0.3f, 0.25f), new Vector2(0.7f, 0.25f), new Vector2(0f, 110f));
        backRect.gameObject.AddComponent<Image>().color = new Color(0.86f, 0.79f, 0.94f);
        Button backButton = backRect.gameObject.AddComponent<Button>();
        backButton.onClick.AddListener(CloseSettings);
        CreateText("Back Label", backRect, "BACK", 48, new Vector2(0.5f, 0.5f));
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)obj.transform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        return rect;
    }

    private static void CreateText(string name, Transform parent, string value, float fontSize, Vector2 anchor)
    {
        RectTransform rect = CreateRect(name, parent, anchor, anchor, new Vector2(600f, 140f));
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = value;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
    }
}
