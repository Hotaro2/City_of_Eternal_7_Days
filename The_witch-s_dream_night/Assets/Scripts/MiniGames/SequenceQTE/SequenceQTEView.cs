using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VN.MiniGames
{
    public sealed class SequenceQTEView : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Text")]
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private TMP_Text roundText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text mistakeText;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private List<TMP_Text> keyTexts = new();

        [Header("Images")]
        [SerializeField] private Image timerFillImage;
        [SerializeField] private Color pendingColor = new(0.85f, 0.86f, 0.9f, 1f);
        [SerializeField] private Color currentColor = new(1f, 0.88f, 0.25f, 1f);
        [SerializeField] private Color completeColor = new(0.35f, 0.95f, 0.62f, 1f);
        [SerializeField] private Color failedColor = new(0.95f, 0.25f, 0.34f, 1f);

        public void SetVisible(bool visible)
        {
            ResolveRoot();
            if (root != null) root.SetActive(visible);
        }

        public void ShowReady(SequenceQTEMiniGameDefinition definition)
        {
            ResolveRoot();
            SetVisible(true);

            if (promptText != null) promptText.text = definition.PromptText;
            if (roundText != null) roundText.text = string.Empty;
            if (timerText != null) timerText.text = definition.TimeLimitSeconds.ToString("0.0");
            if (mistakeText != null) mistakeText.text = $"Mistakes 0 / {definition.MistakeLimit}";
            if (resultText != null) resultText.text = string.Empty;

            UpdateTimer(definition.TimeLimitSeconds, definition.TimeLimitSeconds);
        }

        public void ShowSequence(IReadOnlyList<KeyCode> sequence, int currentIndex, int roundIndex, int roundCount, int mistakes, int mistakeLimit)
        {
            if (roundText != null) roundText.text = $"Round {roundIndex + 1} / {roundCount}";
            if (mistakeText != null) mistakeText.text = $"Mistakes {mistakes} / {mistakeLimit}";

            for (int i = 0; i < keyTexts.Count; i++)
            {
                TMP_Text keyText = keyTexts[i];
                if (keyText == null) continue;

                bool hasKey = sequence != null && i < sequence.Count;
                keyText.gameObject.SetActive(hasKey);
                if (!hasKey) continue;

                keyText.text = FormatKey(sequence[i]);
                keyText.color = GetKeyColor(i, currentIndex);
            }
        }

        public void ShowMistake(IReadOnlyList<KeyCode> sequence, int currentIndex)
        {
            if (sequence == null || currentIndex < 0 || currentIndex >= keyTexts.Count) return;
            if (currentIndex >= sequence.Count) return;

            TMP_Text keyText = keyTexts[currentIndex];
            if (keyText != null) keyText.color = failedColor;
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

        private Color GetKeyColor(int index, int currentIndex)
        {
            if (index < currentIndex) return completeColor;
            if (index == currentIndex) return currentColor;
            return pendingColor;
        }

        private static string FormatKey(KeyCode key)
        {
            return key switch
            {
                KeyCode.UpArrow => "↑",
                KeyCode.DownArrow => "↓",
                KeyCode.LeftArrow => "←",
                KeyCode.RightArrow => "→",
                KeyCode.Space => "SPACE",
                _ => key.ToString().ToUpperInvariant()
            };
        }
    }
}
