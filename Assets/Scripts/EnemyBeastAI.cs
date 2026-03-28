using UnityEngine;

namespace BattleBeasts
{
    [RequireComponent(typeof(BeastController))]
    public class EnemyBeastAI : MonoBehaviour
    {
        private BeastController _beast;
        private BattleManager _manager;
        private float _decisionTimer;
        private float _postActionTimer;

        public void Initialize(BattleManager manager)
        {
            _manager = manager;
            _beast = GetComponent<BeastController>();
            _decisionTimer = _manager.Rules.enemyDecisionInterval;
        }

        private void Update()
        {
            if (_manager == null || _beast == null || !_manager.BattleActive)
                return;

            if (_beast.Team != TeamAlignment.Enemy)
                return;

            if (_beast.Runtime.actionState == BeastActionState.Disabled)
                return;

            if (_postActionTimer > 0f)
            {
                _postActionTimer -= Time.deltaTime;
                return;
            }

            if (_beast.Runtime.IsBusy)
                return;

            _decisionTimer -= Time.deltaTime;
            if (_decisionTimer > 0f)
                return;

            ChooseAction();
            _decisionTimer = _manager.Rules.enemyDecisionInterval;
            _postActionTimer = _manager.Rules.enemyPostActionDelay;
        }

        private void ChooseAction()
        {
            BeastController target = _beast.Opponent;
            if (target == null)
                return;

            float distance = GridCoord.Distance(_beast.Runtime.currentTile, target.Runtime.currentTile);

            AbilityData special = _beast.Data.specialAbility;
            AbilityData basic = _beast.Data.basicAttack;

            if (special != null && _beast.Runtime.specialCooldownRemaining <= 0f && distance <= special.rangeInTiles)
            {
                if (Random.value < 0.55f)
                {
                    _beast.ExecuteCommand(BattleCommand.Special(special));
                    _manager.Log($"{_beast.Data.speciesName} begins charging {special.abilityName}.");
                    return;
                }
            }

            if (basic != null && _beast.Runtime.basicCooldownRemaining <= 0f && distance <= basic.rangeInTiles)
            {
                _beast.ExecuteCommand(BattleCommand.Basic(basic));
                _manager.Log($"{_beast.Data.speciesName} uses {basic.abilityName}.");
                return;
            }

            GridCoord moveTarget = FindApproachTile(target.Runtime.currentTile, _beast.Data.defaultApproachRangeInTiles);
            if (moveTarget != _beast.Runtime.currentTile)
            {
                _beast.ExecuteCommand(BattleCommand.Move(moveTarget));
                _manager.Log($"{_beast.Data.speciesName} repositions.");
            }
        }

        private GridCoord FindApproachTile(GridCoord enemyTile, int preferredDistance)
        {
            GridCoord current = _beast.Runtime.currentTile;
            GridCoord best = current;
            float bestScore = float.MinValue;

            for (int x = -9; x <= 9; x++)
            {
                for (int y = -9; y <= 9; y++)
                {
                    GridCoord test = new GridCoord(current.x + x, current.y + y);
                    test = _manager.Grid.Clamp(test);

                    if (!_manager.Grid.IsInside(test))
                        continue;

                    if (_manager.Grid.IsCenterLine(test))
                        continue;

                    if (_manager.Grid.IsOccupiedByOther(test, _beast))
                        continue;

                    float distToEnemy = GridCoord.Distance(test, enemyTile);
                    float distToPreferred = Mathf.Abs(distToEnemy - preferredDistance);
                    float sideBonus = _beast.Team == TeamAlignment.Enemy && _manager.Grid.IsEnemySide(test) ? 0.2f : 0f;
                    float score = -distToPreferred + sideBonus - (GridCoord.Distance(current, test) * 0.05f);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = test;
                    }
                }
            }

            return best;
        }
    }
}
