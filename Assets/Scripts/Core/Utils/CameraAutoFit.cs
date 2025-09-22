using UnityEngine;

namespace LinkMatch.Core.Utils
{
    [RequireComponent(typeof(Camera))]
    public class CameraAutoFit : MonoBehaviour
    {
        [SerializeField] private float paddingWorld = 0.25f; // kenarlardan pay
        [SerializeField] private float minSize = 3f;         // çok küçük gridlerde okunabilirlik için
        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;
            transform.position = new Vector3(0f, 0f, -10f); // dünya merkezini göster
        }

        public void Fit(int rows, int cols, float cellSize)
        {
            // Gridin toplam genişlik/yüksekliği + padding
            float width  = cols * cellSize + 2f * paddingWorld;
            float height = rows * cellSize + 2f * paddingWorld;

            float aspect = (float)Screen.width / Screen.height;
            float sizeByHeight = height * 0.5f;
            float sizeByWidth  = (width * 0.5f) / aspect;

            _cam.orthographicSize = Mathf.Max(minSize, sizeByHeight, sizeByWidth);
        }

        public void RefitLast(int rows, int cols, float cellSize)
        {
            Fit(rows, cols, cellSize);
        }
    }
}