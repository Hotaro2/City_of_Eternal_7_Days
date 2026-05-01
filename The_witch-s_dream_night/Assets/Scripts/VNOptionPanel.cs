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

            for (int i = 0; i < allResolutions.Length; i++)
            {
                string option = allResolutions[i].width + " x " + allResolutions[i].height;
                if (options.Contains(option)) continue;

                options.Add(option);
                resolutions.Add(allResolutions[i]);

                if (allResolutions[i].width == Screen.width && allResolutions[i].height == Screen.height)
                    currentResIndex = options.Count - 1;
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResIndex;
            resolutionDropdown.RefreshShownValue();

            if (fullscreenToggle != null) fullscreenToggle.isOn = Screen.fullScreen;
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
            if (fullscreenToggle != null) fullscreenToggle.isOn = Screen.fullScreen;
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
                Screen.SetResolution(resolution.width, resolution.height, fullscreenToggle != null && fullscreenToggle.isOn);
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

            float secondsPerChar = Mathf.Lerp(0.1f, 0.0f, settings.textSpeed);
            ui?.GetComponentInChildren<VNTextTyper>()?.SetSpeed(secondsPerChar);
            ui?.SetAutoWaitMultiplier(settings.autoWaitMultiplier);
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
