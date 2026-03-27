using UnityEngine;

namespace BattleBeasts
{
    public class BeastBrain : MonoBehaviour
    {
        [Header("Decision Making")]
        [SerializeField] private float decisionInterval = 0.5f;
        [SerializeField] private float engageDistance = 2f;
        [SerializeField] private float defensiveRetreatDistance = 4f;

        private BeastController controller;
        private float decisionTimer;
        private bool battleActive;

        public void Initialize(BeastController beastController)
        {
            controller = beastController;
            decisionTimer = decisionInterval;
        }

        public void SetBattleActive(bool value)
        {
            battleActive = value;
        }

        private void Update()
        {
            if (!battleActive || controller == null || controller.Opponent == null)
            {
                return;
            }

            if (controller.State.IsDefeated || controller.Opponent.State.IsDefeated)
            {
                return;
            }

            if (controller.IsBusy() || controller.State.hesitationRemaining > 0f)
            {
                return;
            }

            decisionTimer -= Time.deltaTime;
            if (decisionTimer > 0f)
            {
                return;
            }

            decisionTimer = decisionInterval;
            DecideNextAction();
        }

        private void DecideNextAction()
        {
            controller.State.currentBattleState = BeastBattleState.Assessing;

            CommandIntent command = controller.CommandReceiver.GetActiveCommand();
            if (command != null)
            {
                bool handled = TryHandleCommand(command);
                if (handled)
                {
                    return;
                }
            }

            float distance = Vector3.Distance(controller.transform.position, controller.Opponent.transform.position);
            bool enemyCasting = controller.Opponent.State.currentBattleState == BeastBattleState.WindingUp;

            if (enemyCasting && controller.State.currentStrategy != StrategyMode.Aggressive)
            {
                controller.StartActionRoutine(controller.Combat.EvadeRoutine());
                return;
            }

            if (controller.State.currentStrategy == StrategyMode.Defensive && distance < engageDistance)
            {
                Vector3 awayDirection = (controller.transform.position - controller.Opponent.transform.position).normalized;
                Vector3 retreatPoint = controller.transform.position + awayDirection * defensiveRetreatDistance;
                controller.StartActionRoutine(controller.Motor.MoveToRoutine(retreatPoint));
                return;
            }

            if (distance <= controller.Combat.GetBasicAttackRange() && controller.Combat.CanUseBasicAttack())
            {
                controller.StartActionRoutine(controller.Combat.BasicAttackRoutine());
                return;
            }

            if (distance <= controller.Combat.GetSpecialAttackRange() && controller.Combat.CanUseSpecialAttack())
            {
                controller.StartActionRoutine(controller.Combat.SpecialAttackRoutine());
                return;
            }

            controller.StartActionRoutine(controller.Motor.MoveToRoutine(controller.Opponent.transform.position));
        }

        private bool TryHandleCommand(CommandIntent command)
        {
            switch (command.commandType)
            {
                case CommandType.Move:
                    controller.StartActionRoutine(controller.Motor.MoveToRoutine(command.targetPosition));
                    controller.CommandReceiver.ClearCommand();
                    return true;

                case CommandType.BasicAttack:
                    if (controller.Combat.CanUseBasicAttack())
                    {
                        controller.StartActionRoutine(controller.Combat.BasicAttackRoutine());
                        controller.CommandReceiver.ClearCommand();
                        return true;
                    }
                    break;

                case CommandType.SpecialAttack:
                    if (controller.Combat.CanUseSpecialAttack())
                    {
                        controller.StartActionRoutine(controller.Combat.SpecialAttackRoutine());
                        controller.CommandReceiver.ClearCommand();
                        return true;
                    }
                    break;

                case CommandType.Evade:
                    controller.StartActionRoutine(controller.Combat.EvadeRoutine());
                    controller.CommandReceiver.ClearCommand();
                    return true;

                case CommandType.SetStrategy:
                    controller.State.currentStrategy = command.requestedStrategy;
                    BattleManager.Instance?.Log($"{controller.DisplayName} switches to {command.requestedStrategy} mode.");
                    controller.CommandReceiver.ClearCommand();
                    return true;
            }

            return false;
        }
    }
}
