using System;
using System.Collections.Generic;
using LinkMatch.Core.Utils;
using LinkMatch.Game.Chips;
using UnityEngine;

namespace LinkMatch.Game.Strategies
{
    public sealed class LinkPathValidator : ILinkPathValidator
    {
        private readonly int _minLength;
        private readonly bool _allowDiagonal;

        public LinkPathValidator(int minLength = 3, bool allowDiagonal = false)
        {
            _minLength = minLength;
            _allowDiagonal = allowDiagonal;
        }

        public bool CanStart(ChipType headType) => headType != ChipType.None;

        public bool CanAppend(IReadOnlyList<Coord> path, Coord next, Func<Coord, ChipType> get)
        {
            if (path == null || path.Count == 0) return false;

            var last = path[^1];
            int dr = next.Row - last.Row;
            int dc = next.Col - last.Col;

            // Adjacency: 4-yön veya (opsiyonel) 8-yön
            bool adjacent =
                _allowDiagonal
                    ? (Mathf.Abs(dr) <= 1 && Mathf.Abs(dc) <= 1 && !(dr == 0 && dc == 0))
                    : (Mathf.Abs(dr) + Mathf.Abs(dc) == 1);

            if (!adjacent) return false;

            var first = path[0];
            if (get(next) != get(first)) return false;

            // tekrar ziyaret yok (backtrack'i controller yönetiyor)
            for (int i = 0; i < path.Count; i++)
                if (path[i].Row == next.Row && path[i].Col == next.Col)
                    return false;

            return true;
        }

        public bool IsValidOnRelease(IReadOnlyList<Coord> path) => path != null && path.Count >= _minLength;
    }
}