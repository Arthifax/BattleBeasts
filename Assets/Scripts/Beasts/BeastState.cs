using System;
using UnityEngine;

namespace BattleBeasts
{
    public enum BeastBattleState
    {
        Idle,
        Assessing,
        Moving,
        WindingUp,
        Executing,
        Recovering,
        Hesitating,
        Defeated
    }

    public enum TeamAlignment
    {
        Player,
        Enemy
    }

    [Serializable]
    public class BeastState
    {
        public BeastData beastData;
        public TeamAlignment teamAlignment;
        public BeastBattleState currentBattleState = BeastBattleState.Idle;
        public StrategyMode currentStrategy = StrategyMode.Neutral;
        public int currentHP;
        public float currentBond = 50f;

        public float basicCooldownRemaining;
        public float specialCooldownRemaining;
        public float hesitationRemaining;
        public float buffMultiplier = 1f;
        public float buffRemaining;

        public CommandIntent activeCommand;

        public bool IsDefeated => currentHP <= 0 || currentBattleState == BeastBattleState.Defeated;

        public void Initialize(BeastData data, TeamAlignment alignment, float startingBond)
        {
            beastData = data;
            teamAlignment = alignment;
            currentStrategy = data != null ? data.defaultStrategy : StrategyMode.Neutral;
            currentHP = data != null ? data.baseStats.maxHP : 1;
            currentBond = startingBond;
            currentBattleState = BeastBattleState.Idle;
            basicCooldownRemaining = 0f;
            specialCooldownRemaining = 0f;
            hesitationRemaining = 0f;
            buffMultiplier = 1f;
            buffRemaining = 0f;
            activeCommand = null;
        }

        public void Tick(float deltaTime)
        {
            basicCooldownRemaining = Mathf.Max(0f, basicCooldownRemaining - deltaTime);
            specialCooldownRemaining = Mathf.Max(0f, specialCooldownRemaining - deltaTime);
            hesitationRemaining = Mathf.Max(0f, hesitationRemaining - deltaTime);
            buffRemaining = Mathf.Max(0f, buffRemaining - deltaTime);

            if (buffRemaining <= 0f)
            {
                buffMultiplier = 1f;
            }
        }
    }
}
