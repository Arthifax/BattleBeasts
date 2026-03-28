using System.Collections.Generic;
using UnityEngine;

namespace BattleBeasts
{
    public class AttackTelegraphSystem : MonoBehaviour
    {
        [SerializeField] private BattleGridManager grid;
        [SerializeField] private GameObject tileMarkerPrefab;

        private readonly List<ActiveTelegraph> _activeTelegraphs = new List<ActiveTelegraph>();

        public void Initialize(BattleGridManager battleGrid)
        {
            grid = battleGrid;
        }

        public ActiveTelegraph CreateTelegraph(BeastController source, GridCoord center, float radius, Color color)
        {
            ActiveTelegraph telegraph = new ActiveTelegraph
            {
                source = source,
                center = center,
                radius = radius,
                threatenedTiles = grid.GetTilesInRadius(center, radius)
            };

            if (tileMarkerPrefab != null)
            {
                foreach (GridCoord tile in telegraph.threatenedTiles)
                {
                    Vector3 pos = grid.GetWorldPosition(tile);
                    GameObject marker = Instantiate(tileMarkerPrefab, new Vector3(pos.x, grid.Rules.telegraphHeight, pos.z), Quaternion.identity, transform);
                    Renderer renderer = marker.GetComponentInChildren<Renderer>();
                    if (renderer != null && renderer.material != null)
                        renderer.material.color = color;
                    telegraph.visuals.Add(marker);
                }
            }

            _activeTelegraphs.Add(telegraph);
            return telegraph;
        }

        public void ClearTelegraph(ActiveTelegraph telegraph)
        {
            if (telegraph == null)
                return;

            foreach (GameObject obj in telegraph.visuals)
            {
                if (obj != null)
                    Destroy(obj);
            }

            _activeTelegraphs.Remove(telegraph);
        }

        public bool IsTileThreatenedByEnemy(GridCoord tile, TeamAlignment friendlyTeam)
        {
            for (int i = 0; i < _activeTelegraphs.Count; i++)
            {
                ActiveTelegraph telegraph = _activeTelegraphs[i];
                if (telegraph == null || telegraph.source == null)
                    continue;

                if (telegraph.source.Team == friendlyTeam)
                    continue;

                if (telegraph.threatenedTiles.Contains(tile))
                    return true;
            }

            return false;
        }

        public class ActiveTelegraph
        {
            public BeastController source;
            public GridCoord center;
            public float radius;
            public List<GridCoord> threatenedTiles = new List<GridCoord>();
            public List<GameObject> visuals = new List<GameObject>();
        }
    }
}
