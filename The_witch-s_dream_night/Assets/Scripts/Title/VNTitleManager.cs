using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace VN
{
    public sealed class VNTitleManager : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string gameSceneName = "VNScene";

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

        [Header("Intro Video")]
        [SerializeField] private Canvas titleCanvas;
        [SerializeField] private Canvas introCanvas;
        [SerializeField] private RawImage introVideoImage;
        [SerializeField] private Camera introCamera;
        [SerializeField] private string introVideoPath = "Title/TitleIntro.mp4";
        [SerializeField] private bool allowIntroSkip = true;
        [SerializeField, Min(0f)] private float introSkipDelay = 0.5f;

        [Header("Audio")]
        [SerializeField] private VNAssetDatabase assetDatabase;
        [SerializeField] private AudioSource titleBgmSource;
        [SerializeField] private AudioSource titleSfxSource;
        [SerializeField] private string titleBGMKey = "title_theme";

        private VNPresenter scenePresenter;
        private float masterVolume = 1f;
        private float bgmVolume = 1f;
        private float sfxVolume = 1f;
        private VideoPlayer introVideoPlayer;
        private RenderTexture introVideoTexture;

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

            scenePresenter = FindFirstObjectByType<VNPresenter>();
            ApplySavedAudioSettings();
            StartCoroutine(InitializeTitleRoutine());
        }

        private IEnumerator InitializeTitleRoutine()
        {
            if (titleCanvas == null)
                titleCanvas = FindFirstObjectByType<Canvas>();

            if (introCamera == null)
                introCamera = Camera.main;

            if (titleCanvas != null)
                titleCanvas.enabled = false;

            if (introCanvas != null)
                introCanvas.gameObject.SetActive(true);

            yield return PlayIntroVideoRoutine();

            if (introCanvas != null)
                introCanvas.gameObject.SetActive(false);

            if (titleCanvas != null)
                titleCanvas.enabled = true;

            yield return FadeRoutine(0f);
            PlayTitleBGM();
        }

        private IEnumerator PlayIntroVideoRoutine()
        {
            string videoPath = Path.Combine(Application.streamingAssetsPath, introVideoPath);
            if (!File.Exists(videoPath) || introCamera == null)
                yield break;

            introVideoPlayer = introCamera.GetComponent<VideoPlayer>();
            if (introVideoPlayer == null)
                introVideoPlayer = introCamera.gameObject.AddComponent<VideoPlayer>();

            bool prepareFinished = false;
            bool playbackFinished = false;
            bool playbackFailed = false;

            void OnPrepared(VideoPlayer _) => prepareFinished = true;
            void OnFinished(VideoPlayer _) => playbackFinished = true;
            void OnError(VideoPlayer _, string message)
            {
                Debug.LogWarning($"[VNTitleManager] Intro video could not be played: {message}");
                playbackFailed = true;
            }

            introVideoPlayer.playOnAwake = false;
            introVideoPlayer.isLooping = false;
            introVideoPlayer.skipOnDrop = true;
            introVideoPlayer.waitForFirstFrame = true;
            if (introVideoImage != null)
            {
                introVideoTexture = new RenderTexture(
                    Mathf.Max(Screen.width, 16),
                    Mathf.Max(Screen.height, 16),
                    0,
                    RenderTextureFormat.ARGB32);
                introVideoTexture.Create();
                introVideoImage.texture = introVideoTexture;
                introVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
                introVideoPlayer.targetTexture = introVideoTexture;
            }
            else
            {
                introVideoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
                introVideoPlayer.targetCamera = introCamera;
                introVideoPlayer.targetCameraAlpha = 1f;
            }

            introVideoPlayer.aspectRatio = VideoAspectRatio.FitInside;
            introVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            introVideoPlayer.url = videoPath.Replace('\\', '/');
            introVideoPlayer.prepareCompleted += OnPrepared;
            introVideoPlayer.loopPointReached += OnFinished;
            introVideoPlayer.errorReceived += OnError;
            introVideoPlayer.Prepare();

            float prepareElapsed = 0f;
            while (!prepareFinished && !playbackFailed)
            {
                prepareElapsed += Time.unscaledDeltaTime;
                if (prepareElapsed >= 10f)
                {
                    Debug.LogWarning("[VNTitleManager] Intro video preparation timed out.");
                    playbackFailed = true;
                    break;
                }

                yield return null;
            }

            if (!playbackFailed)
            {
                introVideoPlayer.Play();
                float elapsed = 0f;

                while (!playbackFinished && !playbackFailed)
                {
                    elapsed += Time.unscaledDeltaTime;
                    if (allowIntroSkip && elapsed >= introSkipDelay && IsIntroSkipPressed())
                        break;

                    yield return null;
                }
            }

            introVideoPlayer.Stop();
            introVideoPlayer.prepareCompleted -= OnPrepared;
            introVideoPlayer.loopPointReached -= OnFinished;
            introVideoPlayer.errorReceived -= OnError;

            if (introVideoImage != null)
                introVideoImage.texture = null;

            if (introVideoTexture != null)
            {
                introVideoTexture.Release();
                Destroy(introVideoTexture);
                introVideoTexture = null;
            }
        }

        private static bool IsIntroSkipPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return UnityEngine.InputSystem.Keyboard.current?.anyKey.wasPressedThisFrame == true
                || UnityEngine.InputSystem.Mouse.current?.leftButton.wasPressedThisFrame == true;
#else
            return Input.anyKeyDown || Input.GetMouseButtonDown(0);
#endif
        }

        private void ApplySavedAudioSettings()
        {
            VNSettingsData settings = VNSettingsData.Load();
            settings.ApplyDisplaySettings();
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
