using UnityEngine;

namespace BattleBeasts
{
    [System.Serializable]
    public class BeastRuntimeState
    {
        public int currentHP;
        public BeastActionState actionState = BeastActionState.Idle;
        public GridCoord currentTile;
        public GridCoord lastCommittedTargetTile;
        public float recoveryRemaining;
        public float basicCooldownRemaining;
        public float specialCooldownRemaining;
        public float dodgeCooldownRemaining;
        public float currentWindupDuration;
        public float currentWindupElapsed;
        public bool isCommandQueued;
        public BattleCommand queuedCommand;

        public bool IsBusy => actionState == BeastActionState.Moving || actionState == BeastActionState.Windup || actionState == BeastActionState.Recovering;
        public bool IsRecovering => actionState == BeastActionState.Recovering;

        public void Tick(float deltaTime)
        {
            if (recoveryRemaining > 0f)
                recoveryRemaining = Mathf.Max(0f, recoveryRemaining - deltaTime);

            if (basicCooldownRemaining > 0f)
                basicCooldownRemaining = Mathf.Max(0f, basicCooldownRemaining - deltaTime);

            if (specialCooldownRemaining > 0f)
                specialCooldownRemaining = Mathf.Max(0f, specialCooldownRemaining - deltaTime);

            if (dodgeCooldownRemaining > 0f)
                dodgeCooldownRemaining = Mathf.Max(0f, dodgeCooldownRemaining - deltaTime);

            if (actionState == BeastActionState.Recovering && recoveryRemaining <= 0f)
                actionState = BeastActionState.Idle;
        }
    }
}
