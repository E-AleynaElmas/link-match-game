using System.Collections.Generic;
using LinkMatch.Core.Utils;
using LinkMatch.Game.Board;
using LinkMatch.Game.Chips;

namespace LinkMatch.Game.Strategies
{
    public sealed class SimpleShuffleStrategy : IShuffleStrategy
    {
        public bool HasAnyMove(BoardModel m)
        {
            int rows = m.Rows, cols = m.Cols;
            var visited = new bool[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (visited[r, c]) continue;
                    var t = m.Get(new Coord(r, c));
                    if (t == ChipType.None) continue;

                    int size = 0;
                    var q = new Queue<Coord>();
                    q.Enqueue(new Coord(r, c));
                    visited[r, c] = true;

                    while (q.Count > 0)
                    {
                        var cur = q.Dequeue();
                        size++;
                        foreach (var n in m.Neighbors4(cur))
                        {
                            if (visited[n.Row, n.Col]) continue;
                            if (m.Get(n) != t) continue;
                            visited[n.Row, n.Col] = true;
                            q.Enqueue(n);
                        }
                    }

                    if (size >= 3) return true;
                }
            }
            return false;
        }

        public void Shuffle(BoardModel m, System.Random rng)
        {
            var list = new List<ChipType>(m.Rows * m.Cols);
            for (int r = 0; r < m.Rows; r++)
            {
                for (int c = 0; c < m.Cols; c++)
                    list.Add(m.Get(new Coord(r, c)));
            }

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            int k = 0;
            for (int r = 0; r < m.Rows; r++)
            {
                for (int c = 0; c < m.Cols; c++)
                    m.Set(new Coord(r, c), list[k++]);
            }
                
        }
    }
}