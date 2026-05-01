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

        [Header("Controls")]
        [SerializeField] private Button advanceButton;
        [SerializeField] private Button backlogButton;

        [Header("Auto/Skip Settings")]
        [SerializeField] private float autoBaseWait = 0.8f;
        [SerializeField] private float autoPerCharWait = 0.02f;
        [SerializeField] private float autoWaitMultiplier = 1.0f; // 오토 대기 시간 가중치 (1.0 = 기본)
        [SerializeField] private Toggle autoToggle;
        [SerializeField] private Toggle skipToggle;

        private bool advanceRequested;
        private bool advanceButtonPrevActive;

        public bool AutoMode => autoToggle != null && autoToggle.isOn;
        public bool SkipMode => skipToggle != null && skipToggle.isOn;

        public void SetAutoMode(bool isOn) { if (autoToggle != null) autoToggle.isOn = isOn; }
        public void SetSkipMode(bool isOn) { if (skipToggle != null) skipToggle.isOn = isOn; }

        public string CurrentSpeaker => speakerText != null ? speakerText.text : string.Empty;
        public string CurrentText => typer != null ? typer.CurrentText : string.Empty;

        private void Awake()
        {
            if (advanceButton != null) advanceButton.onClick.AddListener(OnAdvancePressed);
            if (backlogButton != null) backlogButton.onClick.AddListener(ToggleBacklog);

            if (choicePanel != null) choicePanel.SetVisible(false);
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
            if (speakerText != null) speakerText.text = speaker ?? string.Empty;
        }

        public void RestoreState(string speaker, string text)
        {
            SetSpeaker(speaker);
            if (typer != null) typer.ShowImmediate(text);
        }

        public IEnumerator PresentLine(string text)
        {
            if (typer == null) yield break;
            yield return typer.TypeText(text, SkipMode);
            
            // 스킵 모드일 때 대사가 출력되자마자 사라지는 것을 방지하기 위한 최소한의 찰나 대기
            if (SkipMode) yield return new WaitForSecondsRealtime(0.03f);
        }

        public void SetAutoWaitMultiplier(float multiplier) => autoWaitMultiplier = Mathf.Max(0.1f, multiplier);

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

        public void ShowChoices(IReadOnlyList<Choice> choices, Action<int> onSelect)
        {
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
    }
}
