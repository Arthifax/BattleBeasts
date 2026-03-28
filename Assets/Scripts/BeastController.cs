using UnityEngine;

namespace BattleBeasts
{
    [RequireComponent(typeof(BeastGridMover))]
    [RequireComponent(typeof(BeastActionExecutor))]
    public class BeastController : MonoBehaviour
    {
        [SerializeField] private BeastData data;
        [SerializeField] private TeamAlignment team;
        [SerializeField] private Renderer[] tintRenderers;

        private BeastRuntimeState _runtime = new BeastRuntimeState();
        private BattleManager _manager;
        private BattleGridManager _grid;
        private BeastGridMover _mover;
        private BeastActionExecutor _actionExecutor;

        public BeastData Data => data;
        public TeamAlignment Team => team;
        public BeastRuntimeState Runtime => _runtime;
        public BeastController Opponent { get; private set; }
        public BattleManager Manager => _manager;
        public BeastGridMover Mover => _mover;
        public BeastActionExecutor Actions => _actionExecutor;

        public void Initialize(BattleManager manager, BattleGridManager grid, BeastController opponent, GridCoord startTile)
        {
            _manager = manager;
            _grid = grid;
            Opponent = opponent;

            _mover = GetComponent<BeastGridMover>();
            _actionExecutor = GetComponent<BeastActionExecutor>();

            _mover.Initialize(this, grid);
            _actionExecutor.Initialize(this, grid, manager.Telegraphs);

            _runtime = new BeastRuntimeState
            {
                currentHP = data.baseStats.maxHP,
                currentTile = startTile,
                actionState = BeastActionState.Idle
            };

            transform.position = grid.GetWorldPosition(startTile);
            _grid.SetOccupant(startTile, this);

            ApplyTint();
        }

        private void Update()
        {
            _runtime.Tick(Time.deltaTime);

            if (_runtime.actionState == BeastActionState.Idle && _runtime.isCommandQueued)
            {
                BattleCommand queued = _runtime.queuedCommand;
                _runtime.isCommandQueued = false;
                ExecuteCommand(queued);
            }

            TryAutoDodgeThreat();
        }

        public bool ExecuteCommand(BattleCommand command)
        {
            if (data == null || _runtime.actionState == BeastActionState.Disabled)
                return false;

            if (_runtime.actionState == BeastActionState.Windup)
            {
                _runtime.queuedCommand = command;
                _runtime.isCommandQueued = true;
                return false;
            }

            switch (command.commandType)
            {
                case CommandType.MoveToTile:
                    return _mover.TryMoveTo(command.targetTile, _grid.Rules.defaultMoveRecovery);

                case CommandType.UseBasicAttack:
                case CommandType.UseSpecialAttack:
                    return _actionExecutor.TryUseAbility(command.ability);

                case CommandType.DodgeAway:
                    return _actionExecutor.TryDodgeAway();
            }

            return false;
        }

        public void EnterRecovery(float duration)
        {
            _runtime.recoveryRemaining = duration;
            _runtime.actionState = duration > 0f ? BeastActionState.Recovering : BeastActionState.Idle;
        }

        public void TakeDamage(int amount)
        {
            if (_runtime.actionState == BeastActionState.Disabled)
                return;

            _runtime.currentHP = Mathf.Max(0, _runtime.currentHP - amount);

            if (_runtime.currentHP <= 0)
            {
                _runtime.actionState = BeastActionState.Disabled;
                _grid.ClearOccupant(_runtime.currentTile, this);
                _manager.NotifyBeastDefeated(this);
            }
        }

        private void TryAutoDodgeThreat()
        {
            if (_manager == null || _manager.Telegraphs == null)
                return;

            if (_runtime.actionState != BeastActionState.Idle)
                return;

            if (_runtime.dodgeCooldownRemaining > 0f)
                return;

            if (!_manager.Telegraphs.IsTileThreatenedByEnemy(_runtime.currentTile, team))
                return;

            if (Random.value <= data.autoDodgeChance)
            {
                ExecuteCommand(BattleCommand.DodgeAway());
            }
        }

        private void ApplyTint()
        {
            if (tintRenderers == null)
                return;

            Color color = team == TeamAlignment.Player ? new Color(0.35f, 0.8f, 1f) : new Color(1f, 0.45f, 0.45f);

            foreach (Renderer renderer in tintRenderers)
            {
                if (renderer != null && renderer.material != null)
                    renderer.material.color = color;
            }
        }
    }
}
