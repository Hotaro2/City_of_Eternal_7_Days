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
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image characterImage;
        [SerializeField] private Image resultImage;
        [SerializeField] private Image timerFillImage;
        [SerializeField] private List<Image> keySlotImages = new();
        [SerializeField] private List<Image> keyIconImages = new();
        [SerializeField] private Color pendingColor = new(0.85f, 0.86f, 0.9f, 1f);
        [SerializeField] private Color currentColor = new(1f, 0.88f, 0.25f, 1f);
        [SerializeField] private Color completeColor = new(0.35f, 0.95f, 0.62f, 1f);
        [SerializeField] private Color failedColor = new(0.95f, 0.25f, 0.34f, 1f);

        private SequenceQTEMiniGameDefinition currentDefinition;

        public void SetVisible(bool visible)
        {
            ResolveRoot();
            if (root != null) root.SetActive(visible);
        }

        public void ShowReady(SequenceQTEMiniGameDefinition definition)
        {
            ResolveRoot();
            SetVisible(true);
            currentDefinition = definition;

            if (promptText != null) promptText.text = definition.PromptText;
            if (roundText != null) roundText.text = string.Empty;
            if (timerText != null) timerText.text = definition.TimeLimitSeconds.ToString("0.0");
            if (mistakeText != null) mistakeText.text = $"Mistakes 0 / {definition.MistakeLimit}";
            if (resultText != null) resultText.text = string.Empty;

            ApplySprite(backgroundImage, definition.BackgroundSprite, true);
            ApplySprite(characterImage, definition.CharacterSprite, false);
            ApplySprite(resultImage, null, false);
            UpdateTimer(definition.TimeLimitSeconds, definition.TimeLimitSeconds);
        }

        public void ShowSequence(IReadOnlyList<KeyCode> sequence, int currentIndex, int roundIndex, int roundCount, int mistakes, int mistakeLimit)
        {
            if (roundText != null) roundText.text = $"Round {roundIndex + 1} / {roundCount}";
            if (mistakeText != null) mistakeText.text = $"Mistakes {mistakes} / {mistakeLimit}";

            UpdateKeyLayout(sequence);
            int visibleCount = Mathf.Max(keyTexts.Count, keySlotImages.Count, keyIconImages.Count);
            for (int i = 0; i < visibleCount; i++)
            {
                bool hasKey = sequence != null && i < sequence.Count;
                SetKeyObjectActive(i, hasKey);
                if (!hasKey) continue;

                KeyCode key = sequence[i];
                SequenceQTEKeyState state = GetKeyState(i, currentIndex);
                Sprite combinedSprite = currentDefinition != null ? currentDefinition.GetKeySprite(key, state) : null;
                if (combinedSprite != null)
                {
                    ApplyCombinedKeySprite(i, combinedSprite);
                }
                else
                {
                    ApplyKeySlot(i, GetKeySlotSprite(i, currentIndex), GetKeyColor(i, currentIndex));
                    ApplyKeyIcon(i, key, null, GetKeyColor(i, currentIndex));
                }
            }
        }

        public void ShowMistake(IReadOnlyList<KeyCode> sequence, int currentIndex)
        {
            if (sequence == null || currentIndex < 0) return;
            if (currentIndex >= sequence.Count) return;

            if (currentIndex < keyTexts.Count && keyTexts[currentIndex] != null)
                keyTexts[currentIndex].color = failedColor;

            KeyCode key = sequence[currentIndex];
            Sprite failedSprite = currentDefinition != null
                ? currentDefinition.GetKeySprite(key, SequenceQTEKeyState.Failed)
                : null;

            if (failedSprite != null)
                ApplyCombinedKeySprite(currentIndex, failedSprite);
            else
                ApplyKeySlot(currentIndex, currentDefinition != null ? currentDefinition.FailedKeySlotSprite : null, failedColor);
        }

        public void UpdateTimer(float remainingSeconds, float totalSeconds)
        {
            float safeTotal = Mathf.Max(0.01f, totalSeconds);
            float normalized = Mathf.Clamp01(remainingSeconds / safeTotal);

            if (timerFillImage != null) timerFillImage.fillAmount = normalized;
            if (timerText != null) timerText.text = Mathf.Max(0f, remainingSeconds).ToString("0.0");
        }

        public bool ShowResult(bool isSuccess, string rank)
        {
            if (resultText != null)
                resultText.text = string.Empty;

            Sprite resultSprite = null;
            if (currentDefinition != null)
                resultSprite = isSuccess ? currentDefinition.SuccessSprite : currentDefinition.FailSprite;

            if (resultImage != null && resultSprite != null)
            {
                resultImage.gameObject.SetActive(true);
                resultImage.enabled = true;
                resultImage.sprite = resultSprite;
                resultImage.color = Color.white;

                RectTransform resultRect = resultImage.rectTransform;
                resultRect.anchorMin = Vector2.zero;
                resultRect.anchorMax = Vector2.one;
                resultRect.anchoredPosition = Vector2.zero;
                resultRect.sizeDelta = Vector2.zero;
                resultImage.preserveAspect = false;
                resultImage.transform.SetAsLastSibling();
                Canvas.ForceUpdateCanvases();
            }
            return resultSprite != null;
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

        private Sprite GetKeySlotSprite(int index, int currentIndex)
        {
            if (currentDefinition == null) return null;
            if (index < currentIndex && currentDefinition.CompleteKeySlotSprite != null) return currentDefinition.CompleteKeySlotSprite;
            if (index == currentIndex && currentDefinition.CurrentKeySlotSprite != null) return currentDefinition.CurrentKeySlotSprite;
            return currentDefinition.KeySlotSprite;
        }

        private static SequenceQTEKeyState GetKeyState(int index, int currentIndex)
        {
            if (index < currentIndex) return SequenceQTEKeyState.Complete;
            if (index == currentIndex) return SequenceQTEKeyState.Current;
            return SequenceQTEKeyState.Pending;
        }

        private void UpdateKeyLayout(IReadOnlyList<KeyCode> sequence)
        {
            if (sequence == null || sequence.Count == 0) return;

            float totalWeight = 0f;
            for (int i = 0; i < sequence.Count; i++)
                totalWeight += sequence[i] == KeyCode.Space ? 1.75f : 1f;

            float cursor = 0f;
            for (int i = 0; i < sequence.Count && i < keyTexts.Count; i++)
            {
                TMP_Text keyText = keyTexts[i];
                if (keyText == null) continue;

                float weight = sequence[i] == KeyCode.Space ? 1.75f : 1f;
                float spacing = 0.006f;
                keyText.rectTransform.anchorMin = new Vector2(cursor / totalWeight + spacing, 0f);
                cursor += weight;
                keyText.rectTransform.anchorMax = new Vector2(cursor / totalWeight - spacing, 1f);
                keyText.rectTransform.anchoredPosition = Vector2.zero;
                keyText.rectTransform.sizeDelta = Vector2.zero;
            }
        }

        private void SetKeyObjectActive(int index, bool active)
        {
            if (index < keySlotImages.Count && keySlotImages[index] != null)
                keySlotImages[index].gameObject.SetActive(active);
            if (index < keyIconImages.Count && keyIconImages[index] != null && !active)
                keyIconImages[index].gameObject.SetActive(false);
            if (index < keyTexts.Count && keyTexts[index] != null)
                keyTexts[index].gameObject.SetActive(active);
        }

        private void ApplyKeySlot(int index, Sprite sprite, Color fallbackColor)
        {
            if (index >= keySlotImages.Count || keySlotImages[index] == null) return;

            Image slot = keySlotImages[index];
            slot.sprite = sprite;
            slot.color = sprite != null ? Color.white : new Color(fallbackColor.r, fallbackColor.g, fallbackColor.b, 0.18f);
            slot.enabled = true;
        }

        private void ApplyCombinedKeySprite(int index, Sprite sprite)
        {
            if (index >= keySlotImages.Count || keySlotImages[index] == null) return;

            Image slot = keySlotImages[index];
            slot.sprite = sprite;
            slot.color = Color.white;
            slot.preserveAspect = true;
            slot.enabled = true;

            if (index < keyIconImages.Count && keyIconImages[index] != null)
                keyIconImages[index].gameObject.SetActive(false);
            if (index < keyTexts.Count && keyTexts[index] != null)
                keyTexts[index].enabled = false;
        }

        private void ApplyKeyIcon(int index, KeyCode key, Sprite sprite, Color fallbackColor)
        {
            Image icon = index < keyIconImages.Count ? keyIconImages[index] : null;
            TMP_Text text = index < keyTexts.Count ? keyTexts[index] : null;

            if (icon != null)
            {
                icon.sprite = sprite;
                icon.color = Color.white;
                icon.enabled = sprite != null;
                icon.gameObject.SetActive(sprite != null);
            }

            if (text != null)
            {
                text.text = FormatKey(key);
                text.color = fallbackColor;
                text.gameObject.SetActive(true);
                text.enabled = sprite == null;
            }
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

        private static void ApplySprite(Image image, Sprite sprite, bool visibleWhenMissing)
        {
            if (image == null) return;
            image.sprite = sprite;
            if (sprite != null) image.color = Color.white;
            image.enabled = sprite != null || visibleWhenMissing;
            image.gameObject.SetActive(sprite != null || visibleWhenMissing);
        }
    }
}
