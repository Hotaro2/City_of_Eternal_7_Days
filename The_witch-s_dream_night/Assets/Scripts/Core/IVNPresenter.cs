using System;
using System.Collections.Generic;

namespace VN
{
    [Serializable]
    public struct VNCharacterState
    {
        public string name;
        public string expression;
        public string position;
    }

    /// <summary>
    /// Visual presentation interface for the VN system.
    /// </summary>
    public interface IVNPresenter
    {
        string CurrentBackground { get; }
        string CurrentBGM { get; }
        bool IsSkipMode { get; set; }

        void SetBackground(string bgName, string transition = null);
        void SetCharacter(string charName, string expression, string position = null, string transition = null);
        void HideCharacter(string charName, string transition = null);
        void PlayBGM(string clipName, float volume = 1f);
        void PlaySFX(string clipName, float volume = 1f);
        void ClearAllCharacters();
        void ResetTransientState();

        void ShakeScreen(float duration, float strength);

        List<VNCharacterState> GetCurrentCharacters();
    }
}
