using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VN
{
    public sealed class VNChoiceSelectionEffect : MonoBehaviour
    {
        [SerializeField] private float duration = 0.34f;
        [SerializeField] private float selectedScale = 1.08f;
        [SerializeField] private float unselectedAlpha = 0.22f;
        [SerializeField] private Color selectedTextColor = new Color(1f, 0.92f, 0.86f, 1f);

        public IEnumerator Play(IReadOnlyList<Button> choices)
        {
            if (choices == null) yield break;

            var selectedRect = transform as RectTransform;
            Vector3 baseScale = selectedRect != null ? selectedRect.localScale : Vector3.one;
            var selectedText = GetComponentInChildren<TMP_Text>(true);
            Color originalTextColor = selectedText != null ? selectedText.color : Color.white;

            SetChoicesInteractable(choices, false);

            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / duration);
                float pulse = Mathf.Sin(t * Mathf.PI);
                float alphaOut = Mathf.SmoothStep(1f, 0f, Mathf.Max(0f, (t - 0.56f) / 0.44f));

                if (selectedRect != null)
                {
                    float scale = Mathf.Lerp(1f, selectedScale, pulse);
                    selectedRect.localScale = baseScale * scale;
                }

                if (selectedText != null)
                {
                    selectedText.color = Color.Lerp(originalTextColor, selectedTextColor, Mathf.SmoothStep(0f, 1f, pulse));
                }

                for (int i = 0; i < choices.Count; i++)
                {
                    var button = choices[i];
                    if (button == null) continue;

                    var group = GetOrAddCanvasGroup(button.gameObject);
                    if (button.gameObject == gameObject)
                    {
                        group.alpha = alphaOut;
                    }
                    else
                    {
                        float fade = Mathf.SmoothStep(1f, unselectedAlpha, t);
                        group.alpha = Mathf.Min(fade, alphaOut);
                    }
                }

                yield return null;
            }

            if (selectedRect != null) selectedRect.localScale = baseScale;
            if (selectedText != null) selectedText.color = originalTextColor;
        }

        private static void SetChoicesInteractable(IReadOnlyList<Button> choices, bool interactable)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                if (choices[i] == null) continue;
                choices[i].interactable = interactable;

                var group = GetOrAddCanvasGroup(choices[i].gameObject);
                group.interactable = interactable;
                group.blocksRaycasts = interactable;
            }
        }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            var group = target.GetComponent<CanvasGroup>();
            if (group == null) group = target.AddComponent<CanvasGroup>();
            return group;
        }
    }
}
