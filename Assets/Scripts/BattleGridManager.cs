using System.Collections.Generic;
using UnityEngine;

namespace BattleBeasts
{
    public class BattleGridManager : MonoBehaviour
    {
        [Header("Rules")]
        [SerializeField] private BattleRulesData rules;

        [Header("Generated Tile Grid")]
        [Tooltip("World position of tile (0,0). Treat this as the CENTER of the bottom-left tile.")]
        [SerializeField] private Transform originTileTransform;
        [SerializeField] private Transform gridParent;
        [SerializeField] private BattleTile tilePrefab;
        [SerializeField] private bool generateOnStart = true;
        [SerializeField] private bool clearExistingChildrenBeforeGenerate = true;

        [Header("Raycast / Hover")]
        [SerializeField] private LayerMask gridRaycastMask = ~0;
        [SerializeField] private Transform hoverMarker;

        [Header("Visuals")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color playerSideColor = new Color(0.28f, 0.44f, 0.85f, 1f);
        [SerializeField] private Color centerLineColor = new Color(0.85f, 0.82f, 0.22f, 1f);
        [SerializeField] private Color enemySideColor = new Color(0.78f, 0.30f, 0.30f, 1f);

        private readonly Dictionary<GridCoord, BeastController> _occupants = new Dictionary<GridCoord, BeastController>();
        private BattleTile[,] _tiles;

        public BattleRulesData Rules => rules;
        public int Width => rules != null ? rules.gridWidth : 35;
        public int Height => rules != null ? rules.gridHeight : 17;
        public int CenterLineX => Width / 2;

        public GridCoord PlayerStartTile => new GridCoord(17 / 2, Height / 2);
        public GridCoord EnemyStartTile => new GridCoord(18 + (17 / 2), Height / 2);

        public void Initialize(BattleRulesData battleRules)
        {
            rules = battleRules;
            if (generateOnStart && !HasGeneratedTiles())
                GenerateGrid();
        }

        private void Start()
        {
            if (generateOnStart && rules != null && !HasGeneratedTiles())
                GenerateGrid();
        }

        public bool HasGeneratedTiles()
        {
            return _tiles != null && _tiles.GetLength(0) == Width && _tiles.GetLength(1) == Height;
        }

        [ContextMenu("Generate Grid")]
        public void GenerateGrid()
        {
            if (rules == null)
            {
                Debug.LogError("BattleGridManager needs BattleRulesData assigned.", this);
                return;
            }

            if (originTileTransform == null)
            {
                Debug.LogError("BattleGridManager needs an Origin Tile Transform assigned.", this);
                return;
            }

            if (tilePrefab == null)
            {
                Debug.LogError("BattleGridManager needs a BattleTile prefab assigned.", this);
                return;
            }

            if (gridParent == null)
                gridParent = transform;

            if (clearExistingChildrenBeforeGenerate)
                ClearGeneratedTiles();

            _tiles = new BattleTile[Width, Height];
            _occupants.Clear();

            Vector3 origin = originTileTransform.position;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    GridCoord coord = new GridCoord(x, y);
                    Vector3 world = origin + new Vector3(x * rules.tileSize, 0f, y * rules.tileSize);
                    BattleTile tile = Instantiate(tilePrefab, world, Quaternion.identity, gridParent);
                    tile.transform.localScale = new Vector3(rules.tileSize, tile.transform.localScale.y, rules.tileSize);
                    tile.Initialize(coord, GetSide(coord), GetBaseColor(coord));
                    _tiles[x, y] = tile;
                }
            }
        }

        [ContextMenu("Clear Generated Tiles")]
        public void ClearGeneratedTiles()
        {
            if (gridParent == null)
                return;

            List<GameObject> children = new List<GameObject>();
            for (int i = 0; i < gridParent.childCount; i++)
                children.Add(gridParent.GetChild(i).gameObject);

            foreach (GameObject child in children)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(child);
                else
#endif
                    Destroy(child);
            }

