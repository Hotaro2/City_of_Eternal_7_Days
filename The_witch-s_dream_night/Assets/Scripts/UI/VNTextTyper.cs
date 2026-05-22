using System.Collections;
using TMPro;
using UnityEngine;

namespace VN
{
    public sealed class VNTextTyper : MonoBehaviour
    {
        [SerializeField] private TMP_Text lineText;
        [SerializeField] private float defaultSecondsPerChar = 0.03f;

        private float currentSecondsPerChar;
        private bool isTyping;
        private bool skipRequested;

        public bool IsTyping => isTyping;
        public string CurrentText => lineText != null ? lineText.text : string.Empty;
        public TMP_Text TextComponent => lineText;

        private void Awake()
        {
            currentSecondsPerChar = defaultSecondsPerChar;
            VNKoreanFontFallback.ApplyTo(lineText);
        }

        public void SetSpeed(float secondsPerChar)
        {
            currentSecondsPerChar = Mathf.Max(0, secondsPerChar);
        }

        public void Skip()
        {
            if (isTyping) skipRequested = true;
        }

        public void ShowImmediate(string text)
        {
            if (lineText == null) return;
            VNKoreanFontFallback.ApplyTo(lineText);
            lineText.text = text;
            lineText.maxVisibleCharacters = int.MaxValue;
            isTyping = false;
        }

        public IEnumerator TypeText(string text, bool instant = false)
        {
            if (lineText == null) yield break;
            VNKoreanFontFallback.ApplyTo(lineText);

            if (instant)
            {
                ShowImmediate(text);
                yield break;
            }

            isTyping = true;
            skipRequested = false;
            lineText.text = text;
            lineText.maxVisibleCharacters = 0;
            lineText.ForceMeshUpdate();

            int total = lineText.textInfo.characterCount;
            int visible = 0;
            float timer = 0f;

            while (visible < total && !skipRequested)
            {
                timer += Time.unscaledDeltaTime;
                while (timer >= currentSecondsPerChar && visible < total)
                {
                    timer -= currentSecondsPerChar;
                    visible++;
                    lineText.maxVisibleCharacters = visible;
                }
                yield return null;
            }

            lineText.maxVisibleCharacters = int.MaxValue;
            isTyping = false;
            skipRequested = false;
        }
    }
}
