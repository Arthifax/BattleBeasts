using UnityEngine;

namespace BattleBeasts
{
    public enum BattleTileSide
    {
        Player,
        Center,
        Enemy
    }

    [RequireComponent(typeof(Collider))]
    public class BattleTile : MonoBehaviour
    {
        [SerializeField] private Renderer tileRenderer;

        public GridCoord Coord { get; private set; }
        public BattleTileSide Side { get; private set; }
        public BeastController Occupant { get; set; }

        private MaterialPropertyBlock _mpb;
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private Color _baseColor = Color.white;

        private void Awake()
        {
            if (tileRenderer == null)
                tileRenderer = GetComponentInChildren<Renderer>();

            _mpb = new MaterialPropertyBlock();
        }

        public void Initialize(GridCoord coord, BattleTileSide side, Color baseColor)
        {
            Coord = coord;
            Side = side;
            _baseColor = baseColor;
            Occupant = null;
            gameObject.name = $"Tile_{coord.x}_{coord.y}";
            SetColor(baseColor);
        }

        public void SetColor(Color color)
        {
            if (tileRenderer == null)
                return;

            tileRenderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(ColorId, color);
            tileRenderer.SetPropertyBlock(_mpb);
        }

        public void ResetColor()
        {
            SetColor(_baseColor);
        }
    }
}
