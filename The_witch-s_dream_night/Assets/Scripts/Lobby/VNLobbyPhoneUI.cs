using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VN
{
    public sealed class VNLobbyPhoneUI : MonoBehaviour
    {
        [SerializeField] private Button toggleButton;
        [SerializeField] private Button outsideCloseButton;
        [SerializeField] private Button storyButton;
        [SerializeField] private Button qteButton;
        [SerializeField] private RectTransform phonePanel;
        [SerializeField] private CanvasGroup phoneCanvasGroup;
        [SerializeField] private string storySceneName = "Day6VNScene";
        [SerializeField] private string qteSceneName = "MiniGameScene";
        [SerializeField] private Vector2 openPosition = new(-28f, 0f);
        [SerializeField] private Vector2 closedPosition = new(580f, 0f);
        [SerializeField] private float animationSeconds = 0.32f;

        private Coroutine animationRoutine;
        private bool isOpen;

        private void Update()
        {
            if (!isOpen || outsideCloseButton != null || phonePanel == null)
                return;

            if (!TryGetPointerDownPosition(out Vector2 screenPosition))
                return;

            Canvas canvas = phonePanel.GetComponentInParent<Canvas>();
            Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            if (!RectTransformUtility.RectangleContainsScreenPoint(phonePanel, screenPosition, eventCamera))
                Close();
        }

        private void Awake()
        {
            toggleButton?.onClick.AddListener(Toggle);
            outsideCloseButton?.onClick.AddListener(Close);
            storyButton?.onClick.AddListener(OpenStory);
            qteButton?.onClick.AddListener(OpenQTE);
            SetState(false);
        }

        private void OnDestroy()
        {
            toggleButton?.onClick.RemoveListener(Toggle);
            outsideCloseButton?.onClick.RemoveListener(Close);
            storyButton?.onClick.RemoveListener(OpenStory);
            qteButton?.onClick.RemoveListener(OpenQTE);
        }

        private void OpenStory()
        {
            LoadScene(storySceneName);
        }

        private void OpenQTE()
        {
            LoadScene(qteSceneName);
        }

        private static void LoadScene(string sceneName)
        {
            if (!string.IsNullOrWhiteSpace(sceneName))
                SceneManager.LoadScene(sceneName);
        }

        private void Toggle()
        {
            Animate(!isOpen);
        }

        private void Close()
        {
            Animate(false);
        }

        private void Animate(bool open)
        {
            if (phonePanel == null || phoneCanvasGroup == null) return;

            if (animationRoutine != null)
                StopCoroutine(animationRoutine);

            animationRoutine = StartCoroutine(AnimateRoutine(open));
        }

        private IEnumerator AnimateRoutine(bool open)
        {
            isOpen = open;
            if (outsideCloseButton != null)
                outsideCloseButton.gameObject.SetActive(true);

            phoneCanvasGroup.blocksRaycasts = true;

            Vector2 startPosition = phonePanel.anchoredPosition;
            Vector2 targetPosition = open ? openPosition : closedPosition;
            float startAlpha = phoneCanvasGroup.alpha;
            float targetAlpha = open ? 1f : 0f;
            float duration = Mathf.Max(0.01f, animationSeconds);
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / duration);
                t = t * t * (3f - 2f * t);
                phonePanel.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, t);
                phoneCanvasGroup.alpha = Mathf.LerpUnclamped(startAlpha, targetAlpha, t);
                yield return null;
            }

            phonePanel.anchoredPosition = targetPosition;
            phoneCanvasGroup.alpha = targetAlpha;
            phoneCanvasGroup.blocksRaycasts = open;
            phoneCanvasGroup.interactable = open;
            if (outsideCloseButton != null)
                outsideCloseButton.gameObject.SetActive(open);

            animationRoutine = null;
        }

        private void SetState(bool open)
        {
            isOpen = open;
            if (phonePanel != null)
                phonePanel.anchoredPosition = open ? openPosition : closedPosition;

            if (phoneCanvasGroup != null)
            {
                phoneCanvasGroup.alpha = open ? 1f : 0f;
                phoneCanvasGroup.blocksRaycasts = open;
                phoneCanvasGroup.interactable = open;
            }

            if (outsideCloseButton != null)
                outsideCloseButton.gameObject.SetActive(open);
        }

        private static bool TryGetPointerDownPosition(out Vector2 screenPosition)
        {
#if ENABLE_INPUT_SYSTEM
            Pointer pointer = Pointer.current;
            if (pointer != null && pointer.press.wasPressedThisFrame)
            {
                screenPosition = pointer.position.ReadValue();
                return true;
            }
#else
            if (Input.GetMouseButtonDown(0))
            {
                screenPosition = Input.mousePosition;
                return true;
            }
#endif

            screenPosition = default;
            return false;
        }
    }
}
