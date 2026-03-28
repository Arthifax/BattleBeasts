using UnityEngine;

namespace BattleBeasts
{
    [CreateAssetMenu(fileName = "New Ability", menuName = "Battle Beasts/Grid Prototype/Ability")]
    public class AbilityData : ScriptableObject
    {
        [Header("Identity")]
        public string abilityName = "New Ability";
        public BeastType type = BeastType.Neutral;
        public DamageCategory damageCategory = DamageCategory.Physical;

        [Header("Grid Combat")]
        [Min(0f)] public float power = 20f;
        [Min(0f)] public float rangeInTiles = 6f;
        [Min(0f)] public float impactRadiusInTiles = 0.5f;
        [Min(0f)] public float windupTime = 0.75f;
        [Min(0f)] public float recoveryTime = 0.4f;
        [Min(0f)] public float cooldown = 2f;

        [Header("Visuals")]
        public Color telegraphColor = new Color(1f, 0.35f, 0.15f, 0.5f);
    }
}
