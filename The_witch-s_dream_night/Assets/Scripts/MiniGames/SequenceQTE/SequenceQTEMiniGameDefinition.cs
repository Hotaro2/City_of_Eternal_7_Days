using System.Collections.Generic;
using UnityEngine;

namespace VN.MiniGames
{
    public enum SequenceQTEKeyState
    {
        Pending,
        Current,
        Complete,
        Failed
    }

    [CreateAssetMenu(fileName = "SequenceQTE_MiniGame", menuName = "VN/MiniGames/Sequence QTE Definition")]
    public sealed class SequenceQTEMiniGameDefinition : MiniGameDefinition
    {
        public const string TypeId = "sequence_qte";

        [Header("Rules")]
        [SerializeField] private List<KeyCode> inputPool = new() { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.Space };
        [SerializeField] private int sequenceLength = 5;
        [SerializeField] private int roundCount = 2;
        [SerializeField] private float timeLimitSeconds = 15f;
        [SerializeField] private int mistakeLimit = 2;
        [SerializeField] private int successScore = 200;
        [SerializeField] private int failScore;
        [SerializeField] private string successRank = "Good";
        [SerializeField] private string failRank = "Fail";
        [SerializeField] private string successFlag;
        [SerializeField] private string failFlag;

        [Header("Presentation")]
        [TextArea]
        [SerializeField] private string promptText = "Enter the sequence!";
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private Sprite characterSprite;
        [SerializeField] private Sprite keySlotSprite;
        [SerializeField] private Sprite currentKeySlotSprite;
        [SerializeField] private Sprite completeKeySlotSprite;
        [SerializeField] private Sprite failedKeySlotSprite;
        [SerializeField] private Sprite upKeySprite;
        [SerializeField] private Sprite downKeySprite;
        [SerializeField] private Sprite leftKeySprite;
        [SerializeField] private Sprite rightKeySprite;
        [SerializeField] private Sprite spaceKeySprite;
        [SerializeField] private Sprite currentUpKeySprite;
        [SerializeField] private Sprite currentDownKeySprite;
        [SerializeField] private Sprite currentLeftKeySprite;
        [SerializeField] private Sprite currentRightKeySprite;
        [SerializeField] private Sprite currentSpaceKeySprite;
        [SerializeField] private Sprite completeUpKeySprite;
        [SerializeField] private Sprite completeDownKeySprite;
        [SerializeField] private Sprite completeLeftKeySprite;
        [SerializeField] private Sprite completeRightKeySprite;
        [SerializeField] private Sprite completeSpaceKeySprite;
        [SerializeField] private Sprite failedUpKeySprite;
        [SerializeField] private Sprite failedDownKeySprite;
        [SerializeField] private Sprite failedLeftKeySprite;
        [SerializeField] private Sprite failedRightKeySprite;
        [SerializeField] private Sprite failedSpaceKeySprite;
        [SerializeField] private Sprite successSprite;
        [SerializeField] private Sprite failSprite;
        [SerializeField] private AudioClip correctSfx;
        [SerializeField] private AudioClip wrongSfx;
        [SerializeField] private AudioClip successSfx;
        [SerializeField] private AudioClip failSfx;

        public override string MiniGameType => TypeId;
        public IReadOnlyList<KeyCode> InputPool => inputPool;
        public int SequenceLength => Mathf.Clamp(sequenceLength, 1, 12);
        public int RoundCount => Mathf.Clamp(roundCount, 1, 10);
        public float TimeLimitSeconds => Mathf.Max(0.5f, timeLimitSeconds);
        public int MistakeLimit => Mathf.Max(0, mistakeLimit);
        public int SuccessScore => successScore;
        public int FailScore => failScore;
        public string SuccessRank => successRank;
        public string FailRank => failRank;
        public string SuccessFlag => successFlag;
        public string FailFlag => failFlag;
        public string PromptText => promptText;
        public Sprite BackgroundSprite => backgroundSprite;
        public Sprite CharacterSprite => characterSprite;
        public Sprite KeySlotSprite => keySlotSprite;
        public Sprite CurrentKeySlotSprite => currentKeySlotSprite;
        public Sprite CompleteKeySlotSprite => completeKeySlotSprite;
        public Sprite FailedKeySlotSprite => failedKeySlotSprite;
        public Sprite SuccessSprite => successSprite;
        public Sprite FailSprite => failSprite;
        public AudioClip CorrectSfx => correctSfx;
        public AudioClip WrongSfx => wrongSfx;
        public AudioClip SuccessSfx => successSfx;
        public AudioClip FailSfx => failSfx;

        public Sprite GetKeySprite(KeyCode key)
        {
            return GetKeySprite(key, SequenceQTEKeyState.Pending);
        }

        public Sprite GetKeySprite(KeyCode key, SequenceQTEKeyState state)
        {
            return state switch
            {
                SequenceQTEKeyState.Current => GetCurrentKeySprite(key) ?? GetPendingKeySprite(key),
                SequenceQTEKeyState.Complete => GetCompleteKeySprite(key) ?? GetPendingKeySprite(key),
                SequenceQTEKeyState.Failed => GetFailedKeySprite(key) ?? GetPendingKeySprite(key),
                _ => GetPendingKeySprite(key)
            };
        }

        private Sprite GetPendingKeySprite(KeyCode key)
        {
            return key switch
            {
                KeyCode.UpArrow => upKeySprite,
                KeyCode.DownArrow => downKeySprite,
                KeyCode.LeftArrow => leftKeySprite,
                KeyCode.RightArrow => rightKeySprite,
                KeyCode.Space => spaceKeySprite,
                _ => null
            };
        }

        private Sprite GetCurrentKeySprite(KeyCode key)
        {
            return key switch
            {
                KeyCode.UpArrow => currentUpKeySprite,
                KeyCode.DownArrow => currentDownKeySprite,
                KeyCode.LeftArrow => currentLeftKeySprite,
                KeyCode.RightArrow => currentRightKeySprite,
                KeyCode.Space => currentSpaceKeySprite,
                _ => null
            };
        }

        private Sprite GetCompleteKeySprite(KeyCode key)
        {
            return key switch
            {
                KeyCode.UpArrow => completeUpKeySprite,
                KeyCode.DownArrow => completeDownKeySprite,
                KeyCode.LeftArrow => completeLeftKeySprite,
                KeyCode.RightArrow => completeRightKeySprite,
                KeyCode.Space => completeSpaceKeySprite,
                _ => null
            };
        }

        private Sprite GetFailedKeySprite(KeyCode key)
        {
            return key switch
            {
                KeyCode.UpArrow => failedUpKeySprite,
                KeyCode.DownArrow => failedDownKeySprite,
                KeyCode.LeftArrow => failedLeftKeySprite,
                KeyCode.RightArrow => failedRightKeySprite,
                KeyCode.Space => failedSpaceKeySprite,
                _ => null
            };
        }
    }
}
