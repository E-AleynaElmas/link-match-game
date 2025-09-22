using System.Collections.Generic;
using LinkMatch.Core.Utils;
using LinkMatch.Game.Chips;

namespace LinkMatch.Game.Strategies
{
    public interface ILinkPathValidator
    {
        bool CanStart(ChipType headType);

        bool CanAppend(IReadOnlyList<Coord> currentPath, Coord next, ChipType[,] logicalGrid);

        bool IsValidOnRelease(IReadOnlyList<Coord> currentPath);
    }
}