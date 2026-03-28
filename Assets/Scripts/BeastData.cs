using UnityEngine;

namespace BattleBeasts
{
    [CreateAssetMenu(fileName = "New Beast", menuName = "Battle Beasts/Grid Prototype/Beast")]
    public class BeastData : ScriptableObject
    {
        [Header("Identity")]
        public string speciesName = "New Beast";
        public BeastType beastType = BeastType.Neutral;

        [Header("Base Stats")]
        public BeastStats baseStats = new BeastStats
        {
            maxHP = 100,
            attack = 10,
            defense = 10,
            specialAttack = 10,
            specialDefense = 10,
            reflexes = 10,
            agility = 10
        };

        [Header("Abilities")]
        public AbilityData basicAttack;
        public AbilityData specialAbility;

        [Header("Movement")]
        [Min(0.1f)] public float moveSpeedTilesPerSecond = 7f;
        [Min(1)] public int defaultApproachRangeInTiles = 3;
        [Min(1)] public int dodgeDistanceInTiles = 4;

        [Header("Behaviour")]
        [Range(0f, 1f)] public float autoDodgeChance = 0.6f;
        [Range(0f, 1f)] public float aggressionBias = 0.6f;
        [Range(0f, 1f)] public float cautionBias = 0.4f;
    }
}
