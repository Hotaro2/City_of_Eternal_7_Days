using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VN
{
    public sealed class VNLobbyController : MonoBehaviour
    {
        [Header("Scene Navigation")]
        [SerializeField] private string titleSceneName = "TitleScene";
        [SerializeField] private string mainStorySceneName = "VNScene";
        [SerializeField] private string miniGameSceneName = "MiniGameScene";

        [Header("Buttons")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button mainStoryButton;
        [SerializeField] private Button subStoryButton;
        [SerializeField] private Button archiveButton;
        [SerializeField] private Button miniGameButton;
        [SerializeField] private Button settingsButton;

        [Header("Feedback")]
        [SerializeField] private CanvasGroup noticeGroup;
        [SerializeField] private TMP_Text noticeText;
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeSeconds = 0.35f;
        [SerializeField] private float noticeSeconds = 1.8f;

        private Coroutine noticeRoutine;
        private bool isTransitioning;

        private void Awake()
        {
            VNSettingsData.Load().ApplyDisplaySettings();

            backButton?.onClick.AddListener(() => LoadScene(titleSceneName));
            mainStoryButton?.onClick.AddListener(() => LoadScene(mainStorySceneName));
            subStoryButton?.onClick.AddListener(() => ShowNotice("서브 스토리는 준비 중입니다."));
            archiveButton?.onClick.AddListener(() => ShowNotice("기록 보관소는 준비 중입니다."));
            miniGameButton?.onClick.AddListener(() => LoadScene(miniGameSceneName));
            settingsButton?.onClick.AddListener(() => ShowNotice("설정은 타이틀 또는 게임 메뉴에서 변경할 수 있습니다."));

            if (noticeGroup != null)
            {
                noticeGroup.alpha = 0f;
                noticeGroup.blocksRaycasts = false;
                noticeGroup.interactable = false;
            }

            if (fadeImage != null)
            {
                Color color = fadeImage.color;
                color.a = 0f;
                fadeImage.color = color;
                fadeImage.raycastTarget = false;
                fadeImage.gameObject.SetActive(false);
            }
        }

        private void LoadScene(string sceneName)
        {
            if (isTransitioning || string.IsNullOrWhiteSpace(sceneName)) return;
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            isTransitioning = true;

            if (fadeImage != null)
            {
                fadeImage.gameObject.SetActive(true);
                fadeImage.raycastTarget = true;

                float duration = Mathf.Max(0f, fadeSeconds);
                float timer = 0f;
                while (timer < duration)
                {
                    timer += Time.unscaledDeltaTime;
                    SetFadeAlpha(Mathf.Clamp01(timer / duration));
                    yield return null;
                }

                SetFadeAlpha(1f);
            }

            SceneManager.LoadScene(sceneName);
        }

        private void ShowNotice(string message)
        {
            if (noticeGroup == null || noticeText == null) return;

            if (noticeRoutine != null)
                StopCoroutine(noticeRoutine);

            noticeRoutine = StartCoroutine(ShowNoticeRoutine(message));
        }

        private IEnumerator ShowNoticeRoutine(string message)
        {
            noticeText.text = message;
            noticeGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, noticeSeconds));

            float timer = 0f;
            const float fadeDuration = 0.2f;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                noticeGroup.alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
                yield return null;
            }

            noticeGroup.alpha = 0f;
            noticeRoutine = null;
        }

        private void SetFadeAlpha(float alpha)
        {
            if (fadeImage == null) return;

            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }
}
