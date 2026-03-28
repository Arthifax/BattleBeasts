using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleBeasts
{
    [RequireComponent(typeof(BeastController))]
    public class BeastGridMover : MonoBehaviour
    {
        private BeastController _controller;
        private BattleGridManager _grid;
        private Coroutine _moveRoutine;

        public bool IsMoving => _moveRoutine != null;

        public void Initialize(BeastController controller, BattleGridManager grid)
        {
            _controller = controller;
            _grid = grid;
        }

        public bool TryMoveTo(GridCoord destination, float postRecovery)
        {
            if (_controller == null || _grid == null || !_grid.IsInside(destination))
                return false;

            if (_controller.Runtime.actionState == BeastActionState.Windup)
                return false;

            if (_grid.IsOccupiedByOther(destination, _controller))
                return false;

            if (_moveRoutine != null)
                StopCoroutine(_moveRoutine);

            _moveRoutine = StartCoroutine(MoveRoutine(destination, postRecovery));
            return true;
        }

        private IEnumerator MoveRoutine(GridCoord destination, float postRecovery)
        {
            _controller.Runtime.actionState = BeastActionState.Moving;
            _controller.Runtime.lastCommittedTargetTile = destination;

            List<GridCoord> path = _grid.BuildLinePath(_controller.Runtime.currentTile, destination);
            float speed = Mathf.Max(0.1f, _controller.Data.moveSpeedTilesPerSecond);

            foreach (GridCoord step in path)
            {
                if (_grid.IsOccupiedByOther(step, _controller))
                    break;

                GridCoord previous = _controller.Runtime.currentTile;
                Vector3 targetPos = _grid.GetWorldPosition(step);

                while (Vector3.Distance(transform.position, targetPos) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * _grid.Rules.tileSize * Time.deltaTime);
                    yield return null;
                }

                transform.position = targetPos;
                _controller.Runtime.currentTile = step;
                _grid.MoveOccupant(previous, step, _controller);
            }

            _controller.EnterRecovery(postRecovery);
            _moveRoutine = null;
        }
    }
}
