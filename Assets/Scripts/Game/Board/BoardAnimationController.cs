using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using LinkMatch.Game.Chips;
using LinkMatch.Core.Utils;

namespace LinkMatch.Game.Board
{
    public sealed class BoardAnimationController
    {
        private const float POP_SCALE_MULTIPLIER = 1.15f;
        private const float DEFAULT_PULSE_SCALE = 1.05f;
        private const float NORMAL_SCALE = 1f;

        private readonly float _popDuration;
        private readonly float _pulseDuration;
        private readonly IChipFactory _chipFactory;

        public BoardAnimationController(float popDuration, float pulseDuration, IChipFactory chipFactory)
        {
            _popDuration = popDuration;
            _pulseDuration = pulseDuration;
            _chipFactory = chipFactory;
        }

        public async UniTask PopAndDestroyChips(IReadOnlyList<Coord> coords, GameObject[,] chipViews, CancellationToken cancellationToken = default)
        {
            foreach (var coord in coords)
            {
                var chipGO = chipViews[coord.Row, coord.Col];
                if (chipGO == null) continue;

                chipViews[coord.Row, coord.Col] = null;
                await StartPopAnimation(chipGO, cancellationToken);
                _chipFactory.Despawn(chipGO);
            }
        }

        public async UniTask PulseAllChips(GameObject[,] chipViews, float scale = DEFAULT_PULSE_SCALE, CancellationToken cancellationToken = default)
        {
            var activeChips = GatherActiveChips(chipViews);
            await PulseChips(activeChips, scale, _pulseDuration, cancellationToken);
        }

        private async UniTask StartPopAnimation(GameObject chipGO, CancellationToken cancellationToken = default)
        {
            var originalScale = chipGO.transform.localScale;
            var targetScale = originalScale * POP_SCALE_MULTIPLIER;

            float elapsed = 0f;
            while (elapsed < _popDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / _popDuration);
                chipGO.transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
                await UniTask.Yield(cancellationToken);
            }
        }

        private async UniTask PulseChips(List<Transform> chips, float scale, float duration, CancellationToken cancellationToken = default)
        {
            await ScaleChips(chips, NORMAL_SCALE, scale, duration, cancellationToken);
            await ScaleChips(chips, scale, NORMAL_SCALE, duration, cancellationToken);
        }

        private async UniTask ScaleChips(List<Transform> chips, float fromScale, float toScale, float duration, CancellationToken cancellationToken = default)
        {
            var scaleVector = Vector3.one;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float currentScale = Mathf.Lerp(fromScale, toScale, progress);

                scaleVector.x = scaleVector.y = scaleVector.z = currentScale;

                for (int i = 0; i < chips.Count; i++)
                {
                    if (chips[i] != null)
                        chips[i].localScale = scaleVector;
                }
                await UniTask.Yield(cancellationToken);
            }
        }

        private List<Transform> GatherActiveChips(GameObject[,] chipViews)
        {
            var activeChips = new List<Transform>();
            int rows = chipViews.GetLength(0);
            int cols = chipViews.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var chipGO = chipViews[r, c];
                    if (chipGO != null)
                        activeChips.Add(chipGO.transform);
                }
            }
            return activeChips;
        }
    }
}