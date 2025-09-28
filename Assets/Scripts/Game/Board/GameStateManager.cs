using LinkMatch.Core.Signals;

namespace LinkMatch.Game.Board
{
    public sealed class GameStateManager
    {
        private int _score;
        private int _movesLeft;
        private readonly LevelConfig _config;

        public int Score => _score;
        public int MovesLeft => _movesLeft;
        public int TargetScore => _config.targetScore;
        public bool IsGameOver => _movesLeft <= 0 || _score >= _config.targetScore;
        public bool HasWon => _score >= _config.targetScore;

        public GameStateManager(LevelConfig config, int initialMovesOverride = -1)
        {
            _config = config;
            _movesLeft = (initialMovesOverride > 0) ? initialMovesOverride : config.initialMoves;
            _score = 0;
        }

        public void Reset(int initialMovesOverride = -1)
        {
            _movesLeft = (initialMovesOverride > 0) ? initialMovesOverride : _config.initialMoves;
            _score = 0;
            NotifyStateChanged();
            GameSignals.TargetScoreChanged(_config.targetScore);
        }

        public bool TryConsumeMove(int pointsGained)
        {
            if (IsGameOver) return false;

            _score += pointsGained;
            _movesLeft--;
            NotifyStateChanged();

            if (IsGameOver)
            {
                GameSignals.GameOver(HasWon);
                return false;
            }

            return true;
        }

        private void NotifyStateChanged()
        {
            GameSignals.ScoreChanged(_score);
            GameSignals.MovesChanged(_movesLeft);
        }
    }
}