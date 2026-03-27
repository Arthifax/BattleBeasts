using System.Collections;
using UnityEngine;

namespace BattleBeasts
{
    [RequireComponent(typeof(BeastBrain))]
    [RequireComponent(typeof(BeastMotor))]
    [RequireComponent(typeof(BeastCombat))]
    [RequireComponent(typeof(BeastCommandReceiver))]
    public class BeastController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private BeastData beastData;
        [SerializeField] private TeamAlignment teamAlignment = TeamAlignment.Enemy;
        [SerializeField, Range(0f, 100f)] private float startingBond = 50f;

        [Header("Runtime")]
        [SerializeField] private BeastController opponent;
        [SerializeField] private BeastState state = new BeastState();

        private BeastBrain brain;
        private BeastMotor motor;
        private BeastCombat combat;
        private BeastCommandReceiver commandReceiver;

        private Coroutine activeRoutine;

        public BeastData BeastData => beastData;
        public BeastState State => state;
        public TeamAlignment TeamAlignment => teamAlignment;
        public BeastController Opponent => opponent;
        public BeastBrain Brain => brain;
        public BeastMotor Motor => motor;
        public BeastCombat Combat => combat;
        public BeastCommandReceiver CommandReceiver => commandReceiver;
        public string DisplayName => beastData != null ? beastData.speciesName : gameObject.name;

        private void Awake()
        {
            brain = GetComponent<BeastBrain>();
            motor = GetComponent<BeastMotor>();
            combat = GetComponent<BeastCombat>();
            commandReceiver = GetComponent<BeastCommandReceiver>();

            state.Initialize(beastData, teamAlignment, startingBond);

            brain.Initialize(this);
            motor.Initialize(this);
            combat.Initialize(this);
            commandReceiver.Initialize(this);
        }

        private void Start()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.RegisterParticipant(this);
            }
        }

        private void Update()
        {
            state.Tick(Time.deltaTime);
            commandReceiver.Tick(Time.deltaTime);
        }

        public void BeginBattle()
        {
            if (state.IsDefeated)
            {
                return;
            }

            state.currentBattleState = BeastBattleState.Idle;
            brain.SetBattleActive(true);
        }

        public void SetOpponent(BeastController newOpponent)
        {
            opponent = newOpponent;
        }

        public void ReceiveCommand(CommandIntent intent)
        {
            commandReceiver.ReceiveCommand(intent);
        }

        public bool IsBusy()
        {
            return activeRoutine != null;
        }

        public void StartActionRoutine(IEnumerator routine)
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
            }

            activeRoutine = StartCoroutine(RunAction(routine));
        }

        public void InterruptCurrentAction()
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }

            motor.StopMoving();
            state.currentBattleState = BeastBattleState.Idle;
        }

        public void ApplyDamage(float amount)
        {
            if (state.IsDefeated)
            {
                return;
            }

            int damage = Mathf.Max(1, Mathf.RoundToInt(amount));
            state.currentHP = Mathf.Max(0, state.currentHP - damage);
            BattleManager.Instance?.Log($"{DisplayName} takes {damage} damage. ({state.currentHP} HP left)");

            if (state.currentHP <= 0)
            {
                state.currentBattleState = BeastBattleState.Defeated;
                brain.SetBattleActive(false);
                motor.StopMoving();
            }
        }

        public void ApplySelfBuff(float multiplier, float duration)
        {
            state.buffMultiplier = Mathf.Max(1f, multiplier);
            state.buffRemaining = Mathf.Max(0f, duration);
            BattleManager.Instance?.Log($"{DisplayName} is empowered!");
        }

        private IEnumerator RunAction(IEnumerator routine)
        {
            yield return StartCoroutine(routine);
            activeRoutine = null;

            if (!state.IsDefeated)
            {
                state.currentBattleState = BeastBattleState.Idle;
            }
        }
    }
}
