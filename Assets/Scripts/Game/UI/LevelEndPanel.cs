using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using LinkMatch.Core.Signals;

namespace LinkMatch.Game.UI
{
    public sealed class LevelEndPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text title;
        [SerializeField] private Button replayButton;

        private void Awake()
        {
            if (root) root.SetActive(false);
            if (replayButton) replayButton.onClick.AddListener(() =>
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        }

        private void OnEnable(){ GameSignals.OnGameOver += OnGameOver; }
        private void OnDisable(){ GameSignals.OnGameOver -= OnGameOver; }

        private void OnGameOver(bool win)
        {
            if (root) root.SetActive(true);
            if (title) title.text = win ? "You Win!" : "Try Again";
        }
    }
}