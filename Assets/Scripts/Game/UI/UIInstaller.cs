using UnityEngine;

namespace LinkMatch.Game.UI
{
    public sealed class UIInstaller : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private HUDController hudPrefab;
        [SerializeField] private LevelEndPanel gameOverPrefab;

        private void Awake()
        {
            if (!canvas)
                canvas = FindFirstObjectByType<Canvas>();

            if (hudPrefab)
                Instantiate(hudPrefab, canvas.transform, false);

            if (gameOverPrefab)
                Instantiate(gameOverPrefab, canvas.transform, false);
        }
    }
}