using System.Collections.Generic;
using UnityEngine;

namespace LinkMatch.Game.Chips
{
    [CreateAssetMenu(menuName = "LinkMatch/ChipPalette", fileName = "ChipPalette")]
    public class ChipPalette : ScriptableObject
    {
        [SerializeField] private ChipPaletteItem[] items;

        private Dictionary<ChipType, ChipPaletteItem> _itemLookup;

        private void BuildLookupTable()
        {
            if (items == null) return;

            _itemLookup = new Dictionary<ChipType, ChipPaletteItem>(items.Length);
            foreach (var item in items)
            {
                _itemLookup[item.type] = item;
            }
        }

        public Sprite GetSprite(ChipType type)
        {
            EnsureLookupTable();
            return _itemLookup.TryGetValue(type, out var item) ? item.sprite : null;
        }

        public Color GetColor(ChipType type)
        {
            EnsureLookupTable();
            return _itemLookup.TryGetValue(type, out var item) ? item.color : Color.white;
        }

        public ChipPaletteItem? GetItem(ChipType type)
        {
            EnsureLookupTable();
            return _itemLookup.TryGetValue(type, out var item) ? item : null;
        }

        private void EnsureLookupTable()
        {
            if (_itemLookup == null)
                BuildLookupTable();
        }
    }
}