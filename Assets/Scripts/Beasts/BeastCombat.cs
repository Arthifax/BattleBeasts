using System.Collections;
using UnityEngine;

namespace BattleBeasts
{
    public class BeastCombat : MonoBehaviour
    {
        [Header("Fallback Combat Values")]
        [SerializeField] private float defaultBasicRange = 2f;
        [SerializeField] private float defaultSpecialRange = 6f;
        [SerializeField] private LayerMask damageableLayers = ~0;

        private BeastController controller;

        public void Initialize(BeastController beastController)
        {
            controller = beastController;
        }

        public float GetBasicAttackRange()
        {
            AbilityData basic = controller.BeastData != null ? controller.BeastData.basicAttack : null;
            return basic != null ? basic.range : defaultBasicRange;
        }

        public float GetSpecialAttackRange()
        {
            AbilityData special = controller.BeastData != null ? controller.BeastData.specialAbility : null;
            return special != null ? special.range : defaultSpecialRange;
        }

        public bool CanUseBasicAttack()
        {
            return controller.State.basicCooldownRemaining <= 0f;
        }

        public bool CanUseSpecialAttack()
        {
            return controller.State.specialCooldownRemaining <= 0f;
        }

        public IEnumerator BasicAttackRoutine()
        {
            AbilityData basic = controller.BeastData != null ? controller.BeastData.basicAttack : null;
            if (basic == null || controller.Opponent == null)
            {
                yield break;
            }

            controller.State.currentBattleState = BeastBattleState.WindingUp;
            BattleManager.Instance?.Log($"{controller.DisplayName} prepares {basic.abilityName}!");

            float windup = GetAdjustedWindup(basic.windupTime);
            yield return new WaitForSeconds(windup);

            if (controller.State.IsDefeated || controller.Opponent.State.IsDefeated)
            {
                yield break;
            }

            Vector3 lockedTargetPoint = controller.Opponent.transform.position;
            yield return controller.StartCoroutine(controller.Motor.DashToPointRoutine(lockedTargetPoint, basic.dashSpeed));

            if (Vector3.Distance(controller.transform.position, controller.Opponent.transform.position) <= basic.blastRadius + 0.75f)
            {
                float damage = CalculateDamage(basic, controller.Opponent);
                controller.Opponent.ApplyDamage(damage);
                BattleManager.Instance?.Log($"{controller.DisplayName} hits with {basic.abilityName}!");
            }
            else
            {
                BattleManager.Instance?.Log($"{controller.DisplayName}'s {basic.abilityName} misses!");
            }

            controller.State.basicCooldownRemaining = basic.cooldown;
            controller.State.currentBattleState = BeastBattleState.Recovering;
            yield return new WaitForSeconds(GetAdjustedRecovery(basic.recoveryTime));
        }

        public IEnumerator SpecialAttackRoutine()
        {
            AbilityData special = controller.BeastData != null ? controller.BeastData.specialAbility : null;
            if (special == null)
            {
                yield break;
            }

            controller.State.currentBattleState = BeastBattleState.WindingUp;
            Vector3 targetPosition = controller.Opponent != null ? controller.Opponent.transform.position : controller.transform.position;
            BattleManager.Instance?.Log($"{controller.DisplayName} starts charging {special.abilityName}!");

            yield return new WaitForSeconds(GetAdjustedWindup(special.windupTime));

            if (controller.State.IsDefeated)
            {
                yield break;
            }

            if (special.executionType == AbilityExecutionType.ProjectileAOE)
            {
                SpawnProjectile(special, targetPosition);
            }
            else if (special.executionType == AbilityExecutionType.SelfBuff)
            {
                controller.ApplySelfBuff(special.selfBuffMultiplier, special.selfBuffDuration);
            }

            controller.State.specialCooldownRemaining = special.cooldown;
            controller.State.currentBattleState = BeastBattleState.Recovering;
            yield return new WaitForSeconds(GetAdjustedRecovery(special.recoveryTime));
        }

        public IEnumerator EvadeRoutine()
        {
            if (controller.Opponent == null)
            {
                yield break;
            }

            Vector3 away = (controller.transform.position - controller.Opponent.transform.position).normalized;
            if (away == Vector3.zero)
            {
                away = -controller.transform.forward;
            }

            float evadeDistance = 2f + controller.BeastData.baseStats.agility * 0.05f;
            Vector3 evadeTarget = controller.transform.position + away * evadeDistance;
            BattleManager.Instance?.Log($"{controller.DisplayName} tries to evade!");
            yield return controller.StartCoroutine(controller.Motor.DashToPointRoutine(evadeTarget, 10f + controller.BeastData.baseStats.agility * 0.2f));
        }

