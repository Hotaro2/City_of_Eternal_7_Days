using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using VN.MiniGames;

namespace VN
{
    public sealed class VNDirector : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private VNUIController ui;
        [SerializeField] private MonoBehaviour presenterComponent;
        [SerializeField] private MiniGameManager miniGameManager;

        private InkStoryEngine engine;
        private VNCommandProcessor commandProcessor;
        private bool isPlaying;
        private bool skipNextContinue; // 로드 직후 첫 대사 진행을 막는 플래그
        private bool playerNameConfirmed;
        private const string PlayerNameVariable = "player_name";
        private const string DefaultPlayerName = "지휘사";
        private const string PlayerNamePrefsKey = "VNPlayerName";

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

            if (miniGameManager == null)
                miniGameManager = FindFirstObjectByType<MiniGameManager>(FindObjectsInactive.Include);

            string savedPlayerName = PlayerPrefs.GetString(PlayerNamePrefsKey, string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(savedPlayerName)
                && engine.TrySetVariable(PlayerNameVariable, savedPlayerName))
            {
                playerNameConfirmed = true;
            }
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
                playerNameConfirmed = this.playerNameConfirmed,
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
            playerNameConfirmed = data.playerNameConfirmed || HasCustomPlayerName();

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
                    yield return ProcessMiniGameTags(line.Tags);

                    string displaySpeaker = ResolveDisplaySpeaker(line.Speaker);
                    if (!string.IsNullOrWhiteSpace(displaySpeaker))
                        ui.SetSpeaker(displaySpeaker);

