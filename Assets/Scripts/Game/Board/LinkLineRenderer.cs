using System.Collections.Generic;
using LinkMatch.Core.Utils;
using LinkMatch.Game.Chips;
using UnityEngine;

namespace LinkMatch.Game.Board
{
    public sealed class LinkLineRenderer
    {
        private readonly LineRenderer _lineRenderer;
        private readonly ChipPalette _palette;

        public LinkLineRenderer(LineRenderer lineRenderer, ChipPalette palette)
        {
            _lineRenderer = lineRenderer;
            _palette = palette;
            _lineRenderer.positionCount = 0;
        }

        public void UpdatePath(IReadOnlyList<Coord> path, System.Func<int, int, Vector3> toWorldPos, ChipType pathType)
        {
            if (path == null || path.Count == 0)
            {
                ClearLine();
                return;
            }

            SetLineColor(pathType);
            _lineRenderer.positionCount = path.Count;

            for (int i = 0; i < path.Count; i++)
            {
                var coord = path[i];
                _lineRenderer.SetPosition(i, toWorldPos(coord.Row, coord.Col));
            }
        }

        public void ClearLine()
        {
            _lineRenderer.positionCount = 0;
        }

        private void SetLineColor(ChipType chipType)
        {
            var color = _palette.GetColor(chipType);
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
        }
    }
}