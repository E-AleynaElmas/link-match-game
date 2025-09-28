using UnityEngine;

namespace LinkMatch.Game.Chips
{
    public interface IChipFactory
    {
        GameObject Create(Vector3 worldPos, ChipType type);
        void Despawn(GameObject chipGO);
    }
}