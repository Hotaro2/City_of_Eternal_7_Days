using System;
using System.Collections.Generic;

namespace VN
{
    [Serializable]
    public struct BacklogData
    {
        public string speaker;
        public string text;
    }

    [Serializable]
    public sealed class VNSaveData
    {
        public string saveDate;
        public string inkStateJson;

        public string chapterTitle;
        public float playTime;
        public string lastText;
        public string lastSpeaker;

        public List<BacklogData> backlog = new List<BacklogData>(); // 백로그 저장

        public string currentBackground;
        public string currentBGM;
        public List<VNCharacterState> activeCharacters = new List<VNCharacterState>();

        public VNSaveData()
        {
            saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
