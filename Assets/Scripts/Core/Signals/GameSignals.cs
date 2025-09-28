using System;

namespace LinkMatch.Core.Signals
{
    public static class GameSignals
    {
        public static event Action<int> OnScoreChanged;
        public static void ScoreChanged(int v) => OnScoreChanged?.Invoke(v);
        
        public static event Action<int> OnMovesChanged;
        public static void MovesChanged(int v) => OnMovesChanged?.Invoke(v);

        public static event Action<int> OnTargetScoreChanged;
        public static void TargetScoreChanged(int v) => OnTargetScoreChanged?.Invoke(v);

        public static event Action<bool> OnBoardBusy;
        public static void BoardBusy(bool v) => OnBoardBusy?.Invoke(v);

        public static event Action<bool> OnGameOver;
        public static void GameOver(bool win) => OnGameOver?.Invoke(win);

        public static event Action OnGameResetRequested;
        public static void RequestGameReset() => OnGameResetRequested?.Invoke();
    }
}