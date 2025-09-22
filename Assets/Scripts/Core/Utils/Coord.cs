using System;

namespace LinkMatch.Core.Utils 
{
    public readonly struct Coord 
    {
        public readonly int Row; // y
        public readonly int Col; // x

        public Coord(int row, int col) 
        {
            Row = row;
            Col = col;
        }

        public static readonly Coord Invalid = new Coord(-1, -1);

        public static int Manhattan(Coord a, Coord b) =>
            Math.Abs(a.Row - b.Row) + Math.Abs(a.Col - b.Col);
    }
}