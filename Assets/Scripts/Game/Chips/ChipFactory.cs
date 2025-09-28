using System.Collections.Generic;
using UnityEngine;

namespace LinkMatch.Game.Chips
{
    public sealed class ChipFactory : IChipFactory
    {
        private readonly ChipPalette _palette;
        private readonly ChipManager _chipManager;
        private readonly Transform _parent;
        private readonly Queue<GameObject> _pooledChips = new();

        public ChipFactory(ChipManager chipManager, Transform parent, ChipPalette palette, int prewarm = 0)
        {
            _chipManager = chipManager;
            _parent = parent;
            _palette = palette;

            if (prewarm > 0)
                PrewarmPool(prewarm);
        }

        public GameObject Create(Vector3 worldPos, ChipType type)
        {
            GameObject chipGO;

            if (_pooledChips.Count > 0)
            {
                // Reuse pooled chip
                chipGO = _pooledChips.Dequeue();
                chipGO.SetActive(true);
                chipGO.transform.position = worldPos;
                chipGO.name = $"Chip_{type}"; // Update name to match new type

                // Update existing chip's type and sprite
                _chipManager.SetChipType(chipGO.GetInstanceID(), type, _palette.GetSprite(type));
            }
            else
            {
                // Create new chip
                chipGO = _chipManager.CreateChip(type, _palette.GetSprite(type), worldPos);
                if (_parent != null)
                    chipGO.transform.SetParent(_parent);
            }

            return chipGO;
        }

        public void Despawn(GameObject chipGO)
        {
            if (chipGO == null) return;

            // Reset scale before pooling to prevent scale issues
            chipGO.transform.localScale = Vector3.one;
            chipGO.transform.rotation = Quaternion.identity;

            // Don't unregister, just deactivate and pool
            // ChipManager keeps tracking for reuse
            chipGO.SetActive(false);
            _pooledChips.Enqueue(chipGO);
        }

        private void PrewarmPool(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var chipGO = _chipManager.CreateChip(ChipType.Yellow, _palette.GetSprite(ChipType.Yellow), Vector3.zero);
                if (_parent != null)
                    chipGO.transform.SetParent(_parent);
                Despawn(chipGO);
            }
        }
    }
}