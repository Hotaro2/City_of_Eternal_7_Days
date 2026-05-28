using UnityEngine;

namespace VN.MiniGames
{
    [CreateAssetMenu(fileName = "QTE_MiniGame", menuName = "VN/MiniGames/QTE Definition")]
    public sealed class QTEMiniGameDefinition : MiniGameDefinition
    {
        public const string TypeId = "qte";

        [Header("Rules")]
        [SerializeField] private KeyCode requiredKey = KeyCode.Space;
        [SerializeField] private float timeLimitSeconds = 5f;
        [SerializeField] private int successScore = 100;
        [SerializeField] private int failScore;
        [SerializeField] private string successRank = "Success";
        [SerializeField] private string failRank = "Fail";
        [SerializeField] private string successFlag;
        [SerializeField] private string failFlag;

        [Header("Presentation")]
        [TextArea]
        [SerializeField] private string promptText = "Press the key!";
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private Sprite requiredKeySprite;
        [SerializeField] private AudioClip successSfx;
        [SerializeField] private AudioClip failSfx;

        public override string MiniGameType => TypeId;
        public KeyCode RequiredKey => requiredKey;
        public float TimeLimitSeconds => Mathf.Max(0.1f, timeLimitSeconds);
        public int SuccessScore => successScore;
        public int FailScore => failScore;
        public string SuccessRank => successRank;
        public string FailRank => failRank;
        public string SuccessFlag => successFlag;
        public string FailFlag => failFlag;
        public string PromptText => promptText;
        public Sprite BackgroundSprite => backgroundSprite;
        public Sprite RequiredKeySprite => requiredKeySprite;
        public AudioClip SuccessSfx => successSfx;
        public AudioClip FailSfx => failSfx;
    }
}
