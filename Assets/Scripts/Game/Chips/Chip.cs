using LinkMatch.Core.Pooling;
using UnityEngine;

namespace LinkMatch.Game.Chips
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Chip : MonoBehaviour, IPoolable
    {
        private const float DEFAULT_SELECT_SCALE = 1.1f;

        [field: SerializeField] public ChipType Type { get; private set; }
        [SerializeField] private float scaleOnSelect = DEFAULT_SELECT_SCALE;
        [SerializeField] private Color highlightColor = Color.white;

        private SpriteRenderer _sr;
        private Vector3 _baseScale;
        private Color _baseColor;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _baseScale = transform.localScale;
            _baseColor = _sr.color;
        }

        public void Init(ChipType type, Sprite sprite)
        {
            SetType(type, sprite);
        }

        public void SetSelected(bool selected)
        {
            if (selected)
            {
                // Tek seçili chip: orta scale + normal renk
                transform.localScale = _baseScale * scaleOnSelect;
                _sr.color = highlightColor;
            }
            else
            {
                // Normal chip: varsayılan scale + normal renk
                transform.localScale = _baseScale;
                _sr.color = _baseColor;
            }
        }

        public void SetType(ChipType type, Sprite sprite)
        {
            Type = type;
            _sr.sprite = sprite;
        }

        public void OnSpawned()
        {
            SetSelected(false);
            transform.localScale = Vector3.one;
        }

        public void OnDespawned()
        {
            SetSelected(false);
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}