using UnityEngine;

namespace BattleBeasts
{
    public class BattleGridClickInput : MonoBehaviour
    {
        [SerializeField] private Camera battleCamera;
        [SerializeField] private LayerMask tileMask = ~0;

        public BattleTile HoveredTile { get; private set; }
        public BattleTile ClickedTile { get; private set; }

        private void Update()
        {
            if (battleCamera == null)
                return;

            UpdateHoveredTile();

            if (Input.GetMouseButtonDown(0) && HoveredTile != null)
            {
                ClickedTile = HoveredTile;
                Debug.Log($"Clicked tile object: {ClickedTile.name}");
            }
        }

        private void UpdateHoveredTile()
        {
            Ray ray = battleCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 500f, tileMask))
            {
                BattleTile tile = hit.collider.GetComponentInParent<BattleTile>();
                if (tile != null)
                {
                    HoveredTile = tile;
                    return;
                }
            }

            HoveredTile = null;
        }
    }
}