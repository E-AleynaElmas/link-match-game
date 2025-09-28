using UnityEngine;

namespace LinkMatch.Game.Chips
{
    /// <summary>
    /// Lightweight data structure representing a chip without MonoBehaviour overhead
    /// </summary>
    public class ChipData
    {
        public ChipType Type { get; private set; }
        public Transform Transform { get; private set; }
        public SpriteRenderer SpriteRenderer { get; private set; }
        public Vector3 BaseScale { get; private set; }
        public Color BaseColor { get; private set; }
        public bool IsSelected { get; set; }

        public ChipData(ChipType type, Transform transform, SpriteRenderer spriteRenderer)
        {
            Type = type;
            Transform = transform;
            SpriteRenderer = spriteRenderer;
            BaseScale = transform.localScale;
            BaseColor = spriteRenderer.color;
            IsSelected = false;
        }

        public void SetType(ChipType newType, Sprite sprite)
        {
            UnityEngine.Debug.Log($"ChipData: Changing type from {Type} to {newType}, sprite: {sprite?.name}");
            Type = newType;
            if (SpriteRenderer != null)
                SpriteRenderer.sprite = sprite;
        }

        public bool IsValid => Transform != null && SpriteRenderer != null;
    }
}