using System.Collections;
using System.Collections.Generic;
using LinkMatch.Core.Signals;
using LinkMatch.Core.Utils;
using LinkMatch.Game.Chips;
using LinkMatch.Game.Inputs;
using LinkMatch.Game.Strategies;
using UnityEngine;

namespace LinkMatch.Game.Board
{
    public sealed class BoardComponents
    {
        public BoardModel Model { get; set; }
        public Chip[,] ChipViews { get; set; }
        public GameStateManager GameState { get; set; }
        public ILinkPathValidator Validator { get; set; }
        public IFillStrategy FillStrategy { get; set; }
        public IShuffleStrategy ShuffleStrategy { get; set; }
        public IChipFactory ChipFactory { get; set; }
        public LinkLineRenderer LinkLineRenderer { get; set; }
        public BoardAnimationController AnimationController { get; set; }
        public System.Random RandomGenerator { get; set; }
        public float CellSize { get; set; }
    }
    public sealed class BoardController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private ChipPalette chipPalette;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject chipPrefab;
        [SerializeField] private LineRenderer linePrefab;
        [SerializeField] private Transform boardRoot;
        [SerializeField] private UnityInputService inputService;
        [SerializeField] private Camera gameCamera;

        [Header("Settings")]
        [SerializeField] private float fallDuration = 0.08f;
        [SerializeField] private int shuffleMaxRetries = 50;
        [SerializeField] private int initialMovesOverride = -1;

        private BoardComponents _components;
        private readonly List<Coord> _currentPath = new();
        private bool _isBusy; 

        private void Start()
        {
            InitializeBoard();
            EnsurePlayability();
            NotifyInitialState();
        }

        private void InitializeBoard()
        {
            _components = InitializeBoardComponents();
        }

        private BoardComponents InitializeBoardComponents()
        {
            var model = new BoardModel(levelConfig.rows, levelConfig.cols);
            var chipViews = new Chip[levelConfig.rows, levelConfig.cols];
            var rng = new System.Random();

            var gameState = new GameStateManager(levelConfig, initialMovesOverride);

            var validator = new LinkPathValidator(minLength: 3);
            var fillStrategy = new GravityFillStrategy();
            var shuffleStrategy = new SimpleShuffleStrategy();

            int boardCapacity = levelConfig.rows * levelConfig.cols;
            var chipComponent = chipPrefab.GetComponent<Chip>();
            var chipFactory = new ChipFactory(
                chipComponent,
                boardRoot,
                chipPalette,
                prewarm: boardCapacity,
                maxSize: boardCapacity * 2);

            var lineRenderer = Instantiate(linePrefab, boardRoot);
            var linkLineRenderer = new LinkLineRenderer(lineRenderer, chipPalette);

            var animationController = new BoardAnimationController(
                popDuration: 0.06f,
                pulseDuration: 0.08f,
                chipFactory);

            var cellSize = CalculateCellSize();
            BuildTiles(model, cellSize);
            SpawnInitialChips(model, chipViews, chipFactory, rng, cellSize);
            ConfigureCamera(cellSize);

            return new BoardComponents
            {
                Model = model,
                ChipViews = chipViews,
                GameState = gameState,
                Validator = validator,
                FillStrategy = fillStrategy,
                ShuffleStrategy = shuffleStrategy,
                ChipFactory = chipFactory,
                LinkLineRenderer = linkLineRenderer,
                AnimationController = animationController,
                RandomGenerator = rng,
                CellSize = cellSize
            };
        }

        private float CalculateCellSize()
        {
            if (tilePrefab.TryGetComponent<SpriteRenderer>(out var tileSR))
            {
                var worldWidth = tileSR.bounds.size.x;
                if (worldWidth > 0f)
                    return worldWidth;
            }
            return 1f;
        }

        private void BuildTiles(BoardModel model, float cellSize)
        {
            for (int r = 0; r < model.Rows; r++)
            {
                for (int c = 0; c < model.Cols; c++)
                {
                    var pos = ToWorldPos(r, c, model.Rows, model.Cols, cellSize);
                    var tileGO = Instantiate(tilePrefab, pos, Quaternion.identity, boardRoot);
                    if (tileGO.TryGetComponent<Tile>(out var tile))
                        tile.Init(new Coord(r, c));
                }
            }
        }

