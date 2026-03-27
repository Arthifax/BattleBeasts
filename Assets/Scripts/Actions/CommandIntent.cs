using UnityEngine;

namespace BattleBeasts
{
    public enum CommandType
    {
        None,
        Move,
        BasicAttack,
        SpecialAttack,
        Evade,
        SetStrategy
    }

    [System.Serializable]
    public class CommandIntent
    {
        public CommandType commandType = CommandType.None;
        public Vector3 targetPosition;
        public StrategyMode requestedStrategy = StrategyMode.Neutral;
        public float timeIssued;
        public float duration = 1.5f;

        public bool IsExpired(float currentTime)
        {
            return commandType != CommandType.None && currentTime > timeIssued + duration;
        }

        public static CommandIntent CreateMove(Vector3 position, float currentTime, float duration = 2f)
        {
            return new CommandIntent
            {
                commandType = CommandType.Move,
                targetPosition = position,
                timeIssued = currentTime,
                duration = duration
            };
        }

        public static CommandIntent CreateAction(CommandType type, float currentTime, float duration = 1.5f)
        {
            return new CommandIntent
            {
                commandType = type,
                timeIssued = currentTime,
                duration = duration
            };
        }

        public static CommandIntent CreateStrategy(StrategyMode mode, float currentTime, float duration = 8f)
        {
            return new CommandIntent
            {
                commandType = CommandType.SetStrategy,
                requestedStrategy = mode,
                timeIssued = currentTime,
                duration = duration
            };
        }
    }
}
