using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using UnityEngine;

namespace VN
{
    public sealed class VNDirector : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private VNUIController ui;
        [SerializeField] private MonoBehaviour presenterComponent;

        private InkStoryEngine engine;
        private VNCommandProcessor commandProcessor;
        private bool isPlaying;
        private bool skipNextContinue; // 로드 직후 첫 대사 진행을 막는 플래그

        public string CurrentChapter { get; set; } = "Chapter 1";
        public float PlayTime { get; private set; }

        public void Initialize(InkStoryEngine storyEngine)
        {
            engine = storyEngine;
            IVNPresenter presenter = presenterComponent as IVNPresenter;

            if (presenter == null)
            {
                var allMono = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (var mono in allMono)
                {
                    if (mono is IVNPresenter p)
                    {
                        presenter = p;
                        presenterComponent = mono;
                        break;
                    }
                }
            }

            if (presenter != null)
                commandProcessor = new VNCommandProcessor(presenter);
        }

        private void Update()
        {
            if (isPlaying) PlayTime += Time.deltaTime;
        }

        public void Play(string startKnot = null)
        {
            if (engine == null || !engine.IsInitialized) return;
            if (isPlaying) return;

            if (!string.IsNullOrWhiteSpace(startKnot)) engine.JumpTo(startKnot);
            isPlaying = true;
            StartCoroutine(StoryLoop());
        }

        public void Save(int slot)
        {
            if (engine == null || !engine.IsInitialized) return;
            var presenter = presenterComponent as IVNPresenter;

            VNSaveData data = new VNSaveData
            {
                inkStateJson = engine.GetStateJson(),
                currentBackground = presenter?.CurrentBackground,
                currentBGM = presenter?.CurrentBGM,
                activeCharacters = presenter?.GetCurrentCharacters() ?? new List<VNCharacterState>(),
                lastSpeaker = ui.CurrentSpeaker,
                lastText = ui.CurrentText,
                chapterTitle = CurrentChapter,
                playTime = PlayTime,
                backlog = ui.GetBacklogData() // 백로그 추출
            };

            VNSaveService.Save(slot, data);
            Debug.Log($"[VNDirector] Saved current line: {data.lastText}");
        }

        public void Load(int slot)
        {
            VNSaveData data = VNSaveService.Load(slot);
            if (data == null) return;

            Time.timeScale = 1f;
            StopAllCoroutines();
            isPlaying = false;
            
            // 로드 시점 설정
            engine.LoadStateJson(data.inkStateJson);

            var presenter = presenterComponent as IVNPresenter;
            if (presenter != null)
            {
                presenter.ResetTransientState();
                presenter.ClearAllCharacters();
                // 키가 비어있더라도(초반 세이브) 명시적으로 호출하여 상태를 초기화합니다.
                presenter.SetBackground(data.currentBackground);
                presenter.PlayBGM(data.currentBGM);
                
                foreach (var ch in data.activeCharacters)
                    presenter.SetCharacter(ch.name, ch.expression, ch.position);
            }

            // UI 및 백로그 복구
            ui.RestoreState(data.lastSpeaker, data.lastText);
            ui.ClearAndRestoreBacklog(data.backlog); // 백로그 복구
            
            CurrentChapter = data.chapterTitle;
            PlayTime = data.playTime;

            // 로드 직후 바로 다음 대사로 넘어가지 않도록 설정
            skipNextContinue = true;
            isPlaying = true;
            StartCoroutine(StoryLoop());
        }

        private IEnumerator StoryLoop()
        {
            while (isPlaying)
            {
                // 스킵 모드 상태 동기화
                var presenter = presenterComponent as IVNPresenter;
                if (presenter != null) presenter.IsSkipMode = ui.SkipMode;

                // 로드 직후라면, 복구된 현재 대사가 끝날 때까지 먼저 대기합니다.
                if (skipNextContinue)
                {
                    skipNextContinue = false;
                    // 현재 UI에 복구된 텍스트 길이를 기준으로 대기
                    yield return ui.WaitForAdvanceOrAuto(ui.CurrentText.Length);
                }

                while (engine.CanContinue())
                {
                    var line = engine.ContinueLine();

                    if (commandProcessor != null)
                        commandProcessor.Process(line.Tags);

                    yield return ProcessUITags(line.Tags, false);

                    if (!string.IsNullOrWhiteSpace(line.Speaker))
                        ui.SetSpeaker(line.Speaker);

                    yield return ui.PresentLine(line.Text);
                    ui.AddBacklog(line.Speaker, line.Text);
                    yield return ProcessNameInputTags(line.Tags);
                    yield return ui.WaitForAdvanceOrAuto(line.Text.Length);
                    yield return ProcessUITags(line.Tags, true);
                }

                var choices = engine.GetCurrentChoices();
                if (choices != null && choices.Count > 0)
                {
                    int selected = -1;
                    ui.ShowChoices(choices, idx => selected = idx);
                    while (selected < 0) yield return null;

                    ui.AddBacklog("Player", choices[selected].text);
                    engine.ChooseChoiceIndex(selected);
                    continue;
                }
                break;
            }
            isPlaying = false;
        }

