using System;

namespace VN.MiniGames
{
    public interface IMiniGameModule
    {
        string MiniGameType { get; }
        bool IsRunning { get; }

        void StartGame(MiniGameDefinition definition, Action<MiniGameResult> onComplete);
        void StopGame();
    }
}
