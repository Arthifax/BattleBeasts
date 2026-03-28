using System.Collections;
using UnityEngine;

namespace BattleBeasts
{
    [RequireComponent(typeof(BeastController))]
    public class BeastActionExecutor : MonoBehaviour
    {
        private BeastController _controller;
        private BattleGridManager _grid;
        private AttackTelegraphSystem _telegraphs;
        private Coroutine _actionRoutine;

        public void Initialize(BeastController controller, BattleGridManager grid, AttackTelegraphSystem telegraphSystem)
        {
            _controller = controller;
            _grid = grid;
            _telegraphs = telegraphSystem;
        }

        public bool TryUseAbility(AbilityData ability)
        {
            if (_controller == null || ability == null || _controller.Opponent == null)
                return false;

            if (_controller.Runtime.IsBusy && _controller.Runtime.actionState != BeastActionState.Idle)
                return false;

            if (!IsAbilityOffCooldown(ability))
                return false;

            GridCoord targetTile = _controller.Opponent.Runtime.currentTile;
            float distance = GridCoord.Distance(_controller.Runtime.currentTile, targetTile);
            if (distance > ability.rangeInTiles)
                return false;

            if (_actionRoutine != null)
                StopCoroutine(_actionRoutine);

            _actionRoutine = StartCoroutine(AbilityRoutine(ability, targetTile));
            return true;
        }

        public bool TryDodgeAway()
        {
            if (_controller.Runtime.dodgeCooldownRemaining > 0f)
                return false;

            if (_controller.Opponent == null)
                return false;

            GridCoord away = GetDodgeDestination();
            if (away == _controller.Runtime.currentTile)
                return false;

            _controller.Runtime.dodgeCooldownRemaining = 3f;
            return _controller.Mover.TryMoveTo(away, _grid.Rules.dodgeRecovery);
        }

        private IEnumerator AbilityRoutine(AbilityData ability, GridCoord targetTile)
        {
            _controller.Runtime.actionState = BeastActionState.Windup;
            _controller.Runtime.currentWindupDuration = ability.windupTime;
            _controller.Runtime.currentWindupElapsed = 0f;
            _controller.Runtime.lastCommittedTargetTile = targetTile;

            if (ability == _controller.Data.basicAttack)
                _controller.Runtime.basicCooldownRemaining = ability.cooldown;
            else if (ability == _controller.Data.specialAbility)
                _controller.Runtime.specialCooldownRemaining = ability.cooldown;

            AttackTelegraphSystem.ActiveTelegraph telegraph = null;
            if (_telegraphs != null)
                telegraph = _telegraphs.CreateTelegraph(_controller, targetTile, ability.impactRadiusInTiles, ability.telegraphColor);

            while (_controller.Runtime.currentWindupElapsed < ability.windupTime)
            {
                _controller.Runtime.currentWindupElapsed += Time.deltaTime;
                yield return null;
            }

            if (telegraph != null)
                _telegraphs.ClearTelegraph(telegraph);

            ApplyAbilityImpact(ability, targetTile);

            _controller.Runtime.currentWindupDuration = 0f;
            _controller.Runtime.currentWindupElapsed = 0f;
            _controller.EnterRecovery(ability.recoveryTime);
            _actionRoutine = null;
        }

        private void ApplyAbilityImpact(AbilityData ability, GridCoord impactTile)
        {
            BeastController opponent = _controller.Opponent;
            if (opponent == null)
                return;

            if (GridCoord.Distance(opponent.Runtime.currentTile, impactTile) <= ability.impactRadiusInTiles + 0.01f)
            {
                int damage = CalculateDamage(_controller, opponent, ability);
                opponent.TakeDamage(damage);
                _controller.Manager.Log($"{_controller.Data.speciesName} hit {opponent.Data.speciesName} with {ability.abilityName} for {damage}.");
            }
            else
            {
                _controller.Manager.Log($"{_controller.Data.speciesName}'s {ability.abilityName} missed.");
            }
        }

        public GridCoord GetDodgeDestination()
        {
            GridCoord current = _controller.Runtime.currentTile;
            GridCoord opponent = _controller.Opponent.Runtime.currentTile;

            int dx = current.x - opponent.x;
            int dy = current.y - opponent.y;

            if (dx == 0 && dy == 0)
                dx = _controller.Team == TeamAlignment.Player ? -1 : 1;

            Vector2 dir = new Vector2(dx, dy).normalized;
            int dodgeDistance = Mathf.Max(1, _controller.Data.dodgeDistanceInTiles);
            GridCoord desired = new GridCoord(
                current.x + Mathf.RoundToInt(dir.x * dodgeDistance),
                current.y + Mathf.RoundToInt(dir.y * dodgeDistance));

            desired = _grid.Clamp(desired);

            if (_grid.IsOccupiedByOther(desired, _controller))
            {
                for (int radius = 1; radius <= dodgeDistance; radius++)
                {
                    for (int ox = -radius; ox <= radius; ox++)
                    {
                        for (int oy = -radius; oy <= radius; oy++)
                        {
                            GridCoord alt = _grid.Clamp(new GridCoord(desired.x + ox, desired.y + oy));
                            if (!_grid.IsOccupiedByOther(alt, _controller))
                                return alt;
                        }
                    }
                }

                return current;
            }

            return desired;
        }

        private bool IsAbilityOffCooldown(AbilityData ability)
        {
            if (ability == _controller.Data.basicAttack)
                return _controller.Runtime.basicCooldownRemaining <= 0f;

            if (ability == _controller.Data.specialAbility)
                return _controller.Runtime.specialCooldownRemaining <= 0f;

            return true;
        }

        private static int CalculateDamage(BeastController source, BeastController target, AbilityData ability)
        {
            int attack = ability.damageCategory == DamageCategory.Physical
                ? source.Data.baseStats.attack
                : source.Data.baseStats.specialAttack;

            int defense = ability.damageCategory == DamageCategory.Physical
                ? target.Data.baseStats.defense
                : target.Data.baseStats.specialDefense;

            float stab = source.Data.beastType == ability.type ? 1.15f : 1f;
            float typeMultiplier = GetTypeMultiplier(ability.type, target.Data.beastType);

            float raw = ((ability.power + attack) - (defense * 0.6f)) * stab * typeMultiplier;
            return Mathf.Max(1, Mathf.RoundToInt(raw));
        }

        private static float GetTypeMultiplier(BeastType attackType, BeastType defenseType)
        {
            if (attackType == BeastType.Fire && defenseType == BeastType.Grass) return 1.25f;
            if (attackType == BeastType.Grass && defenseType == BeastType.Water) return 1.25f;
            if (attackType == BeastType.Water && defenseType == BeastType.Fire) return 1.25f;

            if (attackType == BeastType.Fire && defenseType == BeastType.Water) return 0.8f;
            if (attackType == BeastType.Grass && defenseType == BeastType.Fire) return 0.8f;
            if (attackType == BeastType.Water && defenseType == BeastType.Grass) return 0.8f;

            return 1f;
        }
    }
}
