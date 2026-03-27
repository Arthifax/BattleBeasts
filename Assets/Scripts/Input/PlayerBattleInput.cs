using UnityEngine;

namespace BattleBeasts
{
    public class PlayerBattleInput : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private Camera battleCamera;
        [SerializeField] private LayerMask groundLayer = ~0;
        [SerializeField] private KeyCode moveKey = KeyCode.Mouse1;
        [SerializeField] private KeyCode basicAttackKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode specialAttackKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode evadeKey = KeyCode.Space;
        [SerializeField] private KeyCode aggressiveKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode defensiveKey = KeyCode.Alpha4;
        [SerializeField] private KeyCode neutralKey = KeyCode.Alpha5;

        private BeastController playerBeast;

        private void Start()
        {
            if (battleCamera == null)
            {
                battleCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (BattleManager.Instance == null || !BattleManager.Instance.BattleActive)
            {
                return;
            }

            playerBeast = BattleManager.Instance.PlayerBeast;
            if (playerBeast == null || playerBeast.State.IsDefeated)
            {
                return;
            }

            if (Input.GetKeyDown(basicAttackKey))
            {
                playerBeast.ReceiveCommand(CommandIntent.CreateAction(CommandType.BasicAttack, Time.time));
            }

            if (Input.GetKeyDown(specialAttackKey))
            {
                playerBeast.ReceiveCommand(CommandIntent.CreateAction(CommandType.SpecialAttack, Time.time));
            }

            if (Input.GetKeyDown(evadeKey))
            {
                playerBeast.ReceiveCommand(CommandIntent.CreateAction(CommandType.Evade, Time.time));
            }

            if (Input.GetKeyDown(aggressiveKey))
            {
                playerBeast.ReceiveCommand(CommandIntent.CreateStrategy(StrategyMode.Aggressive, Time.time));
            }

            if (Input.GetKeyDown(defensiveKey))
            {
                playerBeast.ReceiveCommand(CommandIntent.CreateStrategy(StrategyMode.Defensive, Time.time));
            }

            if (Input.GetKeyDown(neutralKey))
            {
                playerBeast.ReceiveCommand(CommandIntent.CreateStrategy(StrategyMode.Neutral, Time.time));
            }

            if (Input.GetKeyDown(moveKey))
            {
                TryIssueMoveCommand();
            }
        }

        private void TryIssueMoveCommand()
        {
            if (battleCamera == null)
            {
                return;
            }

            Ray ray = battleCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 250f, groundLayer))
            {
                playerBeast.ReceiveCommand(CommandIntent.CreateMove(hit.point, Time.time));
            }
        }
    }
}
