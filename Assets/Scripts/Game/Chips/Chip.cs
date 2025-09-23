using UnityEngine;

namespace LinkMatch.Game.Chips
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Chip : MonoBehaviour
    {
        [field: SerializeField] public ChipType Type { get; private set; }

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
            transform.localScale = on ? _baseScale * 1.1f : _baseScale;
        }

        public void SetType(ChipType type, Sprite sprite)
        {
            Type = type;
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            _sr.sprite = sprite;
        }
    }
}