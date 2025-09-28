using System.Threading;
using Cysharp.Threading.Tasks;
using LinkMatch.Core.Utils;
using LinkMatch.Game.Board;
using LinkMatch.Game.Chips;
using UnityEngine;

namespace LinkMatch.Game.Strategies
{
    public sealed class GravityFillStrategy : IFillStrategy
    {
        public async UniTask Fill(
            BoardModel m,
            GameObject[,] views,
            System.Func<int,int,Vector3> ToWorld,
            System.Func<ChipType> NextType,
            System.Func<ChipType, Vector3, GameObject> Spawn,
            float fallDur,
            CancellationToken cancellationToken = default)
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

                        // View'de diziyi güncelle
                        var chipGO = views[r, c];
                        views[writeRow, c] = chipGO;
                        views[r, c] = null;

                        await MoveTo(chipGO.transform, ToWorld(writeRow, c), fallDur, cancellationToken);
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
                    var chipGO = Spawn(t, spawn);
                    views[r, c] = chipGO;

                    await MoveTo(chipGO.transform, target, fallDur, cancellationToken);
                }
            }
        }

        private static async UniTask MoveTo(Transform tr, Vector3 to, float dur, CancellationToken cancellationToken = default)
        {
            Vector3 from = tr.position;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                tr.position = Vector3.Lerp(from, to, k);
                await UniTask.Yield(cancellationToken);
            }
            tr.position = to;
        }
    }
}