                    yield return ui.PresentLine(line.Text);
                    ui.AddBacklog(displaySpeaker, line.Text);
                    yield return ProcessNameInputTags(line.Tags);
                    yield return ui.WaitForAdvanceOrAuto(line.Text.Length);
                    yield return ProcessUITags(line.Tags, true);
                    if (ProcessSceneTags(line.Tags))
                        yield break;
                }

                var choices = engine.GetCurrentChoices();
                if (choices != null && choices.Count > 0)
                {
                    int selected = -1;
                    ui.ShowChoices(choices, idx => selected = idx);
                    while (selected < 0) yield return null;

                    ui.AddBacklog(ResolveDisplaySpeaker("Player"), choices[selected].text);
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

                if (tag.StartsWith("prologueTheme", StringComparison.OrdinalIgnoreCase)
                    && TryReadStringTagValue(tag, out string theme))
                {
                    ui.SetPrologueTheme(theme);
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
                    if (string.Equals(variableName, PlayerNameVariable, StringComparison.OrdinalIgnoreCase))
                    {
                        playerNameConfirmed = true;
                        PlayerPrefs.SetString(PlayerNamePrefsKey, confirmedName);
                        PlayerPrefs.Save();
                    }
                }
            }
        }

        private bool ProcessSceneTags(IReadOnlyList<string> tags)
        {
            if (tags == null) return false;

            for (int i = 0; i < tags.Count; i++)
            {
                string tag = tags[i];
                if (string.IsNullOrWhiteSpace(tag)) continue;

                tag = tag.Trim();
                if (tag.StartsWith("#")) tag = tag.Substring(1).Trim();

                if (!tag.StartsWith("scene", StringComparison.OrdinalIgnoreCase)) continue;
                if (!TryReadStringTagValue(tag, out string sceneName)) continue;

                isPlaying = false;
                SceneManager.LoadScene(sceneName);
                return true;
            }

            return false;
        }

        private IEnumerator ProcessMiniGameTags(IReadOnlyList<string> tags)
        {
            if (tags == null) yield break;

            for (int i = 0; i < tags.Count; i++)
            {
                string tag = tags[i];
                if (string.IsNullOrWhiteSpace(tag)) continue;

                tag = tag.Trim();
                if (tag.StartsWith("#")) tag = tag.Substring(1).Trim();

                if (!tag.StartsWith("minigame", StringComparison.OrdinalIgnoreCase)) continue;
                if (!TryReadStringTagValue(tag, out string miniGameId)) continue;

                if (miniGameManager == null)
                    miniGameManager = FindFirstObjectByType<MiniGameManager>(FindObjectsInactive.Include);

                if (miniGameManager == null)
                {
                    Debug.LogError($"[VNDirector] MiniGameManager is missing. Cannot run mini game: {miniGameId}. Open this scene and run VN Tools > MiniGames > Install Overlay In Active Scene.");
                    continue;
                }

                bool completed = false;
                MiniGameResult result = null;
                bool started = miniGameManager.StartMiniGame(miniGameId, miniGameResult =>
                {
                    result = miniGameResult;
                    completed = true;
                });

                if (!started)
                {
                    Debug.LogWarning($"[VNDirector] Mini game could not start: {miniGameId}");
                    continue;
                }

                while (!completed)
                    yield return null;

                ApplyMiniGameResultToInk(result);
            }
        }

        private void ApplyMiniGameResultToInk(MiniGameResult result)
        {
            if (result == null || engine == null || !engine.IsInitialized) return;

            string baseVariable = !string.IsNullOrWhiteSpace(result.resultVariableName)
                ? result.resultVariableName.Trim()
                : $"{result.miniGameId}_success";

            engine.TrySetVariable(baseVariable, result.isSuccess);
            engine.TrySetVariable($"{baseVariable}_score", result.score);
            engine.TrySetVariable($"{baseVariable}_rank", result.rank ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(result.gainedFlag))
                engine.TrySetVariable(result.gainedFlag, true);

            if (result.variableChanges == null) return;
            for (int i = 0; i < result.variableChanges.Count; i++)
                ApplyMiniGameVariableChange(result.variableChanges[i]);
        }

        private void ApplyMiniGameVariableChange(MiniGameInkVariableChange change)
        {
            if (change == null || engine == null || !engine.IsInitialized) return;
            if (string.IsNullOrWhiteSpace(change.VariableName)) return;

            string variableName = change.VariableName.Trim();
            if (change.Operation == MiniGameVariableOperation.Add)
            {
                ApplyMiniGameVariableAdd(variableName, change);
                return;
            }

            engine.TrySetVariable(variableName, change.GetSetValue());
        }

        private void ApplyMiniGameVariableAdd(string variableName, MiniGameInkVariableChange change)
        {
            if (change.ValueType != MiniGameVariableValueType.Int
                && change.ValueType != MiniGameVariableValueType.Float)
            {
                Debug.LogWarning($"[VNDirector] Add operation supports only Int or Float values: {variableName}");
                return;
            }

            object current = null;
            engine.TryGetVariable(variableName, out current);

            if (change.ValueType == MiniGameVariableValueType.Int)
            {
                int currentValue = ConvertToInt(current);
                engine.TrySetVariable(variableName, currentValue + change.IntValue);
                return;
            }

            float currentFloat = ConvertToFloat(current);
            engine.TrySetVariable(variableName, currentFloat + change.FloatValue);
        }

        private static int ConvertToInt(object value)
        {
            return value switch
            {
                int intValue => intValue,
                float floatValue => Mathf.RoundToInt(floatValue),
                double doubleValue => (int)Math.Round(doubleValue),
                bool boolValue => boolValue ? 1 : 0,
                string stringValue when int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) => parsed,
                _ => 0
            };
        }

        private static float ConvertToFloat(object value)
        {
            return value switch
            {
                float floatValue => floatValue,
                double doubleValue => (float)doubleValue,
                int intValue => intValue,
                bool boolValue => boolValue ? 1f : 0f,
                string stringValue when float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) => parsed,
                _ => 0f
            };
        }

        private string ResolveDisplaySpeaker(string speaker)
        {
            if (string.IsNullOrWhiteSpace(speaker)) return string.Empty;
            if (!IsPlayerSpeakerAlias(speaker)) return speaker;
            if (!playerNameConfirmed) return speaker;

            string playerName = engine.GetVariableString(PlayerNameVariable);
            return string.IsNullOrWhiteSpace(playerName) ? DefaultPlayerName : playerName.Trim();
        }

        private static bool IsPlayerSpeakerAlias(string speaker)
        {
            return speaker.Equals(DefaultPlayerName, StringComparison.OrdinalIgnoreCase)
                || speaker.Equals("나", StringComparison.OrdinalIgnoreCase)
                || speaker.Equals("Player", StringComparison.OrdinalIgnoreCase);
        }

        private bool HasCustomPlayerName()
        {
            if (engine == null || !engine.IsInitialized) return false;

            string playerName = engine.GetVariableString(PlayerNameVariable);
            return !string.IsNullOrWhiteSpace(playerName)
                && !playerName.Trim().Equals(DefaultPlayerName, StringComparison.OrdinalIgnoreCase);
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

        private static bool TryReadStringTagValue(string tag, out string value)
        {
            value = null;
            int colon = tag.IndexOf(':');

            if (colon >= 0)
            {
                value = tag.Substring(colon + 1).Trim();
            }
            else
            {
                var parts = tag.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2) value = parts[1].Trim();
            }

            return !string.IsNullOrWhiteSpace(value);
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
