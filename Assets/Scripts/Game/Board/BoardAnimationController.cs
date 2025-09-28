using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LinkMatch.Game.Chips;
using LinkMatch.Core.Utils;

namespace LinkMatch.Game.Board
{
    public sealed class BoardAnimationController
    {
        private readonly float _popDuration;
        private readonly float _pulseDuration;
        private readonly IChipFactory _chipFactory;

        public BoardAnimationController(float popDuration, float pulseDuration, IChipFactory chipFactory)
        {
            _popDuration = popDuration;
            _pulseDuration = pulseDuration;
            _chipFactory = chipFactory;
        }

        public IEnumerator PopAndDestroyChips(IReadOnlyList<Coord> coords, Chip[,] chipViews)
        {
            foreach (var coord in coords)
            {
                var chip = chipViews[coord.Row, coord.Col];
                if (chip == null) continue;

                chipViews[coord.Row, coord.Col] = null;
                yield return StartPopAnimation(chip);
                _chipFactory.Despawn(chip);
            }
        }

        public IEnumerator PulseAllChips(Chip[,] chipViews, float scale = 1.05f)
        {
            var activeChips = GatherActiveChips(chipViews);
            yield return PulseChips(activeChips, scale, _pulseDuration);
        }

        private IEnumerator StartPopAnimation(Chip chip)
        {
            var originalScale = chip.transform.localScale;
            var targetScale = originalScale * 1.15f;

            float elapsed = 0f;
            while (elapsed < _popDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / _popDuration);
                chip.transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
                yield return null;
            }
        }

        private IEnumerator PulseChips(List<Transform> chips, float scale, float duration)
        {
            yield return ScaleChips(chips, 1f, scale, duration);
            yield return ScaleChips(chips, scale, 1f, duration);
        }

        private IEnumerator ScaleChips(List<Transform> chips, float fromScale, float toScale, float duration)
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
                yield return null;
            }
        }

        private List<Transform> GatherActiveChips(Chip[,] chipViews)
        {
            var activeChips = new List<Transform>();
            int rows = chipViews.GetLength(0);
            int cols = chipViews.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var chip = chipViews[r, c];
                    if (chip != null)
                        activeChips.Add(chip.transform);
                }
            }
            return activeChips;
        }
    }
}