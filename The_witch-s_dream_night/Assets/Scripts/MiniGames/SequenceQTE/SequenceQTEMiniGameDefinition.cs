using System.Collections.Generic;
using UnityEngine;

namespace VN.MiniGames
{
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
        public AudioClip CorrectSfx => correctSfx;
        public AudioClip WrongSfx => wrongSfx;
        public AudioClip SuccessSfx => successSfx;
        public AudioClip FailSfx => failSfx;
    }
}
