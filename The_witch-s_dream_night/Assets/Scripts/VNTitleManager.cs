using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VN
{
    public sealed class VNTitleManager : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string gameSceneName = "SampleScene";

        [Header("UI Panels")]
        [SerializeField] private VNOptionPanel optionPanel;
        [SerializeField] private VNSaveLoadPanel saveLoadPanel;

        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button optionButton;
        [SerializeField] private Button quitButton;

        [Header("Visual Detail")]
        [SerializeField] private Image fader;
        [SerializeField] private TMP_Text versionText;

        [Header("Audio")]
        [SerializeField] private VNAssetDatabase assetDatabase;
        [SerializeField] private AudioSource titleBgmSource;
        [SerializeField] private AudioSource titleSfxSource;
        [SerializeField] private string titleBGMKey = "title_theme";

        private VNPresenter scenePresenter;
        private float masterVolume = 1f;
        private float bgmVolume = 1f;
        private float sfxVolume = 1f;

        public VNAssetDatabase AssetDatabase => assetDatabase;

        private void Awake()
        {
            if (versionText != null) versionText.text = "v" + Application.version;

            if (optionPanel == null) optionPanel = Object.FindFirstObjectByType<VNOptionPanel>(FindObjectsInactive.Include);
            if (saveLoadPanel == null) saveLoadPanel = Object.FindFirstObjectByType<VNSaveLoadPanel>(FindObjectsInactive.Include);

            if (fader != null)
            {
                fader.gameObject.SetActive(true);
                fader.color = Color.black;
                fader.raycastTarget = false;
            }

            startButton?.onClick.AddListener(OnNewGame);
            continueButton?.onClick.AddListener(OnContinue);
            loadButton?.onClick.AddListener(() => saveLoadPanel?.Open(VNSaveLoadPanel.PanelMode.Load));
            optionButton?.onClick.AddListener(() => optionPanel?.ShowSettings());
            quitButton?.onClick.AddListener(OnQuit);

            if (continueButton != null)
                continueButton.interactable = VNSaveService.GetLatestSaveSlot() != -1;

            StartCoroutine(FadeRoutine(0f));

            scenePresenter = FindFirstObjectByType<VNPresenter>();
            ApplySavedAudioSettings();
            PlayTitleBGM();
        }

        private void ApplySavedAudioSettings()
        {
            VNSettingsData settings = VNSettingsData.Load();
            SetVolume(settings.masterVolume, settings.bgmVolume, settings.sfxVolume);
        }

        private void EnsureAudioSources()
        {
            titleBgmSource = EnsureOrCreateAudioSource(titleBgmSource, "TitleBGMSource", true);
            titleSfxSource = EnsureOrCreateAudioSource(titleSfxSource, "TitleSFXSource", false);
        }

        private AudioSource EnsureOrCreateAudioSource(AudioSource existingSource, string childName, bool loop)
        {
            AudioSource source = existingSource;

            if (source == null)
            {
                Transform child = transform.Find(childName);
                if (child != null)
                    source = child.GetComponent<AudioSource>();
            }

            if (source == null)
            {
                var sourceObject = new GameObject(childName, typeof(AudioSource));
                sourceObject.transform.SetParent(transform, false);
                source = sourceObject.GetComponent<AudioSource>();
            }

            ConfigureAudioSource(source, loop);
            return source;
        }

        private static void ConfigureAudioSource(AudioSource source, bool loop)
        {
            if (source == null) return;

            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
        }

        public void SetVolume(float master, float bgm, float sfx)
        {
            masterVolume = Mathf.Clamp01(master);
            bgmVolume = Mathf.Clamp01(bgm);
            sfxVolume = Mathf.Clamp01(sfx);

            if (scenePresenter != null)
            {
                scenePresenter.SetVolume(masterVolume, bgmVolume, sfxVolume);
                return;
            }

            EnsureAudioSources();
            ApplyLocalAudioVolume();
        }

        private void ApplyLocalAudioVolume()
        {
            if (titleBgmSource != null) titleBgmSource.volume = masterVolume * bgmVolume;
            if (titleSfxSource != null) titleSfxSource.volume = masterVolume * sfxVolume;
        }

        private void PlayTitleBGM()
        {
            if (string.IsNullOrEmpty(titleBGMKey))
            {
                StopLocalBGM();
                return;
            }

            if (scenePresenter != null)
            {
                scenePresenter.PlayBGM(titleBGMKey);
                return;
            }

            EnsureAudioSources();
            ApplyLocalAudioVolume();

            if (assetDatabase == null || titleBgmSource == null) return;

            AudioClip clip = assetDatabase.GetBGM(titleBGMKey);
            if (clip == null)
            {
                StopLocalBGM();
                return;
            }

            bool clipChanged = titleBgmSource.clip != clip;
            titleBgmSource.clip = clip;
            titleBgmSource.loop = true;

            if (clipChanged || !titleBgmSource.isPlaying)
                titleBgmSource.Play();
        }

        private void StopLocalBGM()
        {
            if (titleBgmSource == null) return;

            titleBgmSource.Stop();
            titleBgmSource.clip = null;
        }

        private void OnNewGame()
        {
            StartCoroutine(TransitionRoutine(gameSceneName));
        }

        private void OnContinue()
        {
            int latest = VNSaveService.GetLatestSaveSlot();
            if (latest == -1) return;

            PlayerPrefs.SetInt("LoadOnStart_Slot", latest);
            StartCoroutine(TransitionRoutine(gameSceneName));
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator TransitionRoutine(string sceneName)
        {
            if (fader != null) fader.raycastTarget = true;
            yield return FadeRoutine(1f);
            SceneManager.LoadScene(sceneName);
        }

        private IEnumerator FadeRoutine(float targetAlpha)
        {
            if (fader == null) yield break;

            float startAlpha = fader.color.a;
            float elapsed = 0f;
            float duration = 1f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                fader.color = new Color(0f, 0f, 0f, alpha);
                yield return null;
            }

            fader.color = new Color(0f, 0f, 0f, targetAlpha);

            if (targetAlpha <= 0f)
            {
                fader.raycastTarget = false;
                fader.gameObject.SetActive(false);
            }
            else
            {
                fader.raycastTarget = true;
                fader.gameObject.SetActive(true);
            }
        }
    }
}
