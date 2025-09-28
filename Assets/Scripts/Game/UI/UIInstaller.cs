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
            if (canvas == null)
            {
                Debug.LogError("Canvas reference is missing in UIInstaller! Please assign it in the inspector.");
                return;
            }

            if (hudPrefab != null)
                Instantiate(hudPrefab, canvas.transform, false);

            if (gameOverPrefab != null)
                Instantiate(gameOverPrefab, canvas.transform, false);
        }
    }
}