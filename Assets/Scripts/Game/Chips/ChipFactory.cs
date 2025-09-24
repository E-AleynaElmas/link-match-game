using LinkMatch.Core.Pooling;
using UnityEngine;

namespace LinkMatch.Game.Chips
{
    public sealed class ChipFactory : IChipFactory
    {
        private readonly ComponentPool<Chip> _pool;
        private readonly ChipPalette _palette;

        public ChipFactory(Chip chipPrefab, Transform parent, ChipPalette palette, int prewarm = 0, int maxSize = 0)
        {
            _pool = new ComponentPool<Chip>(chipPrefab, parent, maxSize);
            _palette = palette;
            if (prewarm > 0) _pool.Prewarm(prewarm);
        }

        public Chip Create(Vector3 worldPos, ChipType type)
        {
            var chip = _pool.Get();
            chip.transform.position = worldPos;
            chip.SetType(type, _palette.GetSprite(type));
            return chip;
        }

        public void Despawn(Chip chip) => _pool.Return(chip);
    }
}