using System.Collections.Generic;
using UnityEngine;

namespace VN.MiniGames
{
    public abstract class MiniGameDefinition : ScriptableObject
    {
        [SerializeField] private string miniGameId;
        [SerializeField] private string resultVariableName;

        [Header("Ink Variable Changes")]
        [SerializeField] private List<MiniGameInkVariableChange> successVariableChanges = new();
        [SerializeField] private List<MiniGameInkVariableChange> failVariableChanges = new();

        public string MiniGameId => miniGameId;
        public string ResultVariableName => resultVariableName;
        public IReadOnlyList<MiniGameInkVariableChange> SuccessVariableChanges => successVariableChanges;
        public IReadOnlyList<MiniGameInkVariableChange> FailVariableChanges => failVariableChanges;
        public abstract string MiniGameType { get; }

        public IReadOnlyList<MiniGameInkVariableChange> GetVariableChanges(bool isSuccess)
        {
            return isSuccess ? successVariableChanges : failVariableChanges;
        }
    }
}