        private void SpawnProjectile(AbilityData ability, Vector3 targetPosition)
        {
            if (ability.projectilePrefab == null)
            {
                BattleManager.Instance?.Log($"{controller.DisplayName} tried to use {ability.abilityName}, but no projectile prefab is assigned.");
                return;
            }

            GameObject projectileObject = Instantiate(ability.projectilePrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            BattleProjectile projectile = projectileObject.GetComponent<BattleProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<BattleProjectile>();
            }

            projectile.Initialize(controller, ability, targetPosition, damageableLayers);
            BattleManager.Instance?.Log($"{controller.DisplayName} launches {ability.abilityName}!");
        }

        private float CalculateDamage(AbilityData ability, BeastController target)
        {
            if (controller.BeastData == null || target.BeastData == null)
            {
                return ability.power;
            }

            BeastStats attackerStats = controller.BeastData.baseStats;
            BeastStats defenderStats = target.BeastData.baseStats;

            float offensiveStat;
            float defensiveStat;

            if (ability.damageCategory == DamageCategory.Physical)
            {
                offensiveStat = attackerStats.attack * controller.State.buffMultiplier;
                defensiveStat = defenderStats.defense;
            }
            else
            {
                offensiveStat = attackerStats.specialAttack * controller.State.buffMultiplier;
                defensiveStat = defenderStats.specialDefense;
            }

            float typeModifier = GetTypeModifier(ability.type, target.BeastData.beastType);
            float damage = ability.power * (offensiveStat / Mathf.Max(1f, defensiveStat)) * typeModifier;
            return Mathf.Max(1f, damage);
        }

        private float GetTypeModifier(BeastType attackType, BeastType defenderType)
        {
            if (attackType == BeastType.Water && defenderType == BeastType.Fire) return 1.25f;
            if (attackType == BeastType.Fire && defenderType == BeastType.Grass) return 1.25f;
            if (attackType == BeastType.Grass && defenderType == BeastType.Water) return 1.25f;

            if (attackType == BeastType.Fire && defenderType == BeastType.Water) return 0.8f;
            if (attackType == BeastType.Grass && defenderType == BeastType.Fire) return 0.8f;
            if (attackType == BeastType.Water && defenderType == BeastType.Grass) return 0.8f;

            return 1f;
        }

        private float GetAdjustedWindup(float baseWindup)
        {
            float reflexes = controller.BeastData.baseStats.reflexes;
            return Mathf.Max(0.1f, baseWindup - reflexes * 0.015f);
        }

        private float GetAdjustedRecovery(float baseRecovery)
        {
            float reflexes = controller.BeastData.baseStats.reflexes;
            return Mathf.Max(0.05f, baseRecovery - reflexes * 0.01f);
        }
    }

    public class BattleProjectile : MonoBehaviour
    {
        private BeastController owner;
        private AbilityData ability;
        private Vector3 targetPosition;
        private LayerMask damageableLayers;
        private bool initialized;

        public void Initialize(BeastController projectileOwner, AbilityData projectileAbility, Vector3 destination, LayerMask layers)
        {
            owner = projectileOwner;
            ability = projectileAbility;
            targetPosition = destination;
            damageableLayers = layers;
            initialized = true;
        }

        private void Update()
        {
            if (!initialized || ability == null)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, ability.projectileSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) <= 0.05f)
            {
                Explode();
            }
        }

        private void Explode()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, ability.blastRadius, damageableLayers);
            foreach (Collider hit in hits)
            {
                BeastController target = hit.GetComponent<BeastController>();
                if (target == null || target == owner || owner == null)
                {
                    continue;
                }

                BeastCombat combat = owner.Combat;
                if (combat == null)
                {
                    continue;
                }

                float damage = ability.power;
                if (owner.BeastData != null && target.BeastData != null)
                {
                    BeastStats attackerStats = owner.BeastData.baseStats;
                    BeastStats defenderStats = target.BeastData.baseStats;
                    float offensiveStat = attackerStats.specialAttack * owner.State.buffMultiplier;
                    float defensiveStat = Mathf.Max(1f, defenderStats.specialDefense);
                    damage = ability.power * (offensiveStat / defensiveStat);
                }

                target.ApplyDamage(damage);
            }

            BattleManager.Instance?.Log($"{owner.DisplayName}'s {ability.abilityName} explodes!");
            Destroy(gameObject);
        }
    }
}
