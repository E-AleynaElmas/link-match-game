using UnityEngine;

namespace LinkMatch.Core.Utils
{
    [RequireComponent(typeof(Camera))]
    public class CameraAutoFit : MonoBehaviour
    {
        [SerializeField] private float paddingWorld = 0.25f; // kenarlardan pay
        [SerializeField] private float minSize = 3f;         // çok küçük gridlerde okunabilirlik için

        [Header("Background")]
        [SerializeField] private SpriteRenderer backgroundSprite;
        [SerializeField] private float backgroundPadding = 2f;

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

            // Background'ı da fit et
            if (backgroundSprite != null)
                FitBackground(rows, cols, cellSize);
        }

        public void RefitLast(int rows, int cols, float cellSize)
        {
            Fit(rows, cols, cellSize);
        }

        private void FitBackground(int rows, int cols, float cellSize)
        {
            // Kamera'nın görüş alanını hesapla (viewport size)
            float cameraHeight = _cam.orthographicSize * 2f;
            float cameraWidth = cameraHeight * _cam.aspect;

            // Extra padding ekle (daha büyük alan kapla)
            float targetWidth = cameraWidth + backgroundPadding;
            float targetHeight = cameraHeight + backgroundPadding;

            // Sprite'ın orijinal boyutlarını al
            var sprite = backgroundSprite.sprite;
            if (sprite == null) return;

            float spriteWidth = sprite.bounds.size.x;
            float spriteHeight = sprite.bounds.size.y;

            // Scale'i hesapla - TAMAMEN kaplayacak şekilde (boşluk yok)
            float scaleX = targetWidth / spriteWidth;
            float scaleY = targetHeight / spriteHeight;
            float scale = Mathf.Max(scaleX, scaleY); // Büyük olanı seç (crop olur ama boşluk kalmaz)

            // Scale'i uygula
            backgroundSprite.transform.localScale = Vector3.one * scale;

            // Position'ı ayarla (center'da, grid'den geride)
            backgroundSprite.transform.position = new Vector3(0f, 0f, 10f);

            // Sorting order'ı ayarla (en arkada)
            backgroundSprite.sortingLayerName = "Background";
            backgroundSprite.sortingOrder = -100;
        }

        // Background'ı manuel olarak yeniden fit etmek için
        public void RefitBackground(int rows, int cols, float cellSize)
        {
            if (backgroundSprite != null)
                FitBackground(rows, cols, cellSize);
        }
    }
}