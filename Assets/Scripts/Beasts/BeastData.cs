using System;
using UnityEngine;

namespace BattleBeasts
{
    public enum BeastType
    {
        Fire,
        Water,
        Grass,
        Neutral
    }

    public enum StrategyMode
    {
        Neutral,
        Aggressive,
        Defensive
    }

    [Serializable]
    public class BeastStats
    {
        [Min(1)] public int maxHP = 100;
        [Min(1)] public int attack = 10;
        [Min(1)] public int defense = 10;
        [Min(1)] public int specialAttack = 10;
        [Min(1)] public int specialDefense = 10;
        [Min(1)] public int reflexes = 10;
        [Min(1)] public int agility = 10;
    }

    [CreateAssetMenu(fileName = "New Beast", menuName = "Battle Beasts/Beast")]
    public class BeastData : ScriptableObject
    {
        [Header("Identity")]
        public string speciesName = "New Beast";
        public BeastType beastType = BeastType.Neutral;

        [Header("Base Stats")]
        public BeastStats baseStats = new BeastStats();

        [Header("Abilities")]
        public AbilityData basicAttack;
        public AbilityData specialAbility;

        [Header("Behaviour")]
        public StrategyMode defaultStrategy = StrategyMode.Neutral;
        [Range(0f, 1f)] public float aggressionBias = 0.5f;
        [Range(0f, 1f)] public float cautionBias = 0.5f;

        [Header("Visuals")]
        public GameObject beastPrefab;
        public Sprite portrait;
    }
}
