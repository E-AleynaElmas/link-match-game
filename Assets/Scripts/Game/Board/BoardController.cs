using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        public GameObject[,] ChipViews { get; set; }
        public ChipManager ChipManager { get; set; }
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
        private const int MINIMUM_LINK_LENGTH = 3;
        private const int POOL_SIZE_MULTIPLIER = 2;
        private const float DEFAULT_POP_DURATION = 0.06f;
        private const float DEFAULT_PULSE_DURATION = 0.08f;
        private const float DEFAULT_CELL_SIZE = 1f;
        private const int CHIP_TYPE_MIN = 1;
        private const int CHIP_TYPE_MAX = 5;
        private const float DEFAULT_PULSE_SCALE = 1.05f;
        private const float HALF_MULTIPLIER = 0.5f;

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
        private bool _isGameOver; 

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
            var chipViews = new GameObject[levelConfig.rows, levelConfig.cols];
            var rng = new System.Random();

            var gameState = new GameStateManager(levelConfig, initialMovesOverride);

            var validator = new LinkPathValidator(minLength: MINIMUM_LINK_LENGTH);
            var fillStrategy = new GravityFillStrategy();
            var shuffleStrategy = new SimpleShuffleStrategy();

            int boardCapacity = levelConfig.rows * levelConfig.cols;
            var chipManager = new GameObject("ChipManager").AddComponent<ChipManager>();
            chipManager.transform.SetParent(boardRoot);

            var chipFactory = new ChipFactory(
                chipManager,
                boardRoot,
                chipPalette,
                prewarm: boardCapacity);

            var lineRenderer = Instantiate(linePrefab, boardRoot);
            var linkLineRenderer = new LinkLineRenderer(lineRenderer, chipPalette);

            var animationController = new BoardAnimationController(
                popDuration: DEFAULT_POP_DURATION,
                pulseDuration: DEFAULT_PULSE_DURATION,
                chipFactory);

            var cellSize = CalculateCellSize();
            BuildTiles(model, cellSize);
            SpawnInitialChips(model, chipViews, chipFactory, rng, cellSize);
            ConfigureCamera(cellSize);

            return new BoardComponents
            {
                Model = model,
                ChipViews = chipViews,
                ChipManager = chipManager,
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
            return DEFAULT_CELL_SIZE;
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

        private void SpawnInitialChips(BoardModel model, GameObject[,] chipViews, IChipFactory chipFactory, System.Random rng, float cellSize)
        {
            for (int r = 0; r < model.Rows; r++)
            {
                for (int c = 0; c < model.Cols; c++)
                {
                    var coord = new Coord(r, c);
                    var type = (ChipType)rng.Next(CHIP_TYPE_MIN, CHIP_TYPE_MAX);
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

            float originX = -width * HALF_MULTIPLIER + (cellSize * HALF_MULTIPLIER);
            float originY = -height * HALF_MULTIPLIER + (cellSize * HALF_MULTIPLIER);

            return new Vector3(originX + col * cellSize, originY + row * cellSize, 0f);
        }

        private void EnsurePlayability()
        {
            if (!_components.ShuffleStrategy.HasAnyMove(_components.Model))
                ShuffleRoutine().Forget();
        }

        public void ResetGame()
        {
            if (_components == null) return;

            ClearSelection();
            _isGameOver = false;
            _components.GameState.Reset(initialMovesOverride);
            RegenerateBoard();
            EnsurePlayability();
            GameSignals.BoardBusy(false);
        }

        private void RegenerateBoard()
        {
            // Clear existing chips
            for (int r = 0; r < _components.Model.Rows; r++)
            {
                for (int c = 0; c < _components.Model.Cols; c++)
                {
                    var chipGO = _components.ChipViews[r, c];
                    if (chipGO != null)
                    {
                        _components.ChipFactory.Despawn(chipGO);
                        _components.ChipViews[r, c] = null;
                    }
                }
            }

            // Regenerate board content
            _components.FillStrategy.Fill(
                _components.Model, _components.ChipViews,
                ToWorld, NextRandomType, SpawnChip, 0f
            ).Forget();
        }

        private void NotifyInitialState()
        {
            _isGameOver = false; // Reset game over state
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

            GameSignals.OnGameOver += OnGameOver;
            GameSignals.OnGameResetRequested += ResetGame;
        }

        private void OnDisable()
        {
            if (inputService != null)
            {
                inputService.PressedWorld -= OnPressed;
                inputService.DraggedWorld -= OnDragged;
                inputService.ReleasedWorld -= OnReleased;
            }

            GameSignals.OnGameOver -= OnGameOver;
            GameSignals.OnGameResetRequested -= ResetGame;
        }

        private void OnGameOver(bool hasWon)
        {
            _isGameOver = true;
            _isBusy = true; // Block all further interactions
            ClearSelection();
            _currentPath.Clear();
            _components.LinkLineRenderer.ClearLine();
        }

        private void OnPressed(Vector3 worldPos)
        {
            if (_isBusy || _isGameOver) return;

            if (!TryWorldToCoord(worldPos, out var coord)) return;

            var chipType = _components.Model.Get(coord);
            if (!_components.Validator.CanStart(chipType)) return;

            ClearSelection();
            _currentPath.Clear();
            _currentPath.Add(coord);
            var chipGO = _components.ChipViews[coord.Row, coord.Col];
            if (chipGO != null)
                _components.ChipManager.SetChipSelected(chipGO, true);

            _components.LinkLineRenderer.UpdatePath(_currentPath, ToWorld, chipType);
        }

        private void OnDragged(Vector3 world)
        {
            if (_isBusy || _isGameOver || _currentPath.Count == 0) return;
            if (!TryWorldToCoord(world, out var coord)) return;

            // Backtrack handling
            if (_currentPath.Count >= 2 && coord.Equals(_currentPath[^2]))
            {
                var removed = _currentPath[^1];
                _currentPath.RemoveAt(_currentPath.Count - 1);
                var chipGO = _components.ChipViews[removed.Row, removed.Col];
                if (chipGO != null)
                    _components.ChipManager.SetChipSelected(chipGO, false);

                var pathType = _components.Model.Get(_currentPath[0]);
                _components.LinkLineRenderer.UpdatePath(_currentPath, ToWorld, pathType);
                return;
            }

            // Normal path extension
            if (_components.Validator.CanAppend(_currentPath, coord, c => _components.Model.Get(c)))
            {
                _currentPath.Add(coord);
                var chipGO = _components.ChipViews[coord.Row, coord.Col];
                if (chipGO != null)
                    _components.ChipManager.SetChipSelected(chipGO, true);

                var pathType = _components.Model.Get(_currentPath[0]);
                _components.LinkLineRenderer.UpdatePath(_currentPath, ToWorld, pathType);
            }
        }

        private void OnReleased(Vector3 world)
        {
            if (_isBusy || _isGameOver || _currentPath.Count == 0) return;

            _components.LinkLineRenderer.ClearLine();
            bool isValidPath = _components.Validator.IsValidOnRelease(_currentPath);

            if (!isValidPath)
            {
                ClearSelection();
                _currentPath.Clear();
                return;
            }

            HandleValidPath().Forget();
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

            float originX = -width * HALF_MULTIPLIER + (_components.CellSize * HALF_MULTIPLIER);
            float originY = -height * HALF_MULTIPLIER + (_components.CellSize * HALF_MULTIPLIER);

            return new Vector3(originX + col * _components.CellSize, originY + row * _components.CellSize, 0f);
        }

        private void ClearSelection()
        {
            _components.ChipManager.ClearAllSelections();
        }

        private async UniTask HandleValidPath(CancellationToken cancellationToken = default)
        {
            _isBusy = true;
            GameSignals.BoardBusy(true);

            int removedCount = _currentPath.Count;

            foreach (var coord in _currentPath)
                _components.Model.Set(coord, ChipType.None);

            await _components.AnimationController.PopAndDestroyChips(_currentPath, _components.ChipViews, cancellationToken);
            _currentPath.Clear();

            bool canContinue = _components.GameState.TryConsumeMove(removedCount);

            // Fill işlemini game over olsa bile tamamla (visual completeness için)
            await _components.FillStrategy.Fill(
                _components.Model, _components.ChipViews,
                ToWorld, NextRandomType, SpawnChip, fallDuration, cancellationToken
            );

            // Game over olduysa shuffle yapmaya gerek yok, input zaten bloklu
            if (canContinue && !_components.ShuffleStrategy.HasAnyMove(_components.Model))
                await ShuffleRoutine(cancellationToken);

            _isBusy = false;
            GameSignals.BoardBusy(false);
        }

        private ChipType NextRandomType()
        {
            return (ChipType)_components.RandomGenerator.Next(CHIP_TYPE_MIN, CHIP_TYPE_MAX);
        }

        private GameObject SpawnChip(ChipType type, Vector3 position)
        {
            return _components.ChipFactory.Create(position, type);
        }

        private async UniTask ShuffleRoutine(CancellationToken cancellationToken = default)
        {
            _isBusy = true;

            int retries = 0;
            do
            {
                retries++;
                _components.ShuffleStrategy.Shuffle(_components.Model, _components.RandomGenerator);

                UpdateChipVisuals();
                await _components.AnimationController.PulseAllChips(_components.ChipViews, DEFAULT_PULSE_SCALE, cancellationToken);

            } while (!_components.ShuffleStrategy.HasAnyMove(_components.Model) && retries < shuffleMaxRetries);

            _isBusy = false;
        }

        private void UpdateChipVisuals()
        {
            for (int r = 0; r < _components.Model.Rows; r++)
            {
                for (int c = 0; c < _components.Model.Cols; c++)
                {
                    var chipGO = _components.ChipViews[r, c];
                    if (chipGO == null) continue;

                    var chipType = _components.Model.Get(new Coord(r, c));
                    _components.ChipManager.SetChipSelected(chipGO, false);
                    _components.ChipManager.SetChipType(chipGO.GetInstanceID(), chipType, chipPalette.GetSprite(chipType));
                }
            }
        }
    }
}