        private void SpawnInitialChips(BoardModel model, Chip[,] chipViews, IChipFactory chipFactory, System.Random rng, float cellSize)
        {
            for (int r = 0; r < model.Rows; r++)
            {
                for (int c = 0; c < model.Cols; c++)
                {
                    var coord = new Coord(r, c);
                    var type = (ChipType)rng.Next(1, 5);
                    model.Set(coord, type);

                    var pos = ToWorldPos(r, c, model.Rows, model.Cols, cellSize);
                    var chip = chipFactory.Create(pos, type);
                    chipViews[r, c] = chip;
                }
            }
        }

        private void ConfigureCamera(float cellSize)
        {
            var camera = gameCamera != null ? gameCamera : Camera.main;
            if (camera != null && camera.TryGetComponent<CameraAutoFit>(out var fitter))
                fitter.Fit(levelConfig.rows, levelConfig.cols, cellSize);
        }

        private Vector3 ToWorldPos(int row, int col, int totalRows, int totalCols, float cellSize)
        {
            float width = totalCols * cellSize;
            float height = totalRows * cellSize;

            float originX = -width * 0.5f + (cellSize * 0.5f);
            float originY = -height * 0.5f + (cellSize * 0.5f);

            return new Vector3(originX + col * cellSize, originY + row * cellSize, 0f);
        }

        private void EnsurePlayability()
        {
            if (!_components.ShuffleStrategy.HasAnyMove(_components.Model))
                StartCoroutine(ShuffleRoutine());
        }

        private void NotifyInitialState()
        {
            _components.GameState.Reset(initialMovesOverride);
            GameSignals.BoardBusy(false);
        }

        private void OnEnable()
        {
            if (inputService != null)
            {
                inputService.PressedWorld += OnPressed;
                inputService.DraggedWorld += OnDragged;
                inputService.ReleasedWorld += OnReleased;
            }
            else
            {
                Debug.LogError("InputService reference is missing in BoardController! Please assign it in the inspector.");
            }
        }

        private void OnDisable()
        {
            if (inputService != null)
            {
                inputService.PressedWorld -= OnPressed;
                inputService.DraggedWorld -= OnDragged;
                inputService.ReleasedWorld -= OnReleased;
            }
        }

        private void OnPressed(Vector3 worldPos)
        {
            if (_isBusy) return;

            if (!TryWorldToCoord(worldPos, out var coord)) return;

            var chipType = _components.Model.Get(coord);
            if (!_components.Validator.CanStart(chipType)) return;

            ClearSelection();
            _currentPath.Clear();
            _currentPath.Add(coord);
            var chip = _components.ChipViews[coord.Row, coord.Col];
            if (chip != null)
                chip.SetSelected(true);

            _components.LinkLineRenderer.UpdatePath(_currentPath, ToWorld, chipType);
        }

        private void OnDragged(Vector3 world)
        {
            if (_isBusy || _currentPath.Count == 0) return;
            if (!TryWorldToCoord(world, out var coord)) return;

            // Backtrack handling
            if (_currentPath.Count >= 2 && coord.Equals(_currentPath[^2]))
            {
                var removed = _currentPath[^1];
                _currentPath.RemoveAt(_currentPath.Count - 1);
                var chip = _components.ChipViews[removed.Row, removed.Col];
                if (chip != null)
                    chip.SetSelected(false);

                var pathType = _components.Model.Get(_currentPath[0]);
                _components.LinkLineRenderer.UpdatePath(_currentPath, ToWorld, pathType);
                return;
            }

            // Normal path extension
            if (_components.Validator.CanAppend(_currentPath, coord, c => _components.Model.Get(c)))
            {
                _currentPath.Add(coord);
                var chip = _components.ChipViews[coord.Row, coord.Col];
                if (chip != null)
                    chip.SetSelected(true);

                var pathType = _components.Model.Get(_currentPath[0]);
                _components.LinkLineRenderer.UpdatePath(_currentPath, ToWorld, pathType);
            }
        }

        private void OnReleased(Vector3 world)
        {
            if (_isBusy || _currentPath.Count == 0) return;

            _components.LinkLineRenderer.ClearLine();
            bool isValidPath = _components.Validator.IsValidOnRelease(_currentPath);

            if (!isValidPath)
            {
                ClearSelection();
                _currentPath.Clear();
                return;
            }

            StartCoroutine(HandleValidPath());
        }

