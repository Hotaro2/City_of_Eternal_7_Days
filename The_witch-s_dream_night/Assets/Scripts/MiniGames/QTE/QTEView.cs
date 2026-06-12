using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VN.MiniGames
{
    public sealed class QTEView : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Text")]
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private TMP_Text keyText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text resultText;

        [Header("Images")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image keyImage;
        [SerializeField] private Image timerFillImage;

        public void SetVisible(bool visible)
        {
            ResolveRoot();
            if (root != null) root.SetActive(visible);
        }

        public void ShowReady(QTEMiniGameDefinition definition)
        {
            ResolveRoot();
            SetVisible(true);

            if (promptText != null) promptText.text = definition.PromptText;
            if (keyText != null) keyText.text = definition.RequiredKey.ToString().ToUpperInvariant();
            if (timerText != null) timerText.text = definition.TimeLimitSeconds.ToString("0.0");
            if (resultText != null) resultText.text = string.Empty;

            ApplySprite(backgroundImage, definition.BackgroundSprite, true);

            if (keyImage != null)
            {
                bool hasKeySprite = definition.RequiredKeySprite != null;
                ApplySprite(keyImage, definition.RequiredKeySprite, hasKeySprite);
                if (keyText != null) keyText.gameObject.SetActive(!hasKeySprite);
            }

            UpdateTimer(definition.TimeLimitSeconds, definition.TimeLimitSeconds);
        }

        public void UpdateTimer(float remainingSeconds, float totalSeconds)
        {
            float safeTotal = Mathf.Max(0.01f, totalSeconds);
            float normalized = Mathf.Clamp01(remainingSeconds / safeTotal);

            if (timerFillImage != null) timerFillImage.fillAmount = normalized;
            if (timerText != null) timerText.text = Mathf.Max(0f, remainingSeconds).ToString("0.0");
        }

        public void ShowResult(bool isSuccess, string rank)
        {
            if (resultText == null) return;
            resultText.text = isSuccess ? $"SUCCESS\n{rank}" : $"FAIL\n{rank}";
        }

        private void ResolveRoot()
        {
            if (root == null) root = gameObject;
        }

        private static void ApplySprite(Image image, Sprite sprite, bool visibleWhenMissing)
        {
            if (image == null) return;

            image.sprite = sprite;
            if (sprite != null) image.color = Color.white;
            image.enabled = sprite != null || visibleWhenMissing;
        }
    }
}
