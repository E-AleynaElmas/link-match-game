using UnityEngine;
using LinkMatch.Core.Signals;
using TMPro;
using UnityEngine.UI;

namespace LinkMatch.Game.UI
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text movesText;
        [SerializeField] private TMP_Text targetScoreText;
        [SerializeField] private GameObject busyBlocker;

        private int _currentScore;
        private int _targetScore;

        private void OnEnable()
        {
            GameSignals.OnScoreChanged += OnScore;
            GameSignals.OnMovesChanged += OnMoves;
            GameSignals.OnTargetScoreChanged += OnTargetScore;
            GameSignals.OnBoardBusy += OnBusy;
        }

        private void OnDisable()
        {
            GameSignals.OnScoreChanged -= OnScore;
            GameSignals.OnMovesChanged -= OnMoves;
            GameSignals.OnTargetScoreChanged -= OnTargetScore;
            GameSignals.OnBoardBusy -= OnBusy;
        }

        private void OnScore(int v)
        {
            _currentScore = v;
            UpdateScoreDisplay();
        }

        private void OnMoves(int v)
        {
            if (movesText)
                movesText.text = $"Moves: {v}";
        }

        private void OnTargetScore(int v)
        {
            _targetScore = v;
            UpdateScoreDisplay();
        }

        private void OnBusy(bool b)
        {
            if (busyBlocker)
                busyBlocker.SetActive(b);
        }

        private void UpdateScoreDisplay()
        {
            if (scoreText != null)
                scoreText.text = $"Score: {_currentScore}";

            if (targetScoreText != null)
                targetScoreText.text = $"Target: {_targetScore}";
        }
    }
}