using LinkMatch.Core.Pooling;
using UnityEngine;

namespace LinkMatch.Game.Chips
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Chip : MonoBehaviour, IPoolable
    {
        [field: SerializeField] public ChipType Type { get; private set; }
        [SerializeField] private float scaleOnSelect = 1.1f;

        private SpriteRenderer _sr;
        private Vector3 _baseScale;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _baseScale = transform.localScale;
        }

        public void Init(ChipType type, Sprite sprite)
        {
            SetType(type, sprite);
        }

        public void SetSelected(bool on)
        {
            // Basit görsel feedback: scale büyüt/küçült
            transform.localScale = on ? _baseScale * scaleOnSelect : _baseScale;
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