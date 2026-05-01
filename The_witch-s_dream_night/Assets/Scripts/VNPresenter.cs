using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace VN
{
    public sealed class VNPresenter : MonoBehaviour, IVNPresenter
    {
        [Header("Data")]
        [SerializeField] private VNAssetDatabase assetDB;

        [Header("Visual Components")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Transform characterRoot;
        [SerializeField] private GameObject characterPrefab;
        [SerializeField] private RectTransform masterUIRoot;

        [Header("Audio Components")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Volume Settings")]
        private float masterVolume = 1.0f;
        private float bgmVolume = 1.0f;
        private float sfxVolume = 1.0f;

        [SerializeField] private string currentBackgroundKey;
        [SerializeField] private string currentBGMKey;

        public VNAssetDatabase AssetDatabase => assetDB;
        public string CurrentBackground => currentBackgroundKey;
        public string CurrentBGM => currentBGMKey;
        public bool IsSkipMode { get; set; }

        private readonly Dictionary<string, (VNCharacterState state, GameObject obj)> activeCharacters = new();
        private Vector2 masterUIRootOrigin;
        private Coroutine shakeRoutine;

        private void Awake()
        {
            EnsureAudioSources();
            if (masterUIRoot != null) masterUIRootOrigin = masterUIRoot.anchoredPosition;
        }

        private void EnsureAudioSources()
        {
            bgmSource = EnsureOrCreateAudioSource(bgmSource, "BGMSource", true);
            sfxSource = EnsureOrCreateAudioSource(sfxSource, "SFXSource", false);
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

        public void SetBackground(string bgKey, string transition = null)
        {
            if (backgroundImage == null) return;

            if (string.IsNullOrEmpty(bgKey))
            {
                backgroundImage.gameObject.SetActive(false);
                backgroundImage.sprite = null;
                currentBackgroundKey = string.Empty;
                return;
            }

            if (assetDB == null) return;

            var sprite = assetDB.GetBackground(bgKey);
            if (sprite == null) return;

            if (transition == "fade" && backgroundImage.gameObject.activeSelf)
            {
                StartCoroutine(FadeBackgroundRoutine(sprite));
            }
            else
            {
                backgroundImage.sprite = sprite;
                backgroundImage.gameObject.SetActive(true);
            }

            currentBackgroundKey = bgKey;
        }

        private System.Collections.IEnumerator FadeBackgroundRoutine(Sprite nextSprite)
        {
            var fadeObj = new GameObject("BackgroundFade", typeof(Image));
            fadeObj.transform.SetParent(backgroundImage.transform.parent, false);
            fadeObj.transform.SetSiblingIndex(backgroundImage.transform.GetSiblingIndex() + 1);

            var fadeImg = fadeObj.GetComponent<Image>();
            fadeImg.sprite = nextSprite;
            fadeImg.rectTransform.anchorMin = backgroundImage.rectTransform.anchorMin;
            fadeImg.rectTransform.anchorMax = backgroundImage.rectTransform.anchorMax;
            fadeImg.rectTransform.offsetMin = backgroundImage.rectTransform.offsetMin;
            fadeImg.rectTransform.offsetMax = backgroundImage.rectTransform.offsetMax;

            Color color = fadeImg.color;
            color.a = 0f;
            fadeImg.color = color;

            float duration = IsSkipMode ? 0.05f : 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                color.a = Mathf.Clamp01(elapsed / duration);
                fadeImg.color = color;
                yield return null;
            }

            backgroundImage.sprite = nextSprite;
            Destroy(fadeObj);
        }

        public void SetCharacter(string charName, string expression, string position = null, string transition = null)
        {
            if (assetDB == null || characterRoot == null) return;

            string key = charName.ToLower();
            var sprite = assetDB.GetCharacter(charName, expression);
            if (sprite == null) return;

            GameObject charObj;
            bool isNew = false;

            if (!activeCharacters.ContainsKey(key))
            {
                charObj = characterPrefab != null
                    ? Instantiate(characterPrefab, characterRoot)
                    : new GameObject(key, typeof(Image), typeof(CanvasGroup));

                charObj.transform.SetParent(characterRoot, false);
                activeCharacters[key] = (new VNCharacterState
                {
                    name = charName,
                    expression = expression,
                    position = position
                }, charObj);
                isNew = true;
            }
            else
            {
                charObj = activeCharacters[key].obj;
            }

            var (state, _) = activeCharacters[key];
            state.expression = expression;
            if (position != null) state.position = position;
            activeCharacters[key] = (state, charObj);

            var image = charObj.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = sprite;
                image.SetNativeSize();
            }

            ApplyPosition(charObj.transform as RectTransform, state.position);

            if (isNew && transition == "fade")
                StartCoroutine(FadeCharacterRoutine(charObj, true));
        }

        private System.Collections.IEnumerator FadeCharacterRoutine(GameObject obj, bool fadeIn, System.Action onComplete = null)
        {
            var canvasGroup = obj.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = obj.AddComponent<CanvasGroup>();

            float duration = IsSkipMode ? 0.02f : 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = fadeIn
                    ? Mathf.Clamp01(elapsed / duration)
                    : Mathf.Clamp01(1f - (elapsed / duration));
                yield return null;
            }

            canvasGroup.alpha = fadeIn ? 1f : 0f;
            onComplete?.Invoke();
        }

        private void ApplyPosition(RectTransform rect, string position)
        {
            if (rect == null) return;

            float x = 0f;
            switch (position?.ToLower())
            {
                case "left":
                    x = -600f;
                    break;
                case "right":
                    x = 600f;
                    break;
                case "center":
                default:
                    x = 0f;
                    break;
            }

            rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y);
        }

        public void HideCharacter(string charName, string transition = null)
        {
            string key = charName.ToLower();
            if (!activeCharacters.TryGetValue(key, out var entry)) return;

            if (transition == "fade" && !IsSkipMode)
            {
                StartCoroutine(FadeCharacterRoutine(entry.obj, false, () =>
                {
                    if (entry.obj != null) Destroy(entry.obj);
                    activeCharacters.Remove(key);
                }));
            }
            else
            {
                Destroy(entry.obj);
                activeCharacters.Remove(key);
            }
        }

        public void ShakeScreen(float duration, float strength)
        {
            if (masterUIRoot == null || IsSkipMode) return;

            if (shakeRoutine != null)
                StopCoroutine(shakeRoutine);

            RestoreScreenPosition();
            shakeRoutine = StartCoroutine(ShakeUIRoutine(duration, strength));
        }

        private System.Collections.IEnumerator ShakeUIRoutine(float duration, float strength)
        {
            float safeDuration = Mathf.Max(0f, duration);
            float safeStrength = Mathf.Max(0f, strength);
            float elapsed = 0f;

            if (safeDuration <= 0f || safeStrength <= 0f)
            {
                RestoreScreenPosition();
                shakeRoutine = null;
                yield break;
            }

            while (elapsed < safeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                Vector2 offset = Random.insideUnitCircle * safeStrength;
                masterUIRoot.anchoredPosition = masterUIRootOrigin + offset;
                yield return null;
            }

            RestoreScreenPosition();
            shakeRoutine = null;
        }

        public void ResetTransientState()
        {
            StopAllCoroutines();
            shakeRoutine = null;
            CleanupBackgroundFadeObjects();
            RestoreScreenPosition();
        }

        private void RestoreScreenPosition()
        {
            if (masterUIRoot != null)
                masterUIRoot.anchoredPosition = masterUIRootOrigin;
        }

        private void CleanupBackgroundFadeObjects()
        {
            if (backgroundImage == null || backgroundImage.transform.parent == null) return;

            var parent = backgroundImage.transform.parent;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child != null && child.name == "BackgroundFade")
                    Destroy(child.gameObject);
            }
        }

        public void ClearAllCharacters()
        {
            foreach (var entry in activeCharacters.Values)
            {
                if (entry.obj != null) Destroy(entry.obj);
            }

            activeCharacters.Clear();
        }

        public List<VNCharacterState> GetCurrentCharacters()
        {
            return activeCharacters.Values.Select(value => value.state).ToList();
        }

        public void SetVolume(float master, float bgm, float sfx)
        {
            EnsureAudioSources();

            masterVolume = master;
            bgmVolume = bgm;
            sfxVolume = sfx;
            ApplyVolume();
        }

        private void ApplyVolume()
        {
            EnsureAudioSources();

            if (bgmSource != null) bgmSource.volume = masterVolume * bgmVolume;
            if (sfxSource != null) sfxSource.volume = masterVolume * sfxVolume;
        }

        public void PlayBGM(string clipKey, float volume = 1f)
        {
            EnsureAudioSources();

            if (string.IsNullOrEmpty(clipKey))
            {
                StopBGM();
                return;
            }

            if (assetDB == null || bgmSource == null) return;

            var clip = assetDB.GetBGM(clipKey);
            if (clip == null) return;

            bool clipChanged = bgmSource.clip != clip;
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.volume = masterVolume * bgmVolume * volume;

            if (clipChanged || !bgmSource.isPlaying)
                bgmSource.Play();

            currentBGMKey = clipKey;
        }

        public void PlaySFX(string clipKey, float volume = 1f)
        {
            EnsureAudioSources();

            if (string.IsNullOrEmpty(clipKey)) return;
            if (assetDB == null || sfxSource == null) return;

            var clip = assetDB.GetSFX(clipKey);
            if (clip != null)
                sfxSource.PlayOneShot(clip, masterVolume * sfxVolume * volume);
        }

        private void StopBGM()
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
                bgmSource.clip = null;
            }

            currentBGMKey = string.Empty;
        }
    }
}
