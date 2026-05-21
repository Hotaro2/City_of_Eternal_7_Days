using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VN
{
    public sealed class VNOptionPanel : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private VNUIController ui;
        [SerializeField] private VNPresenter presenter;

        [Header("Panels")]
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private GameObject settingsRoot;

        [Header("Audio Controls")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Graphic Controls")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;

        [Header("Gameplay Controls")]
        [SerializeField] private Slider textSpeedSlider;
        [SerializeField] private Slider autoWaitSlider;

        [Header("Footer Buttons")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button closeButton;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button saveMenuButton;
        [SerializeField] private Button loadMenuButton;
        [SerializeField] private Button openSettingsButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;

        private VNSettingsData settings;
        private readonly List<Resolution> resolutions = new();
        private VNTitleManager titleManager;

        private void EnsureSettings()
        {
            if (settings == null) settings = VNSettingsData.Load();
        }

        private void RefreshRuntimeBindings()
        {
            if (ui == null) ui = Object.FindFirstObjectByType<VNUIController>(FindObjectsInactive.Include);
            if (presenter == null) presenter = Object.FindFirstObjectByType<VNPresenter>(FindObjectsInactive.Include);
            if (titleManager == null) titleManager = Object.FindFirstObjectByType<VNTitleManager>(FindObjectsInactive.Include);
        }

        private void Awake()
        {
            EnsureSettings();
            RefreshRuntimeBindings();
            EnsureResolutionDropdownTemplate();
            SetupGraphicsData();

            saveMenuButton?.onClick.AddListener(() => OpenSaveLoad(VNSaveLoadPanel.PanelMode.Save));
            loadMenuButton?.onClick.AddListener(() => OpenSaveLoad(VNSaveLoadPanel.PanelMode.Load));
            openSettingsButton?.onClick.AddListener(ShowSettings);
            resumeButton?.onClick.AddListener(Close);
            quitButton?.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            applyButton?.onClick.AddListener(ApplyAndSave);
            closeButton?.onClick.AddListener(HandleCloseButton);

            ApplySettingsToSystems();
        }

        private void HandleCloseButton()
        {
            if (menuRoot != null) ShowMainMenu();
            else Close();
        }

        private void SetupGraphicsData()
        {
            if (resolutionDropdown == null) return;

            resolutionDropdown.ClearOptions();
            resolutions.Clear();

            Resolution[] allResolutions = Screen.resolutions;
            List<string> options = new();
            int currentResIndex = 0;
            int targetWidth = settings != null && settings.hasDisplaySettings ? settings.resolutionWidth : Screen.width;
            int targetHeight = settings != null && settings.hasDisplaySettings ? settings.resolutionHeight : Screen.height;

            for (int i = 0; i < allResolutions.Length; i++)
            {
                string option = allResolutions[i].width + " x " + allResolutions[i].height;
                if (options.Contains(option)) continue;

                options.Add(option);
                resolutions.Add(allResolutions[i]);

                if (allResolutions[i].width == targetWidth && allResolutions[i].height == targetHeight)
                    currentResIndex = options.Count - 1;
            }

            if (options.Count == 0)
            {
                options.Add(Screen.width + " x " + Screen.height);
                resolutions.Add(new Resolution { width = Screen.width, height = Screen.height });
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResIndex;
            resolutionDropdown.RefreshShownValue();

            if (fullscreenToggle != null)
                fullscreenToggle.isOn = settings != null && settings.hasDisplaySettings ? settings.fullscreen : Screen.fullScreen;
        }

        public void Open()
        {
            EnsureSettings();
            RefreshRuntimeBindings();

            if (menuRoot != null) ShowMainMenu();
            else ShowSettings();
        }

        public void Close()
        {
            if (settings != null) settings.Save();

            gameObject.SetActive(false);
            Time.timeScale = 1f;
        }

        public void ShowMainMenu()
        {
            if (menuRoot == null) return;

            gameObject.SetActive(true);
            menuRoot.SetActive(true);
            if (settingsRoot != null) settingsRoot.SetActive(false);
            Time.timeScale = 0f;
        }

        public void ShowSettings()
        {
            RefreshRuntimeBindings();

            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            if (menuRoot != null) menuRoot.SetActive(false);
            if (settingsRoot != null) settingsRoot.SetActive(true);

            EnsureSettings();
            UpdateUIFromData();
            Time.timeScale = 0f;
        }

        private void UpdateUIFromData()
        {
            EnsureSettings();

            if (masterSlider != null) masterSlider.value = settings.masterVolume;
            if (bgmSlider != null) bgmSlider.value = settings.bgmVolume;
            if (sfxSlider != null) sfxSlider.value = settings.sfxVolume;
            if (textSpeedSlider != null) textSpeedSlider.value = settings.textSpeed;
            if (autoWaitSlider != null) autoWaitSlider.value = settings.autoWaitMultiplier - 0.5f;
            if (resolutionDropdown != null) SelectResolution(settings.resolutionWidth, settings.resolutionHeight);
            if (fullscreenToggle != null)
                fullscreenToggle.isOn = settings.hasDisplaySettings ? settings.fullscreen : Screen.fullScreen;
        }

        private void ApplyAndSave()
        {
            EnsureSettings();

            if (masterSlider != null) settings.masterVolume = masterSlider.value;
            if (bgmSlider != null) settings.bgmVolume = bgmSlider.value;
            if (sfxSlider != null) settings.sfxVolume = sfxSlider.value;
            if (textSpeedSlider != null) settings.textSpeed = textSpeedSlider.value;
            if (autoWaitSlider != null) settings.autoWaitMultiplier = autoWaitSlider.value + 0.5f;

            if (resolutionDropdown != null && resolutions.Count > resolutionDropdown.value)
            {
                Resolution resolution = resolutions[resolutionDropdown.value];
                settings.resolutionWidth = resolution.width;
                settings.resolutionHeight = resolution.height;
                settings.fullscreen = fullscreenToggle != null && fullscreenToggle.isOn;
                settings.hasDisplaySettings = true;
            }
            else if (fullscreenToggle != null)
            {
                settings.resolutionWidth = Screen.width;
                settings.resolutionHeight = Screen.height;
                settings.fullscreen = fullscreenToggle.isOn;
                settings.hasDisplaySettings = true;
            }

            ApplySettingsToSystems();
            settings.Save();

            if (menuRoot != null) ShowMainMenu();
            else Close();
        }

        private void ApplySettingsToSystems()
        {
            RefreshRuntimeBindings();
            if (settings == null) return;

            if (titleManager != null) titleManager.SetVolume(settings.masterVolume, settings.bgmVolume, settings.sfxVolume);
            else presenter?.SetVolume(settings.masterVolume, settings.bgmVolume, settings.sfxVolume);

            ApplyDisplaySettings();

            float secondsPerChar = Mathf.Lerp(0.1f, 0.0f, settings.textSpeed);
            ui?.GetComponentInChildren<VNTextTyper>()?.SetSpeed(secondsPerChar);
            ui?.SetAutoWaitMultiplier(settings.autoWaitMultiplier);
        }

        private void ApplyDisplaySettings()
        {
            settings?.ApplyDisplaySettings();
        }

        private void SelectResolution(int width, int height)
        {
            if (resolutionDropdown == null || resolutions.Count == 0) return;
            if (width <= 0 || height <= 0) return;

            for (int i = 0; i < resolutions.Count; i++)
            {
                if (resolutions[i].width != width || resolutions[i].height != height) continue;

                resolutionDropdown.SetValueWithoutNotify(i);
                resolutionDropdown.RefreshShownValue();
                return;
            }
        }

        private void EnsureResolutionDropdownTemplate()
        {
            if (resolutionDropdown == null || resolutionDropdown.template != null) return;

            TMP_FontAsset font = resolutionDropdown.captionText != null ? resolutionDropdown.captionText.font : null;
            RectTransform dropdownRect = resolutionDropdown.GetComponent<RectTransform>();
            float width = dropdownRect != null && dropdownRect.rect.width > 0f ? dropdownRect.rect.width : 220f;

            GameObject template = CreateDropdownObject("Template", resolutionDropdown.transform, typeof(Image), typeof(ScrollRect), typeof(CanvasGroup));
            RectTransform templateRect = template.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0f, 0f);
            templateRect.anchorMax = new Vector2(1f, 0f);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.anchoredPosition = new Vector2(0f, -2f);
            templateRect.sizeDelta = new Vector2(0f, 180f);
            template.GetComponent<Image>().color = Color.white;

            GameObject viewport = CreateDropdownObject("Viewport", template.transform, typeof(Image), typeof(Mask));
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = CreateDropdownObject("Content", viewport.transform, typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 28f);

            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject item = CreateDropdownObject("Item", content.transform, typeof(Toggle), typeof(Image), typeof(LayoutElement));
            item.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 28f);
            item.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
            item.GetComponent<LayoutElement>().preferredHeight = 28f;

            GameObject itemBackground = CreateDropdownObject("Item Background", item.transform, typeof(Image));
            RectTransform itemBackgroundRect = itemBackground.GetComponent<RectTransform>();
            itemBackgroundRect.anchorMin = Vector2.zero;
            itemBackgroundRect.anchorMax = Vector2.one;
            itemBackgroundRect.offsetMin = Vector2.zero;
            itemBackgroundRect.offsetMax = Vector2.zero;
            itemBackground.GetComponent<Image>().color = new Color(0.86f, 0.86f, 0.86f, 1f);

            GameObject itemCheckmark = CreateDropdownObject("Item Checkmark", item.transform, typeof(Image));
            RectTransform itemCheckmarkRect = itemCheckmark.GetComponent<RectTransform>();
            itemCheckmarkRect.anchorMin = new Vector2(0f, 0.5f);
            itemCheckmarkRect.anchorMax = new Vector2(0f, 0.5f);
            itemCheckmarkRect.pivot = new Vector2(0.5f, 0.5f);
            itemCheckmarkRect.anchoredPosition = new Vector2(12f, 0f);
            itemCheckmarkRect.sizeDelta = new Vector2(10f, 10f);
            itemCheckmark.GetComponent<Image>().color = Color.black;

            GameObject itemLabel = CreateDropdownObject("Item Label", item.transform, typeof(TextMeshProUGUI));
            RectTransform itemLabelRect = itemLabel.GetComponent<RectTransform>();
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(28f, 2f);
            itemLabelRect.offsetMax = new Vector2(-8f, -2f);

            TextMeshProUGUI itemText = itemLabel.GetComponent<TextMeshProUGUI>();
            itemText.text = "Option";
            itemText.font = font;
            itemText.fontSize = 16f;
            itemText.color = Color.black;
            itemText.alignment = TextAlignmentOptions.Left;

            Toggle toggle = item.GetComponent<Toggle>();
            toggle.targetGraphic = itemBackground.GetComponent<Image>();
            toggle.graphic = itemCheckmark.GetComponent<Image>();

            ScrollRect scrollRect = template.GetComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            resolutionDropdown.template = templateRect;
            resolutionDropdown.itemText = itemText;
            template.SetActive(false);
        }

        private static GameObject CreateDropdownObject(string name, Transform parent, params System.Type[] components)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            foreach (var component in components)
                obj.AddComponent(component);

            obj.transform.SetParent(parent, false);
            return obj;
        }

        public void OpenSaveLoad(VNSaveLoadPanel.PanelMode mode)
        {
            RefreshRuntimeBindings();

            if (ui != null)
            {
                ui.OpenSaveLoad(mode);
                gameObject.SetActive(false);
                return;
            }

            var saveLoad = Object.FindFirstObjectByType<VNSaveLoadPanel>(FindObjectsInactive.Include);
            if (saveLoad != null)
            {
                saveLoad.Open(mode);
                gameObject.SetActive(false);
            }
        }

        public bool IsOpen => gameObject.activeSelf;
    }
}