        private IEnumerator ProcessUITags(IReadOnlyList<string> tags, bool afterLine)
        {
            if (ui == null || tags == null) yield break;

            for (int i = 0; i < tags.Count; i++)
            {
                string tag = tags[i];
                if (string.IsNullOrWhiteSpace(tag)) continue;

                tag = tag.Trim();
                if (tag.StartsWith("#")) tag = tag.Substring(1).Trim();

                bool shouldProcessAfterLine = tag.StartsWith("afterFadeOut", StringComparison.OrdinalIgnoreCase)
                    || tag.StartsWith("afterFadeIn", StringComparison.OrdinalIgnoreCase);
                if (afterLine != shouldProcessAfterLine) continue;

                if (tag.Equals("clear", StringComparison.OrdinalIgnoreCase))
                {
                    ui.ClearPrologueText();
                    continue;
                }

                if (tag.StartsWith("mode", StringComparison.OrdinalIgnoreCase))
                {
                    int colon = tag.IndexOf(':');
                    if (colon >= 0)
                    {
                        ui.SetPresentationMode(tag.Substring(colon + 1).Trim());
                        continue;
                    }

                    var parts = tag.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        ui.SetPresentationMode(parts[1].Trim());
                    }
                }

                if (tag.StartsWith("textFade", StringComparison.OrdinalIgnoreCase)
                    && TryReadFloatTagValue(tag, out float fadeSeconds))
                {
                    ui.SetPrologueTextFadeSeconds(fadeSeconds);
                }

                if (tag.StartsWith("fadeOut", StringComparison.OrdinalIgnoreCase)
                    && TryReadFadeTag(tag, out float fadeOutSeconds, out Color fadeOutColor))
                {
                    yield return ui.FadeScreen(true, fadeOutSeconds, fadeOutColor);
                }

                if (tag.StartsWith("fadeIn", StringComparison.OrdinalIgnoreCase)
                    && TryReadFadeTag(tag, out float fadeInSeconds, out Color fadeInColor))
                {
                    yield return ui.FadeScreen(false, fadeInSeconds, fadeInColor);
                }

                if (tag.StartsWith("afterFadeOut", StringComparison.OrdinalIgnoreCase)
                    && TryReadFadeTag(tag, out float afterFadeOutSeconds, out Color afterFadeOutColor))
                {
                    yield return ui.FadeScreen(true, afterFadeOutSeconds, afterFadeOutColor);
                }

                if (tag.StartsWith("afterFadeIn", StringComparison.OrdinalIgnoreCase)
                    && TryReadFadeTag(tag, out float afterFadeInSeconds, out Color afterFadeInColor))
                {
                    yield return ui.FadeScreen(false, afterFadeInSeconds, afterFadeInColor);
                }

            }
        }

        private IEnumerator ProcessNameInputTags(IReadOnlyList<string> tags)
        {
            if (ui == null || tags == null) yield break;

            for (int i = 0; i < tags.Count; i++)
            {
                string tag = tags[i];
                if (string.IsNullOrWhiteSpace(tag)) continue;

                tag = tag.Trim();
                if (tag.StartsWith("#")) tag = tag.Substring(1).Trim();

                if (tag.StartsWith("nameInput", StringComparison.OrdinalIgnoreCase)
                    && TryReadNameInputTag(tag, out string variableName, out string defaultName))
                {
                    string confirmedName = defaultName;
                    yield return ui.RequestNameInput(defaultName, value => confirmedName = value);
                    engine.SetVariable(variableName, confirmedName);
                }
            }
        }

        private static bool TryReadFloatTagValue(string tag, out float value)
        {
            value = 0f;
            int colon = tag.IndexOf(':');
            string rawValue = null;

            if (colon >= 0)
            {
                rawValue = tag.Substring(colon + 1).Trim();
            }
            else
            {
                var parts = tag.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2) rawValue = parts[1].Trim();
            }

            return !string.IsNullOrWhiteSpace(rawValue)
                && float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryReadFadeTag(string tag, out float seconds, out Color color)
        {
            seconds = 0.5f;
            color = Color.black;

            var parts = tag.Split(new[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out seconds);
            }

            if (parts.Length >= 3)
            {
                color = ParseFadeColor(parts[2]);
            }

            return true;
        }

        private static Color ParseFadeColor(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return Color.black;

            switch (raw.Trim().ToLowerInvariant())
            {
                case "white": return Color.white;
                case "red": return Color.red;
                case "clear": return Color.clear;
                case "black":
                default:
                    return Color.black;
            }
        }

        private static bool TryReadNameInputTag(string tag, out string variableName, out string defaultName)
        {
            variableName = "player_name";
            defaultName = "지휘사";

            var parts = tag.Split(new[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) variableName = parts[1].Trim();
            if (parts.Length >= 3) defaultName = parts[2].Trim();

            return !string.IsNullOrWhiteSpace(variableName);
        }
    }
}
