using System;
using System.Collections.Generic;
using LinkMatch.Core.Utils;
using LinkMatch.Game.Chips;

namespace LinkMatch.Game.Strategies
{
    public interface ILinkPathValidator
    {
        bool CanStart(ChipType headType);

        bool CanAppend(IReadOnlyList<Coord> path, Coord next, Func<Coord, ChipType> get);

        bool IsValidOnRelease(IReadOnlyList<Coord> currentPath);
    }
}