            _tiles = null;
            _occupants.Clear();
        }

        public bool IsInside(GridCoord coord)
        {
            return coord.x >= 0 && coord.x < Width && coord.y >= 0 && coord.y < Height;
        }

        public bool IsPlayerSide(GridCoord coord) => coord.x < CenterLineX;
        public bool IsEnemySide(GridCoord coord) => coord.x > CenterLineX;
        public bool IsCenterLine(GridCoord coord) => coord.x == CenterLineX;

        public Vector3 GetWorldPosition(GridCoord coord)
        {
            BattleTile tile = GetTile(coord);
            if (tile != null)
                return tile.transform.position;

            Vector3 origin = originTileTransform != null ? originTileTransform.position : rules.gridOrigin;
            return origin + new Vector3(coord.x * rules.tileSize, 0f, coord.y * rules.tileSize);
        }

        public GridCoord WorldToGrid(Vector3 worldPosition)
        {
            Vector3 origin = originTileTransform != null ? originTileTransform.position : rules.gridOrigin;
            Vector3 local = worldPosition - origin;
            int x = Mathf.RoundToInt(local.x / rules.tileSize);
            int y = Mathf.RoundToInt(local.z / rules.tileSize);
            return new GridCoord(x, y);
        }

        public bool TryGetTileUnderCursor(Camera camera, out GridCoord coord, out Vector3 hitPoint)
        {
            coord = default;
            hitPoint = default;

            if (camera == null)
                return false;

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 500f, gridRaycastMask))
                return false;

            hitPoint = hit.point;
            BattleTile tile = hit.collider.GetComponentInParent<BattleTile>();
            if (tile == null)
                return false;

            coord = tile.Coord;
            return IsInside(coord);
        }

        public void SetHoverMarker(GridCoord? coord)
        {
            if (hoverMarker == null)
                return;

            if (coord.HasValue && IsInside(coord.Value))
            {
                hoverMarker.gameObject.SetActive(true);
                Vector3 world = GetWorldPosition(coord.Value);
                hoverMarker.position = new Vector3(world.x, rules.telegraphHeight, world.z);
            }
            else
            {
                hoverMarker.gameObject.SetActive(false);
            }
        }

        public bool IsOccupied(GridCoord coord)
        {
            return _occupants.ContainsKey(coord);
        }

        public bool IsOccupiedByOther(GridCoord coord, BeastController requester)
        {
            return _occupants.TryGetValue(coord, out BeastController occupant) && occupant != requester;
        }

        public BeastController GetOccupant(GridCoord coord)
        {
            _occupants.TryGetValue(coord, out BeastController occupant);
            return occupant;
        }

        public void SetOccupant(GridCoord coord, BeastController beast)
        {
            if (!IsInside(coord))
                return;

            _occupants[coord] = beast;
            BattleTile tile = GetTile(coord);
            if (tile != null)
                tile.Occupant = beast;
        }

        public void ClearOccupant(GridCoord coord, BeastController beast)
        {
            if (_occupants.TryGetValue(coord, out BeastController occupant) && occupant == beast)
                _occupants.Remove(coord);

            BattleTile tile = GetTile(coord);
            if (tile != null && tile.Occupant == beast)
                tile.Occupant = null;
        }

        public void MoveOccupant(GridCoord from, GridCoord to, BeastController beast)
        {
            ClearOccupant(from, beast);
            SetOccupant(to, beast);
        }

        public GridCoord Clamp(GridCoord coord)
        {
            return new GridCoord(Mathf.Clamp(coord.x, 0, Width - 1), Mathf.Clamp(coord.y, 0, Height - 1));
        }

        public List<GridCoord> GetTilesInRadius(GridCoord center, float radius)
        {
            List<GridCoord> results = new List<GridCoord>();
            int r = Mathf.CeilToInt(radius);

            for (int x = center.x - r; x <= center.x + r; x++)
            {
                for (int y = center.y - r; y <= center.y + r; y++)
                {
                    GridCoord test = new GridCoord(x, y);
                    if (!IsInside(test))
                        continue;

                    if (GridCoord.Distance(center, test) <= radius + 0.01f)
                        results.Add(test);
                }
            }

            return results;
        }

        public List<GridCoord> BuildLinePath(GridCoord from, GridCoord to)
        {
            List<GridCoord> path = new List<GridCoord>();
            int x0 = from.x;
            int y0 = from.y;
            int x1 = to.x;
            int y1 = to.y;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (!(x0 == x1 && y0 == y1))
            {
                int e2 = err * 2;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }

                GridCoord next = new GridCoord(x0, y0);
                if (IsInside(next))
                    path.Add(next);
                else
                    break;
            }

            return path;
        }

        public BattleTile GetTile(GridCoord coord)
        {
            if (!HasGeneratedTiles() || !IsInside(coord))
                return null;

            return _tiles[coord.x, coord.y];
        }

        private BattleTileSide GetSide(GridCoord coord)
        {
            if (IsCenterLine(coord))
                return BattleTileSide.Center;
            return IsPlayerSide(coord) ? BattleTileSide.Player : BattleTileSide.Enemy;
        }

        private Color GetBaseColor(GridCoord coord)
        {
            if (IsCenterLine(coord))
                return centerLineColor;
            return IsPlayerSide(coord) ? playerSideColor : enemySideColor;
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || rules == null || originTileTransform == null)
                return;

            Gizmos.color = Color.gray;
            Vector3 origin = originTileTransform.position;
            float half = rules.tileSize * 0.5f;

            for (int x = 0; x <= Width; x++)
            {
                Vector3 start = origin + new Vector3(x * rules.tileSize - half, 0f, -half);
                Vector3 end = origin + new Vector3(x * rules.tileSize - half, 0f, (Height - 1) * rules.tileSize + half);
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= Height; y++)
            {
                Vector3 start = origin + new Vector3(-half, 0f, y * rules.tileSize - half);
                Vector3 end = origin + new Vector3((Width - 1) * rules.tileSize + half, 0f, y * rules.tileSize - half);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
