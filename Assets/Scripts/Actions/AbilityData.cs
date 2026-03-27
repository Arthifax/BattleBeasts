using UnityEngine;

namespace BattleBeasts
{
    public enum DamageCategory
    {
        Physical,
        Special,
        Status
    }

    public enum AbilityExecutionType
    {
        PhysicalDash,
        ProjectileAOE,
        SelfBuff
    }

    [CreateAssetMenu(fileName = "New Ability", menuName = "Battle Beasts/Ability")]
    public class AbilityData : ScriptableObject
    {
        [Header("Identity")]
        public string abilityName = "New Ability";
        public BeastType type = BeastType.Neutral;
        public DamageCategory damageCategory = DamageCategory.Physical;
        public AbilityExecutionType executionType = AbilityExecutionType.ProjectileAOE;

        [Header("Combat")]
        [Min(0f)] public float power = 20f;
        [Min(0f)] public float range = 4f;
        [Min(0f)] public float blastRadius = 1.5f;
        [Min(0f)] public float windupTime = 0.75f;
        [Min(0f)] public float recoveryTime = 0.4f;
        [Min(0f)] public float cooldown = 2f;

        [Header("Projectile")]
        public GameObject projectilePrefab;
        [Min(0f)] public float projectileSpeed = 8f;

        [Header("Movement")]
        [Min(0f)] public float dashDistance = 1.75f;
        [Min(0f)] public float dashSpeed = 10f;

        [Header("Buff")]
        [Min(0f)] public float selfBuffMultiplier = 1.2f;
        [Min(0f)] public float selfBuffDuration = 3f;
    }
}
