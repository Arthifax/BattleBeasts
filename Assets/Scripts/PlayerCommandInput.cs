using UnityEngine;

namespace BattleBeasts
{
    public class PlayerCommandInput : MonoBehaviour
    {
        [SerializeField] private Camera battleCamera;
        [SerializeField] private KeyCode slowMoModifier = KeyCode.LeftShift;
        [SerializeField] private KeyCode basicAttackKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode specialAttackKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode dodgeKey = KeyCode.Space;

        private BattleManager _manager;
        private BeastController _playerBeast;

        public void Initialize(BattleManager manager, BeastController playerBeast)
        {
            _manager = manager;
            _playerBeast = playerBeast;
        }

        private void Update()
        {
            if (_manager == null || _playerBeast == null || !_manager.BattleActive)
                return;

            HandleSlowMo();

            if (_manager.Grid.TryGetTileUnderCursor(battleCamera, out GridCoord tile, out _))
            {
                _manager.Grid.SetHoverMarker(tile);
                _manager.UIManager?.SetHoveredTile(tile);

                if (Input.GetMouseButtonDown(0))
                {
                    if (!_manager.Grid.IsCenterLine(tile) && !_manager.Grid.IsOccupiedByOther(tile, _playerBeast))
                    {
                        _playerBeast.ExecuteCommand(BattleCommand.Move(tile));
                    }
                }
            }
            else
            {
                _manager.Grid.SetHoverMarker(null);
                _manager.UIManager?.SetHoveredTile(null);
            }

            if (Input.GetKeyDown(basicAttackKey) && _playerBeast.Data.basicAttack != null)
            {
                _playerBeast.ExecuteCommand(BattleCommand.Basic(_playerBeast.Data.basicAttack));
            }

            if (Input.GetKeyDown(specialAttackKey) && _playerBeast.Data.specialAbility != null)
            {
                _playerBeast.ExecuteCommand(BattleCommand.Special(_playerBeast.Data.specialAbility));
            }

            if (Input.GetKeyDown(dodgeKey))
            {
                _playerBeast.ExecuteCommand(BattleCommand.DodgeAway());
            }

            _manager.UIManager?.SetCommandHint("LMB Move  |  1 Basic  |  2 Special  |  Space Dodge  |  Hold Left Shift for slow-mo");
        }

        private void HandleSlowMo()
        {
            float targetScale = Input.GetKey(slowMoModifier) ? _manager.Rules.commandSlowMoScale : 1f;
            _manager.SetBattleTimeScale(targetScale);
        }
    }
}
