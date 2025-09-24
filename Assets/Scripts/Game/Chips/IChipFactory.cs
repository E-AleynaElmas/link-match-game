using UnityEngine;

namespace LinkMatch.Game.Chips
{
    public interface IChipFactory
    {
        Chip Create(Vector3 worldPos, ChipType type);
        void Despawn(Chip chip);
    }
}