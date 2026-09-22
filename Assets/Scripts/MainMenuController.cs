using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private const string VolumeKey = "MasterVolume";

    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject levelSelectPanel;

    private RectTransform levelViewport;
    private RectTransform levelContent;
    private GridLayoutGroup levelGrid;
    private float lastViewportWidth;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplySavedVolume()
    {
        AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));
    }

    private void Awake()
    {
        if (playButton == null || settingsButton == null || mainMenuPanel == null || settingsPanel == null || levelSelectPanel == null)
        {
            Debug.LogError("[MainMenuController] Menu references are missing.", this);
            enabled = false;
            return;
        }

        BuildSettingsPanel();
        BuildLevelSelectPanel();
        settingsPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        playButton.onClick.AddListener(OpenLevelSelect);
        settingsButton.onClick.AddListener(OpenSettings);

        if (LevelSession.ConsumeOpenLevelSelectRequest()) OpenLevelSelect();
    }

    private void LateUpdate()
    {
        if (levelSelectPanel.activeInHierarchy) RefreshLevelGrid();
    }

    private void OnDestroy()
    {
        if (playButton != null) playButton.onClick.RemoveListener(OpenLevelSelect);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
    }

    private void OpenLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
        Canvas.ForceUpdateCanvases();
        RefreshLevelGrid();
    }

    private void CloseLevelSelect()
    {
        levelSelectPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
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

    private void BuildLevelSelectPanel()
    {
        RectTransform panel = levelSelectPanel.GetComponent<RectTransform>();
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = Vector2.zero;

        Image background = levelSelectPanel.GetComponent<Image>();
        if (background != null) background.color = new Color(0.18f, 0.12f, 0.21f, 0.98f);

        TMP_Text title = CreateText("Title", panel, "LEVEL SELECT", 76, new Vector2(0.5f, 0.88f));
        title.rectTransform.anchorMin = new Vector2(0.05f, 0.88f);
        title.rectTransform.anchorMax = new Vector2(0.95f, 0.88f);
        title.rectTransform.sizeDelta = new Vector2(0f, 120f);

        RectTransform scrollArea = CreateRect("Levels Scroll Area", panel, new Vector2(0.08f, 0.17f), new Vector2(0.92f, 0.79f), Vector2.zero);
        ScrollRect scroll = scrollArea.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 50f;

        levelViewport = CreateRect("Viewport", scrollArea, Vector2.zero, Vector2.one, Vector2.zero);
        levelViewport.gameObject.AddComponent<RectMask2D>();
        scroll.viewport = levelViewport;

        levelContent = CreateRect("Content", levelViewport, new Vector2(0f, 1f), Vector2.one, Vector2.zero);
        levelContent.pivot = new Vector2(0.5f, 1f);
        levelGrid = levelContent.gameObject.AddComponent<GridLayoutGroup>();
        levelGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        levelGrid.constraintCount = 2;
        levelGrid.spacing = new Vector2(22f, 22f);
        levelGrid.padding = new RectOffset(16, 16, 16, 16);
        levelGrid.childAlignment = TextAnchor.UpperCenter;
        scroll.content = levelContent;

        for (int index = 0; index < LevelSession.LevelCount; index++)
        {
            int selectedIndex = index;
            RectTransform card = CreateRect($"Level {index + 1:00}", levelContent, Vector2.zero, Vector2.zero, Vector2.zero);
            Image cardImage = card.gameObject.AddComponent<Image>();
            cardImage.color = index % 2 == 0
                ? new Color(0.86f, 0.78f, 0.91f)
                : new Color(0.78f, 0.87f, 0.91f);
            Button cardButton = card.gameObject.AddComponent<Button>();
            cardButton.onClick.AddListener(() => LevelSession.SelectLevel(selectedIndex));

            TMP_Text number = CreateText("Number", card, $"{index + 1:00}", 68, new Vector2(0.5f, 0.62f));
            number.color = new Color(0.25f, 0.16f, 0.29f);
            number.rectTransform.anchorMin = new Vector2(0.05f, 0.62f);
            number.rectTransform.anchorMax = new Vector2(0.95f, 0.62f);
            number.rectTransform.sizeDelta = new Vector2(0f, 100f);

            int best = LevelSession.BestStars(index);
            TMP_Text stars = CreateText("Best Stars", card, new string('★', best) + new string('☆', 3 - best), 38, new Vector2(0.5f, 0.28f));
            stars.color = new Color(0.43f, 0.29f, 0.23f);
            stars.rectTransform.anchorMin = new Vector2(0.05f, 0.28f);
            stars.rectTransform.anchorMax = new Vector2(0.95f, 0.28f);
            stars.rectTransform.sizeDelta = new Vector2(0f, 70f);
        }

        RectTransform backRect = CreateRect("Back", panel, new Vector2(0.3f, 0.085f), new Vector2(0.7f, 0.085f), new Vector2(0f, 105f));
        backRect.gameObject.AddComponent<Image>().color = new Color(0.86f, 0.79f, 0.94f);
        Button backButton = backRect.gameObject.AddComponent<Button>();
        backButton.onClick.AddListener(CloseLevelSelect);
        TMP_Text backText = CreateText("Back Label", backRect, "BACK", 46, new Vector2(0.5f, 0.5f));
        backText.color = new Color(0.25f, 0.16f, 0.29f);
        backText.rectTransform.anchorMin = Vector2.zero;
        backText.rectTransform.anchorMax = Vector2.one;
        backText.rectTransform.sizeDelta = Vector2.zero;
    }

    private void RefreshLevelGrid()
    {
        float width = levelViewport.rect.width;
        if (width < 1f || Mathf.Approximately(width, lastViewportWidth)) return;

        lastViewportWidth = width;
        float cardWidth = (width - levelGrid.padding.horizontal - levelGrid.spacing.x) / 2f;
        float cardHeight = Mathf.Clamp(cardWidth * 0.58f, 150f, 240f);
        levelGrid.cellSize = new Vector2(cardWidth, cardHeight);

        int rows = (LevelSession.LevelCount + 1) / 2;
        float contentHeight = levelGrid.padding.vertical + rows * cardHeight + (rows - 1) * levelGrid.spacing.y;
        levelContent.sizeDelta = new Vector2(0f, contentHeight);
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

    private static TextMeshProUGUI CreateText(string name, Transform parent, string value, float fontSize, Vector2 anchor)
    {
        RectTransform rect = CreateRect(name, parent, anchor, anchor, new Vector2(600f, 140f));
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = value;
        label.fontSize = fontSize;
        label.enableAutoSizing = true;
        label.fontSizeMin = 20f;
        label.fontSizeMax = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        return label;
    }
}
