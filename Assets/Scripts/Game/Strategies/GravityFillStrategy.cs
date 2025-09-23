using System.Collections;
using LinkMatch.Core.Utils;
using LinkMatch.Game.Board;
using LinkMatch.Game.Chips;
using UnityEngine;

namespace LinkMatch.Game.Strategies
{
    public sealed class GravityFillStrategy : IFillStrategy
    {
        public IEnumerator Fill(
            BoardModel m,
            Chip[,] views,
            System.Func<int,int,Vector3> ToWorld,
            System.Func<ChipType> NextType,
            System.Func<ChipType, Vector3, Chip> Spawn,
            float fallDur)
        {
            int rows = m.Rows, cols = m.Cols;

            // Her kolon için aşağı sıkıştır
            for (int c = 0; c < cols; c++)
            {
                int writeRow = 0;

                // Önce mevcut chipleri aşağı topla
                for (int r = 0; r < rows; r++)
                {
                    if (m.Get(new Coord(r, c)) == ChipType.None || views[r, c] == null) continue;

                    if (writeRow != r)
                    {
                        // Modelde kaydır
                        var t = m.Get(new Coord(r, c));
                        m.Set(new Coord(writeRow, c), t);
                        m.Set(new Coord(r, c), ChipType.None);

                        // View’de diziyi güncelle
                        var chip = views[r, c];
                        views[writeRow, c] = chip;
                        views[r, c] = null;

                        yield return MoveTo(chip.transform, ToWorld(writeRow, c), fallDur);
                    }
                    writeRow++;
                }

                // Kalan boşluklara üstten yeni chip spawn et
                for (int r = writeRow; r < rows; r++)
                {
                    var t = NextType();
                    m.Set(new Coord(r, c), t);

                    Vector3 target = ToWorld(r, c);
                    Vector3 spawn = target + Vector3.up * (rows * 0.5f);
                    var chip = Spawn(t, spawn);
                    views[r, c] = chip;

                    yield return MoveTo(chip.transform, target, fallDur);
                }
            }
        }

        private static IEnumerator MoveTo(Transform tr, Vector3 to, float dur)
        {
            Vector3 from = tr.position;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                tr.position = Vector3.Lerp(from, to, k);
                yield return null;
            }
            tr.position = to;
        }
    }
}