        private bool TryWorldToCoord(Vector3 world, out Coord coord)
        {
            var hit = Physics2D.OverlapPoint(world);
            if (hit && hit.TryGetComponent<Tile>(out var tile))
            {
                coord = tile.Coord;
                return true;
            }

            int col = Mathf.RoundToInt(world.x / _components.CellSize);
            int row = Mathf.RoundToInt(world.y / _components.CellSize);
            coord = new Coord(row, col);
            return _components.Model.InBounds(coord);
        }

        private Vector3 ToWorld(int row, int col)
        {
            float width = levelConfig.cols * _components.CellSize;
            float height = levelConfig.rows * _components.CellSize;

            float originX = -width * 0.5f + (_components.CellSize * 0.5f);
            float originY = -height * 0.5f + (_components.CellSize * 0.5f);

            return new Vector3(originX + col * _components.CellSize, originY + row * _components.CellSize, 0f);
        }

        private void ClearSelection()
        {
            foreach (var coord in _currentPath)
            {
                var chip = _components.ChipViews[coord.Row, coord.Col];
                if (chip != null)
                    chip.SetSelected(false);
            }
        }

        private IEnumerator HandleValidPath()
        {
            _isBusy = true;
            GameSignals.BoardBusy(true);

            int removedCount = _currentPath.Count;

            foreach (var coord in _currentPath)
                _components.Model.Set(coord, ChipType.None);

            yield return _components.AnimationController.PopAndDestroyChips(_currentPath, _components.ChipViews);
            _currentPath.Clear();

            bool canContinue = _components.GameState.TryConsumeMove(removedCount);
            if (!canContinue)
            {
                _isBusy = false;
                GameSignals.BoardBusy(false);
                yield break;
            }

            yield return _components.FillStrategy.Fill(
                _components.Model, _components.ChipViews,
                ToWorld, NextRandomType, SpawnChip, fallDuration
            );

            if (!_components.ShuffleStrategy.HasAnyMove(_components.Model))
                yield return ShuffleRoutine();

            _isBusy = false;
            GameSignals.BoardBusy(false);
        }

        private ChipType NextRandomType()
        {
            return (ChipType)_components.RandomGenerator.Next(1, 5);
        }

        private Chip SpawnChip(ChipType type, Vector3 position)
        {
            return _components.ChipFactory.Create(position, type);
        }

        private IEnumerator ShuffleRoutine()
        {
            _isBusy = true;

            int retries = 0;
            do
            {
                retries++;
                _components.ShuffleStrategy.Shuffle(_components.Model, _components.RandomGenerator);

                UpdateChipVisuals();
                yield return _components.AnimationController.PulseAllChips(_components.ChipViews, 1.05f);

            } while (!_components.ShuffleStrategy.HasAnyMove(_components.Model) && retries < shuffleMaxRetries);

            _isBusy = false;
        }

        private void UpdateChipVisuals()
        {
            for (int r = 0; r < _components.Model.Rows; r++)
            {
                for (int c = 0; c < _components.Model.Cols; c++)
                {
                    var chip = _components.ChipViews[r, c];
                    if (chip == null) continue;

                    var chipType = _components.Model.Get(new Coord(r, c));
                    chip.SetSelected(false);
                    chip.SetType(chipType, chipPalette.GetSprite(chipType));
                }
            }
        }
    }

    public sealed class GameStateManager
    {
        private int _score;
        private int _movesLeft;
        private readonly LevelConfig _config;

        public int Score => _score;
        public int MovesLeft => _movesLeft;
        public bool IsGameOver => _movesLeft <= 0 || _score >= _config.targetScore;
        public bool HasWon => _score >= _config.targetScore;

        public GameStateManager(LevelConfig config, int initialMovesOverride = -1)
        {
            _config = config;
            _movesLeft = (initialMovesOverride > 0) ? initialMovesOverride : config.initialMoves;
            _score = 0;
        }

        public void Reset(int initialMovesOverride = -1)
        {
            _movesLeft = (initialMovesOverride > 0) ? initialMovesOverride : _config.initialMoves;
            _score = 0;
            NotifyStateChanged();
        }

        public bool TryConsumeMove(int pointsGained)
        {
            if (IsGameOver) return false;

            _score += pointsGained;
            _movesLeft--;
            NotifyStateChanged();

            if (IsGameOver)
            {
                GameSignals.GameOver(HasWon);
                return false;
            }

            return true;
        }

        private void NotifyStateChanged()
        {
            GameSignals.ScoreChanged(_score);
            GameSignals.MovesChanged(_movesLeft);
        }
    }

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