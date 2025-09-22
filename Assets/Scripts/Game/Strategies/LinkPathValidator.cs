using System.Collections.Generic;
using LinkMatch.Core.Utils;
using LinkMatch.Game.Chips;

namespace LinkMatch.Game.Strategies
{
    public sealed class LinkPathValidator : ILinkPathValidator
    {
        private readonly int _minLength;

        public LinkPathValidator(int minLength = 3)
        {
            _minLength = minLength;
        }

        public bool CanStart(ChipType headType) => headType != ChipType.None;

        public bool CanAppend(IReadOnlyList<Coord> path, Coord next, ChipType[,] grid)
        {
            if (path == null || path.Count == 0) return false;

            var last = path[^1];
            // 4-yön komşu olmalı
            if (Coord.Manhattan(last, next) != 1) return false;

            // Renk kontrolü (ilk düğümün rengi ile aynı)
            var first = path[0];
            if (grid[next.Row, next.Col] != grid[first.Row, first.Col]) return false;

            // Tekrar ziyaret engeli (backtrack'i controller zaten ayrı ele alıyor)
            for (int i = 0; i < path.Count; i++)
                if (path[i].Row == next.Row && path[i].Col == next.Col)
                    return false;

            return true;
        }

        public bool IsValidOnRelease(IReadOnlyList<Coord> path) => path != null && path.Count >= _minLength;
    }
}