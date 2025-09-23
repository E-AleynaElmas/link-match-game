using System.Collections.Generic;
using LinkMatch.Core.Utils;
using LinkMatch.Game.Chips;
using LinkMatch.Game.Inputs;
using LinkMatch.Game.Strategies;
using UnityEngine;

namespace LinkMatch.Game.Board
{
    public sealed class BoardController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private ChipPalette chipPalette;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject chipPrefab;
        [SerializeField] private Transform boardRoot;
        [SerializeField] private UnityInputService inputService;

        [Header("Board Settings")]
        [SerializeField] private float cellSize = 1f;

        private BoardModel _model;
        private Chip[,] _chipViews;
        private ILinkPathValidator _validator;
        private readonly List<Coord> _path = new();

        private void Start()
        {
            _model = new BoardModel(levelConfig.rows, levelConfig.cols);
            _chipViews = new Chip[levelConfig.rows, levelConfig.cols];
            _validator = new LinkPathValidator(minLength: 3, allowDiagonal: true);

            // BoardController.Start() içinde, BuildTiles()'dan önce:
            var tileSR = tilePrefab.GetComponent<SpriteRenderer>();
            if (tileSR != null)
            {
                // sprite’ın dünyadaki genişliği (scale ve PPU dahil)
                var worldWidth = tileSR.bounds.size.x;
                if (worldWidth > 0f) cellSize = worldWidth;
            }
            BuildTiles();
            SpawnInitialChips();

            var fitter = Camera.main ? Camera.main.GetComponent<CameraAutoFit>() : null;
            if (fitter != null)
                fitter.Fit(levelConfig.rows, levelConfig.cols, cellSize);
        }

        private void OnEnable()
        {
            if (!inputService) inputService = FindAnyObjectByType<UnityInputService>();
            if (inputService)
            {
                inputService.PressedWorld += OnPressed;
                inputService.DraggedWorld += OnDragged;
                inputService.ReleasedWorld += OnReleased;
            }
        }

        private void OnDisable()
        {
            if (inputService)
            {
                inputService.PressedWorld -= OnPressed;
                inputService.DraggedWorld -= OnDragged;
                inputService.ReleasedWorld -= OnReleased;
            }
        }

        private void OnPressed(Vector3 world)
        {
            if (!TryWorldToCoord(world, out var c)) return;

            var type = _model.Get(c);
            if (!_validator.CanStart(type)) return;

            ClearSelection();
            _path.Clear();
            _path.Add(c);
            _chipViews[c.Row, c.Col]?.SetSelected(true);
            Debug.Log($"PATH START @ {c.Row},{c.Col} type={type}");
        }

        private void OnDragged(Vector3 world)
        {
            if (_path.Count == 0) return;
            if (!TryWorldToCoord(world, out var c)) return;

            // Backtrack: bir önceki düğüme geri dönüyorsak son node’u kaldır
            if (_path.Count >= 2 && c.Row == _path[^2].Row && c.Col == _path[^2].Col)
            {
                var removed = _path[^1];
                _path.RemoveAt(_path.Count - 1);
                _chipViews[removed.Row, removed.Col]?.SetSelected(false);
                return;
            }

            // Normal ekleme
            if (_validator.CanAppend(_path, c, GetGridSnapshot()))
            {
                _path.Add(c);
                _chipViews[c.Row, c.Col]?.SetSelected(true);
                Debug.Log($"PATH ADD @ {c.Row},{c.Col}");
            }
        }

        private void OnReleased(Vector3 world)
        {
            if (_path.Count == 0) return;

            bool valid = _validator.IsValidOnRelease(_path);
            Debug.Log(valid ? $"PATH VALID len={_path.Count}" : "PATH INVALID");

            if (!valid)
            {
                ClearSelection();
                _path.Clear();
                return;
            }

            ClearSelection();
            _path.Clear();
        }

        private bool TryWorldToCoord(Vector3 world, out Coord coord)
        {
            // Öncelik: Tile collider’ı
            var hit = Physics2D.OverlapPoint(world);
            if (hit && hit.TryGetComponent<Tile>(out var tile))
            {
                coord = tile.Coord;
                return true;
            }
            // Fallback: grid hesabı
            int col = Mathf.RoundToInt(world.x / cellSize);
            int row = Mathf.RoundToInt(world.y / cellSize);
            coord = new Coord(row, col);
            return _model.InBounds(coord);
        }

        private void BuildTiles()
        {
            for (int r = 0; r < _model.Rows; r++)
                for (int c = 0; c < _model.Cols; c++)
                {
                    var pos = ToWorld(r, c);
                    var go = Instantiate(tilePrefab, pos, Quaternion.identity, boardRoot);
                    var tile = go.GetComponent<Tile>();
                    tile.Init(new Coord(r, c));
                }
        }

        private void SpawnInitialChips()
        {
            for (int r = 0; r < _model.Rows; r++)
                for (int c = 0; c < _model.Cols; c++)
                {
                    var coord = new Coord(r, c);
                    var type = (ChipType)Random.Range(1, 5); // 1..4
                    _model.Set(coord, type);

                    Vector3 pos = ToWorld(r, c);
                    var go = Instantiate(chipPrefab, pos, Quaternion.identity, boardRoot);
                    var chip = go.GetComponent<Chip>();
                    chip.Init(type, chipPalette.GetSprite(type));
                    _chipViews[r, c] = chip;
                }
        }

        private Vector3 ToWorld(int row, int col)
        {
            float width = levelConfig.cols * cellSize;
            float height = levelConfig.rows * cellSize;

            // Alt-sol köşeyi (-width/2, -height/2) al, hücrenin merkezine oturt
            float originX = -width * 0.5f + (cellSize * 0.5f);
            float originY = -height * 0.5f + (cellSize * 0.5f);

            return new Vector3(originX + col * cellSize, originY + row * cellSize, 0f);
        }
        
        private void ClearSelection()
        {
            foreach (var c in _path)
                _chipViews[c.Row, c.Col]?.SetSelected(false);
        }

        private ChipType[,] GetGridSnapshot()
        {
            var g = new ChipType[_model.Rows, _model.Cols];
            for (int r = 0; r < _model.Rows; r++)
                for (int c = 0; c < _model.Cols; c++)
                    g[r, c] = _model.Get(new Coord(r, c));
            return g;
        }
    }
}