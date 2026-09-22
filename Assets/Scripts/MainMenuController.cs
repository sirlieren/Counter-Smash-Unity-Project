using System.Collections;
using System;
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
    [SerializeField] private Button levelBackButton;
    [SerializeField] private Button[] levelButtons;

    [Header("Seviye yıldızları")]
    [SerializeField] private Sprite blankStarSprite;
    [SerializeField] private Sprite filledStarSprite;

    [Header("Menü geçişi")]
    [SerializeField, Range(0.05f, 1f)] private float transitionDuration = 0.3f;
    [SerializeField, Range(0f, 0.1f)] private float entranceScaleOffset = 0.025f;

    private PanelState mainPanelState;
    private PanelState settingsPanelState;
    private PanelState levelPanelState;
    private PanelState activePanel;
    private Coroutine transitionRoutine;
    private CanvasGroup[] levelCardGroups;
    private float[] levelCardBaseAlphas;
    private Image[,] levelStars;
    private TMP_Text resetProgressLabel;

    private sealed class PanelState
    {
        public GameObject Root;
        public CanvasGroup Group;
        public Vector3 Scale;
        public float Alpha;
    }

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
        BindLevelButtons();
        RefreshLevelLocks();
        RefreshLevelStars();

        mainPanelState = PreparePanel(mainMenuPanel);
        settingsPanelState = PreparePanel(settingsPanel);
        levelPanelState = PreparePanel(levelSelectPanel);
        activePanel = LevelSession.ConsumeOpenLevelSelectRequest() ? levelPanelState : mainPanelState;
        SetInitialPanel(mainPanelState);
        SetInitialPanel(settingsPanelState);
        SetInitialPanel(levelPanelState);

        playButton.onClick.AddListener(OpenLevelSelect);
        settingsButton.onClick.AddListener(OpenSettings);
    }

    private void OnDestroy()
    {
        if (playButton != null) playButton.onClick.RemoveListener(OpenLevelSelect);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
        if (levelBackButton != null) levelBackButton.onClick.RemoveListener(CloseLevelSelect);
    }

    private void OpenLevelSelect()
    {
        RefreshLevelLocks();
        RefreshLevelStars();
        SwitchTo(levelPanelState);
    }

    private void CloseLevelSelect()
    {
        SwitchTo(mainPanelState);
    }

    private void OpenSettings()
    {
        if (resetProgressLabel != null) resetProgressLabel.text = "RESET PROGRESS";
        SwitchTo(settingsPanelState);
    }

    private void CloseSettings()
    {
        SwitchTo(mainPanelState);
    }

    private static PanelState PreparePanel(GameObject root)
    {
        CanvasGroup group = root.GetComponent<CanvasGroup>();
        if (group == null) group = root.AddComponent<CanvasGroup>();
        return new PanelState
        {
            Root = root,
            Group = group,
            Scale = root.transform.localScale,
            Alpha = group.alpha
        };
    }

    private void SetInitialPanel(PanelState panel)
    {
        bool visible = panel == activePanel;
        panel.Root.SetActive(visible);
        panel.Root.transform.localScale = panel.Scale;
        panel.Group.alpha = panel.Alpha;
        panel.Group.interactable = visible;
        panel.Group.blocksRaycasts = visible;
    }

    private void SwitchTo(PanelState target)
    {
        if (target == null || target == activePanel || transitionRoutine != null) return;
        transitionRoutine = StartCoroutine(Transition(activePanel, target));
    }

    private IEnumerator Transition(PanelState from, PanelState to)
    {
        to.Root.SetActive(true);
        from.Group.interactable = false;
        from.Group.blocksRaycasts = false;
        to.Group.interactable = false;
        to.Group.blocksRaycasts = false;
        to.Group.alpha = 0f;
        to.Root.transform.localScale = to.Scale * (1f - entranceScaleOffset);

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionDuration));
            from.Group.alpha = from.Alpha * (1f - t);
            from.Root.transform.localScale = from.Scale * (1f - entranceScaleOffset * 0.5f * t);
            to.Group.alpha = to.Alpha * t;
            to.Root.transform.localScale = to.Scale * (1f - entranceScaleOffset * (1f - t));
            yield return null;
        }

        from.Root.SetActive(false);
        from.Root.transform.localScale = from.Scale;
        from.Group.alpha = from.Alpha;
        to.Root.transform.localScale = to.Scale;
        to.Group.alpha = to.Alpha;
        to.Group.interactable = true;
        to.Group.blocksRaycasts = true;
        activePanel = to;
        transitionRoutine = null;
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

        RectTransform resetRect = CreateRect("Debug Reset Progress", panel, new Vector2(0.2f, 0.11f), new Vector2(0.8f, 0.11f), new Vector2(0f, 95f));
        resetRect.gameObject.AddComponent<Image>().color = new Color(0.86f, 0.55f, 0.56f);
        Button resetButton = resetRect.gameObject.AddComponent<Button>();
        resetButton.onClick.AddListener(ResetProgressFromMenu);
        resetProgressLabel = CreateText("Reset Label", resetRect, "RESET PROGRESS", 38, new Vector2(0.5f, 0.5f));
        resetProgressLabel.rectTransform.anchorMin = Vector2.zero;
        resetProgressLabel.rectTransform.anchorMax = Vector2.one;
        resetProgressLabel.rectTransform.sizeDelta = Vector2.zero;
        resetProgressLabel.color = new Color(0.25f, 0.16f, 0.29f);
    }

    private void ResetProgressFromMenu()
    {
        LevelSession.ResetProgress();
        RefreshLevelLocks();
        RefreshLevelStars();
        if (resetProgressLabel != null) resetProgressLabel.text = "PROGRESS RESET";
    }

    private void BindLevelButtons()
    {
        if (levelBackButton != null) levelBackButton.onClick.AddListener(CloseLevelSelect);

        if (levelButtons == null || levelButtons.Length != LevelSession.LevelCount)
        {
            Debug.LogError($"[MainMenuController] Assign {LevelSession.LevelCount} level buttons in order.", this);
            return;
        }

        levelCardGroups = new CanvasGroup[LevelSession.LevelCount];
        levelCardBaseAlphas = new float[LevelSession.LevelCount];
        levelStars = new Image[LevelSession.LevelCount, 3];

        int starGroupsFound = 0;

        for (int index = 0; index < levelButtons.Length; index++)
        {
            Button button = levelButtons[index];
            if (button == null)
            {
                Debug.LogError($"[MainMenuController] Level {index + 1} button is missing.", this);
                continue;
            }

            int selectedIndex = index;
            button.onClick.AddListener(() => LevelSession.SelectLevel(selectedIndex));
            CanvasGroup cardGroup = button.GetComponent<CanvasGroup>();
            if (cardGroup == null) cardGroup = button.gameObject.AddComponent<CanvasGroup>();
            levelCardGroups[index] = cardGroup;
            levelCardBaseAlphas[index] = cardGroup.alpha;
            if (CacheLevelStars(index, button.transform)) starGroupsFound++;
        }

        if (starGroupsFound < LevelSession.LevelCount)
            Debug.LogWarning($"[MainMenuController] Found Stars on {starGroupsFound}/{LevelSession.LevelCount} level cards. Expected Stars > star1, star2, star3.", this);
    }

    private bool CacheLevelStars(int levelIndex, Transform card)
    {
        Transform starsRoot = null;
        foreach (Transform child in card)
        {
            if (!string.Equals(child.name, "Stars", StringComparison.OrdinalIgnoreCase)) continue;
            starsRoot = child;
            break;
        }

        if (starsRoot == null) return false;

        foreach (Transform child in starsRoot)
        {
            string name = child.name;
            if (name.Length != 5 || !name.StartsWith("star", StringComparison.OrdinalIgnoreCase)) continue;
            int slot = name[4] - '1';
            if (slot < 0 || slot >= 3) continue;
            levelStars[levelIndex, slot] = child.GetComponent<Image>();
        }

        return levelStars[levelIndex, 0] != null &&
               levelStars[levelIndex, 1] != null &&
               levelStars[levelIndex, 2] != null;
    }

    private void RefreshLevelStars()
    {
        if (levelStars == null) return;

        for (int level = 0; level < LevelSession.LevelCount; level++)
        {
            int earned = LevelSession.BestStars(level);
            for (int slot = 0; slot < 3; slot++)
            {
                Image star = levelStars[level, slot];
                if (star == null) continue;
                Sprite sprite = slot < earned ? filledStarSprite : blankStarSprite;
                if (sprite != null) star.sprite = sprite;
            }
        }
    }

    private void RefreshLevelLocks()
    {
        if (levelCardGroups == null) return;
        int unlockedCount = LevelSession.UnlockedLevelCount;
        for (int index = 0; index < levelCardGroups.Length; index++)
        {
            Button button = levelButtons[index];
            if (button == null) continue;
            bool unlocked = index < unlockedCount;
            button.interactable = unlocked;
            if (levelCardGroups[index] != null)
                levelCardGroups[index].alpha = levelCardBaseAlphas[index] * (unlocked ? 1f : 0.45f);
        }
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
