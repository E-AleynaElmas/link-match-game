using System.Collections;
using LinkMatch.Game.Board;
using LinkMatch.Game.Chips;
using UnityEngine;

namespace LinkMatch.Game.Strategies
{
    public interface IFillStrategy
    {
        IEnumerator Fill(
            BoardModel model,
            Chip[,] chipViews,
            System.Func<int,int,Vector3> toWorld,
            System.Func<ChipType> nextRandomType,
            System.Func<ChipType, Vector3, Chip> spawnChip, 
            float fallDuration
        );
    }
}