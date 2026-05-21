using System;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VN
{
    public sealed class VNUIController : MonoBehaviour
    {
        [Header("Dialogue")]
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private VNTextTyper typer;

        [Header("Choices")]
        [SerializeField] private VNChoicePanel choicePanel;

        [Header("Backlog")]
        [SerializeField] private VNBacklogManager backlogManager;

        [Header("Save/Load")]
        [SerializeField] private VNOptionPanel optionPanel;
        [SerializeField] private VNSaveLoadPanel saveLoadPanel;
        [SerializeField] private VNNameInputPanel nameInputPanel;

        [Header("Controls")]
        [SerializeField] private Button advanceButton;
        [SerializeField] private Button backlogButton;

        [Header("Screen Fade")]
        [SerializeField] private Image screenFadeImage;

        [Header("Prologue Presentation")]
        [SerializeField] private GameObject prologueRoot;
        [SerializeField] private TMP_Text prologueText;
        [SerializeField] private Button prologueAdvanceButton;
        [SerializeField] private List<Button> prologueChoiceButtons = new List<Button>();
        [SerializeField] private bool prologueTextUsesTypewriter = false;
        [SerializeField] private float prologueTextSecondsPerChar = 0.01f;
        [SerializeField] private float prologueTextFadeSeconds = 0.45f;
        [SerializeField] private float prologueChoiceFadeSeconds = 0.35f;

        [Header("Auto/Skip Settings")]
        [SerializeField] private float autoBaseWait = 0.8f;
        [SerializeField] private float autoPerCharWait = 0.02f;
        [SerializeField] private float autoWaitMultiplier = 1.0f; // 오토 대기 시간 가중치 (1.0 = 기본)
        [SerializeField] private Toggle autoToggle;
        [SerializeField] private Toggle skipToggle;

        private bool advanceRequested;
        private bool advanceButtonPrevActive;
        private bool isPrologueMode;
        private bool isPrologueChoiceSelecting;
        private readonly List<string> prologueLines = new List<string>();
        private Coroutine prologueChoiceFadeCoroutine;
        private Coroutine screenFadeCoroutine;

        public bool AutoMode => autoToggle != null && autoToggle.isOn;
        public bool SkipMode => skipToggle != null && skipToggle.isOn;

        public void SetAutoMode(bool isOn) { if (autoToggle != null) autoToggle.isOn = isOn; }
        public void SetSkipMode(bool isOn) { if (skipToggle != null) skipToggle.isOn = isOn; }

        public string CurrentSpeaker => speakerText != null ? speakerText.text : string.Empty;
        public string CurrentText => isPrologueMode && prologueText != null ? prologueText.text : typer != null ? typer.CurrentText : string.Empty;

        private void Awake()
        {
            ResolveNameInputPanel();

            VNKoreanFontFallback.ApplyToAllIn(gameObject);
            if (prologueRoot != null) VNKoreanFontFallback.ApplyToAllIn(prologueRoot);

            if (advanceButton != null) advanceButton.onClick.AddListener(OnAdvancePressed);
            if (prologueAdvanceButton != null) prologueAdvanceButton.onClick.AddListener(OnAdvancePressed);
            if (backlogButton != null) backlogButton.onClick.AddListener(ToggleBacklog);

            if (choicePanel != null) choicePanel.SetVisible(false);
            if (prologueRoot != null) prologueRoot.SetActive(false);
            if (saveLoadPanel != null) saveLoadPanel.Close();
            if (optionPanel != null) optionPanel.Close();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (saveLoadPanel != null && saveLoadPanel.gameObject.activeSelf)
                {
                    saveLoadPanel.Close();
                }
                else if (optionPanel != null)
                {
                    if (optionPanel.IsOpen) optionPanel.Close();
                    else optionPanel.Open();
                }
            }
        }

        private void OnAdvancePressed()
        {
            if (typer != null && typer.IsTyping) typer.Skip();
            else advanceRequested = true;
        }

        public void SetSpeaker(string speaker)
        {
            if (isPrologueMode) return;
            if (speakerText != null) speakerText.text = speaker ?? string.Empty;
        }

        public void RestoreState(string speaker, string text)
        {
            SetSpeaker(speaker);
            if (typer != null) typer.ShowImmediate(text);
        }

        public IEnumerator PresentLine(string text)
        {
            if (isPrologueMode)
            {
                yield return PresentPrologueLine(text);
                yield break;
            }

            if (typer == null) yield break;
            yield return typer.TypeText(text, SkipMode);
            
            // 스킵 모드일 때 대사가 출력되자마자 사라지는 것을 방지하기 위한 최소한의 찰나 대기
            if (SkipMode) yield return new WaitForSecondsRealtime(0.03f);
        }

        public void SetAutoWaitMultiplier(float multiplier) => autoWaitMultiplier = Mathf.Max(0.1f, multiplier);
        public void SetPrologueTextFadeSeconds(float seconds) => prologueTextFadeSeconds = Mathf.Max(0f, seconds);

        public IEnumerator FadeScreen(bool fadeOut, float seconds, Color color)
        {
            EnsureScreenFadeImage();
            if (screenFadeImage == null) yield break;

            if (screenFadeCoroutine != null)
            {
                StopCoroutine(screenFadeCoroutine);
                screenFadeCoroutine = null;
            }

            float duration = SkipMode ? 0.02f : Mathf.Max(0f, seconds);
            float from = screenFadeImage.color.a;
            float to = fadeOut ? 1f : 0f;

            screenFadeImage.gameObject.SetActive(true);
            screenFadeImage.raycastTarget = false;

            if (duration <= 0f)
            {
                SetScreenFadeColor(color, to);
                if (!fadeOut) screenFadeImage.gameObject.SetActive(false);
                yield break;
            }

            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(from, to, Mathf.Clamp01(timer / duration));
                SetScreenFadeColor(color, alpha);
                yield return null;
            }

            SetScreenFadeColor(color, to);
            if (!fadeOut) screenFadeImage.gameObject.SetActive(false);
        }

        public IEnumerator WaitForAdvanceOrAuto(int lineCharCount)
        {
            advanceRequested = false;
            
            // 지능형 대기 시간 계산
            float autoWait = Mathf.Max(0.05f, (autoBaseWait + lineCharCount * autoPerCharWait) * autoWaitMultiplier);
            float timer = 0f;

            // 루프 조건에 !optionPanel.IsOpen을 추가하여 메뉴가 열리면 대기하게 함
            while (!advanceRequested && (!AutoMode || timer < autoWait) && !SkipMode)
            {
                // 옵션 패널이 열려있지 않을 때만 타이머를 올림
                if (optionPanel == null || !optionPanel.IsOpen)
                {
                    timer += Time.unscaledDeltaTime;
                }
                yield return null;
            }

            // 스킵 모드일 때도 메뉴가 열려있으면 대기
            while (SkipMode && optionPanel != null && optionPanel.IsOpen)
            {
                yield return null;
            }

            // 스킵 모드일 때의 아주 짧은 시각적 대기
            if (SkipMode)
            {
                yield return new WaitForSecondsRealtime(0.05f);
            }
        }

        public void AddBacklog(string speaker, string line) => backlogManager?.AddEntry(speaker, line);
        public void ToggleBacklog() => backlogManager?.ToggleBacklog();
        
        // 백로그 데이터 교환
        public List<BacklogData> GetBacklogData() => backlogManager?.GetBacklogData() ?? new List<BacklogData>();
        public void ClearAndRestoreBacklog(List<BacklogData> data) => backlogManager?.ClearAndRestore(data);

        public void OpenSaveLoad(VNSaveLoadPanel.PanelMode mode)
        {
            if (saveLoadPanel != null) saveLoadPanel.Open(mode);
        }

        public IEnumerator RequestNameInput(string defaultName, Action<string> onConfirm)
        {
            ResolveNameInputPanel();

            if (nameInputPanel == null)
            {
                Debug.LogError("[VNUIController] Name input panel is not assigned. Run VN Tools/Setup Name Input UI or assign it in the Inspector.");
                onConfirm?.Invoke(string.IsNullOrWhiteSpace(defaultName) ? "지휘사" : defaultName.Trim());
                yield break;
            }

            bool done = false;
            nameInputPanel.Open(defaultName, value =>
            {
                done = true;
                onConfirm?.Invoke(value);
            });

            while (!done) yield return null;
        }

        private void ResolveNameInputPanel()
        {
            if (nameInputPanel != null) return;
            nameInputPanel = FindFirstObjectByType<VNNameInputPanel>(FindObjectsInactive.Include);
        }

        public void ShowChoices(IReadOnlyList<Choice> choices, Action<int> onSelect)
        {
            if (isPrologueMode)
            {
                ShowPrologueChoices(choices, onSelect);
                return;
            }

            if (choicePanel == null) return;
            if (advanceButton != null)
            {
                advanceButtonPrevActive = advanceButton.gameObject.activeSelf;
                advanceButton.gameObject.SetActive(false);
            }

            choicePanel.Show(choices, index =>
            {
                if (advanceButton != null) advanceButton.gameObject.SetActive(advanceButtonPrevActive);
                onSelect?.Invoke(index);
            });
        }

        public void SetPresentationMode(string mode)
        {
            bool shouldUsePrologue = string.Equals(mode, "prologue", StringComparison.OrdinalIgnoreCase);
            if (shouldUsePrologue)
            {
                if (!HasPrologueUI())
                {
                    Debug.LogError("[VNUIController] Prologue UI is not assigned. Run VN Tools/Setup Prologue UI or assign the scene objects in the Inspector.");
                    return;
                }

                isPrologueMode = true;
                ClearPrologueText();
                if (prologueRoot != null) prologueRoot.SetActive(true);
                if (choicePanel != null) choicePanel.SetVisible(false);
                if (speakerText != null) speakerText.gameObject.SetActive(false);
                if (typer != null && typer.TextComponent != null) typer.TextComponent.gameObject.SetActive(false);
                return;
            }

            isPrologueMode = false;
            ClearPrologueChoices();
            if (prologueRoot != null) prologueRoot.SetActive(false);
            if (speakerText != null) speakerText.gameObject.SetActive(true);
            if (typer != null && typer.TextComponent != null) typer.TextComponent.gameObject.SetActive(true);
        }

        public void ClearPrologueText()
        {
            prologueLines.Clear();
            if (prologueText != null)
            {
                prologueText.text = string.Empty;
                prologueText.maxVisibleCharacters = int.MaxValue;
            }
        }

        private IEnumerator PresentPrologueLine(string text)
        {
            if (prologueText == null) yield break;

            if (!string.IsNullOrWhiteSpace(text))
            {
                prologueLines.Add(text.Trim());
            }

            string fullText = string.Join("\n\n", prologueLines);

            if (SkipMode || prologueTextFadeSeconds <= 0f)
            {
                prologueText.text = fullText;
                prologueText.maxVisibleCharacters = int.MaxValue;
                yield break;
            }

            int lastIndex = prologueLines.Count - 1;
            float fadeTimer = 0f;
            while (fadeTimer < prologueTextFadeSeconds)
            {
                fadeTimer += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(fadeTimer / prologueTextFadeSeconds);
                prologueText.text = BuildPrologueTextWithFadingLine(lastIndex, alpha);
                prologueText.maxVisibleCharacters = int.MaxValue;
                yield return null;
            }

            prologueText.text = fullText;
            prologueText.ForceMeshUpdate();

            if (!prologueTextUsesTypewriter)
            {
                prologueText.maxVisibleCharacters = int.MaxValue;
                yield break;
            }

            int total = prologueText.textInfo.characterCount;
            int previousVisible = 0;
            for (int i = 0; i < prologueLines.Count - 1; i++)
            {
                previousVisible += prologueLines[i].Length + 2;
            }

            prologueText.maxVisibleCharacters = Mathf.Clamp(previousVisible, 0, total);
            float timer = 0f;
            while (prologueText.maxVisibleCharacters < total)
            {
                timer += Time.unscaledDeltaTime;
                while (timer >= prologueTextSecondsPerChar && prologueText.maxVisibleCharacters < total)
                {
                    timer -= prologueTextSecondsPerChar;
                    prologueText.maxVisibleCharacters++;
                }
                yield return null;
            }

            prologueText.maxVisibleCharacters = int.MaxValue;
        }

        private string BuildPrologueTextWithFadingLine(int fadingLineIndex, float alpha)
        {
            int alphaByte = Mathf.RoundToInt(Mathf.Clamp01(alpha) * 255f);
            string alphaHex = alphaByte.ToString("X2");
            var lines = new List<string>(prologueLines.Count);

            for (int i = 0; i < prologueLines.Count; i++)
            {
                string line = prologueLines[i];
                if (i == fadingLineIndex)
                {
                    line = $"<alpha=#{alphaHex}>{line}<alpha=#FF>";
                }
                lines.Add(line);
            }

            return string.Join("\n\n", lines);
        }

        private void ShowPrologueChoices(IReadOnlyList<Choice> choices, Action<int> onSelect)
        {
            ClearPrologueChoices();

            if (choices == null || choices.Count == 0) return;
            if (prologueChoiceButtons == null || prologueChoiceButtons.Count < choices.Count)
            {
                Debug.LogError("[VNUIController] Not enough prologue choice buttons are assigned in the scene.");
                return;
            }

            for (int i = 0; i < choices.Count; i++)
            {
                int index = i;
                var button = prologueChoiceButtons[i];
                if (button == null) continue;
                button.gameObject.SetActive(true);

                var label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    VNKoreanFontFallback.ApplyTo(label);
                    label.text = choices[i].text;
                }

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    if (!isPrologueChoiceSelecting)
                    {
                        StartCoroutine(PlayPrologueChoiceSelection(button, index, onSelect));
                    }
                });
            }

            StartPrologueChoiceFade(choices.Count);
        }

        private IEnumerator PlayPrologueChoiceSelection(Button selectedButton, int selectedIndex, Action<int> onSelect)
        {
            isPrologueChoiceSelecting = true;
            StopPrologueChoiceFade();

            var effect = selectedButton.GetComponent<VNChoiceSelectionEffect>();
            if (effect == null) effect = selectedButton.gameObject.AddComponent<VNChoiceSelectionEffect>();

            yield return effect.Play(prologueChoiceButtons);

            ClearPrologueChoices();
            isPrologueChoiceSelecting = false;
            onSelect?.Invoke(selectedIndex);
        }

        private void ClearPrologueChoices()
        {
            StopPrologueChoiceFade();
            isPrologueChoiceSelecting = false;
            if (prologueChoiceButtons == null) return;
            for (int i = 0; i < prologueChoiceButtons.Count; i++)
            {
                if (prologueChoiceButtons[i] != null)
                {
                    prologueChoiceButtons[i].interactable = true;
                    var group = prologueChoiceButtons[i].GetComponent<CanvasGroup>();
                    if (group != null)
                    {
                        group.alpha = 1f;
                        group.interactable = true;
                        group.blocksRaycasts = true;
                    }

                    prologueChoiceButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private void StartPrologueChoiceFade(int visibleCount)
        {
            StopPrologueChoiceFade();
            if (prologueChoiceFadeSeconds <= 0f)
            {
                SetPrologueChoiceAlpha(visibleCount, 1f);
                return;
            }

            SetPrologueChoiceAlpha(visibleCount, 0f);
            prologueChoiceFadeCoroutine = StartCoroutine(FadePrologueChoices(visibleCount));
        }

        private IEnumerator FadePrologueChoices(int visibleCount)
        {
            float timer = 0f;
            while (timer < prologueChoiceFadeSeconds)
            {
                timer += Time.unscaledDeltaTime;
                SetPrologueChoiceAlpha(visibleCount, Mathf.Clamp01(timer / prologueChoiceFadeSeconds));
                yield return null;
            }

            SetPrologueChoiceAlpha(visibleCount, 1f);
            prologueChoiceFadeCoroutine = null;
        }

        private void StopPrologueChoiceFade()
        {
            if (prologueChoiceFadeCoroutine == null) return;
            StopCoroutine(prologueChoiceFadeCoroutine);
            prologueChoiceFadeCoroutine = null;
        }

        private void SetPrologueChoiceAlpha(int visibleCount, float alpha)
        {
            if (prologueChoiceButtons == null) return;
            int count = Mathf.Min(visibleCount, prologueChoiceButtons.Count);
            for (int i = 0; i < count; i++)
            {
                var button = prologueChoiceButtons[i];
                if (button == null) continue;

                var group = button.GetComponent<CanvasGroup>();
                if (group == null) group = button.gameObject.AddComponent<CanvasGroup>();
                group.alpha = alpha;
                group.interactable = alpha >= 1f;
                group.blocksRaycasts = alpha >= 1f;
            }
        }

        private bool HasPrologueUI()
        {
            return prologueRoot != null && prologueText != null && prologueChoiceButtons != null && prologueChoiceButtons.Count > 0;
        }

        private void EnsureScreenFadeImage()
        {
            if (screenFadeImage != null) return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var fadeObject = new GameObject("ScreenFadeOverlay", typeof(RectTransform), typeof(Image));
            fadeObject.transform.SetParent(canvas.transform, false);
            fadeObject.transform.SetAsLastSibling();

            var rect = fadeObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            screenFadeImage = fadeObject.GetComponent<Image>();
            screenFadeImage.color = new Color(0f, 0f, 0f, 0f);
            screenFadeImage.raycastTarget = false;
            screenFadeImage.gameObject.SetActive(false);
        }

        private void SetScreenFadeColor(Color color, float alpha)
        {
            color.a = alpha;
            screenFadeImage.color = color;
        }
    }
}
