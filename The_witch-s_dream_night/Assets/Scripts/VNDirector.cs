using System.Collections;
using System.Collections.Generic;
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
                var allMono = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
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

                    if (!string.IsNullOrWhiteSpace(line.Speaker))
                        ui.SetSpeaker(line.Speaker);

                    yield return ui.PresentLine(line.Text);
                    ui.AddBacklog(line.Speaker, line.Text);
                    yield return ui.WaitForAdvanceOrAuto(line.Text.Length);
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
    }
}
