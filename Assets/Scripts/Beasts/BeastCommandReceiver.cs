using UnityEngine;

namespace BattleBeasts
{
    public class BeastCommandReceiver : MonoBehaviour
    {
        [Header("Command Response")]
        [SerializeField] private bool allowInterrupts = true;
        [SerializeField] private float minResponseDelay = 0.05f;
        [SerializeField] private float maxResponseDelay = 0.75f;

        private BeastController controller;
        private float responseDelayRemaining;

        public void Initialize(BeastController beastController)
        {
            controller = beastController;
        }

        public void Tick(float deltaTime)
        {
            responseDelayRemaining = Mathf.Max(0f, responseDelayRemaining - deltaTime);

            CommandIntent active = controller.State.activeCommand;
            if (active != null && active.IsExpired(Time.time))
            {
                ClearCommand();
            }
        }

        public void ReceiveCommand(CommandIntent intent)
        {
            if (controller == null || controller.State.IsDefeated)
            {
                return;
            }

            float obedience = Mathf.Clamp01(controller.State.currentBond / 100f);
            bool willObey = Random.value <= Mathf.Lerp(0.55f, 0.98f, obedience);
            if (!willObey)
            {
                BattleManager.Instance?.Log($"{controller.DisplayName} ignores the command!");
                controller.State.hesitationRemaining = 0.2f;
                return;
            }

            controller.State.activeCommand = intent;
            responseDelayRemaining = Mathf.Lerp(maxResponseDelay, minResponseDelay, obedience);
            BattleManager.Instance?.Log($"Trainer instructs {controller.DisplayName}: {intent.commandType}");

            if (allowInterrupts && controller.IsBusy())
            {
                float interruptChance = Mathf.Lerp(0.2f, 0.9f, obedience);
                if (Random.value <= interruptChance)
                {
                    controller.InterruptCurrentAction();
                }
            }
        }

        public CommandIntent GetActiveCommand()
        {
            if (responseDelayRemaining > 0f)
            {
                return null;
            }

            return controller.State.activeCommand;
        }

        public void ClearCommand()
        {
            controller.State.activeCommand = null;
            responseDelayRemaining = 0f;
        }
    }
}
