using System.Collections.Generic;
using LinkMatch.Core.Utils;
using LinkMatch.Game.Chips;

namespace LinkMatch.Game.Board
{
    public sealed class BoardModel
    {
        public int Rows { get; }
        public int Cols { get; }

        private readonly ChipType[,] _grid;

        public BoardModel(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            _grid = new ChipType[rows, cols];
        }

        public ChipType Get(Coord c) => _grid[c.Row, c.Col];

        public void Set(Coord c, ChipType t) => _grid[c.Row, c.Col] = t;

        public bool InBounds(Coord c) =>
            c.Row >= 0 && c.Row < Rows && c.Col >= 0 && c.Col < Cols;

        public IEnumerable<Coord> Neighbors4(Coord c)
        {
            var deltas = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            foreach (var (dr, dc) in deltas)
            {
                var n = new Coord(c.Row + dr, c.Col + dc);
                if (InBounds(n))
                    yield return n;
            }
        }
    }
}