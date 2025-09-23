using UnityEngine;

namespace LinkMatch.Game.Chips
{
    public sealed class ChipFactory
    {
        private readonly GameObject _prefab;
        private readonly ChipPalette _palette;

        public ChipFactory(GameObject prefab, ChipPalette palette)
        {
            _prefab = prefab;
            _palette = palette;
        }

        public Chip Create(Transform parent, Vector3 pos, ChipType type)
        {
            var go = Object.Instantiate(_prefab, pos, Quaternion.identity, parent);
            var chip = go.GetComponent<Chip>();
            chip.Init(type, _palette.GetSprite(type));
            return chip;
        }
    }
}