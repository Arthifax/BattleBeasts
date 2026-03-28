namespace BattleBeasts
{
    public struct BattleCommand
    {
        public CommandType commandType;
        public GridCoord targetTile;
        public AbilityData ability;

        public static BattleCommand Move(GridCoord tile)
        {
            return new BattleCommand
            {
                commandType = CommandType.MoveToTile,
                targetTile = tile,
                ability = null
            };
        }

        public static BattleCommand Basic(AbilityData ability)
        {
            return new BattleCommand
            {
                commandType = CommandType.UseBasicAttack,
                targetTile = default,
                ability = ability
            };
        }

        public static BattleCommand Special(AbilityData ability)
        {
            return new BattleCommand
            {
                commandType = CommandType.UseSpecialAttack,
                targetTile = default,
                ability = ability
            };
        }

        public static BattleCommand DodgeAway()
        {
            return new BattleCommand
            {
                commandType = CommandType.DodgeAway,
                targetTile = default,
                ability = null
            };
        }
    }
}
