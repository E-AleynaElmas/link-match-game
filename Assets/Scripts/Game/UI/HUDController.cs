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
        [SerializeField] private GameObject busyBlocker;

        private void OnEnable()
        {
            GameSignals.OnScoreChanged += OnScore;
            GameSignals.OnMovesChanged += OnMoves;
            GameSignals.OnBoardBusy    += OnBusy;
        }
        private void OnDisable()
        {
            GameSignals.OnScoreChanged -= OnScore;
            GameSignals.OnMovesChanged -= OnMoves;
            GameSignals.OnBoardBusy    -= OnBusy;
        }

        private void OnScore(int v){ if (scoreText) scoreText.text = $"Score: {v}"; }
        private void OnMoves(int v){ if (movesText) movesText.text = $"Moves: {v}"; }
        private void OnBusy(bool b){ if (busyBlocker) busyBlocker.SetActive(b); }
    }
}