using System;
using System.Collections.Generic;

namespace VN.MiniGames
{
    [Serializable]
    public sealed class MiniGameResult
    {
        public string miniGameId;
        public bool isSuccess;
        public int score;
        public string rank;
        public string gainedFlag;
        public string resultVariableName;
        public List<MiniGameInkVariableChange> variableChanges = new();
    }
}
