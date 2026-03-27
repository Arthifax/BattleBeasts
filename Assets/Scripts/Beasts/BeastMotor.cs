using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace BattleBeasts
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class BeastMotor : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float stopDistance = 1.5f;
        [SerializeField] private float turnSpeed = 12f;

        private BeastController controller;
        private NavMeshAgent agent;

        public void Initialize(BeastController beastController)
        {
            controller = beastController;
            agent = GetComponent<NavMeshAgent>();
            ApplyStatsToAgent();
        }

        private void Update()
        {
            if (controller == null || controller.Opponent == null)
            {
                return;
            }

            Vector3 flatDirection = controller.Opponent.transform.position - transform.position;
            flatDirection.y = 0f;
            if (flatDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(flatDirection.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        public IEnumerator MoveToRoutine(Vector3 destination)
        {
            if (agent == null)
            {
                yield break;
            }

            controller.State.currentBattleState = BeastBattleState.Moving;
            ApplyStatsToAgent();
            agent.isStopped = false;
            agent.stoppingDistance = stopDistance;
            agent.SetDestination(destination);

            while (!controller.State.IsDefeated)
            {
                if (agent.pathPending)
                {
                    yield return null;
                    continue;
                }

                if (agent.remainingDistance <= agent.stoppingDistance + 0.05f)
                {
                    break;
                }

                yield return null;
            }

            StopMoving();
            yield return null;
        }

        public IEnumerator DashToPointRoutine(Vector3 destination, float dashSpeed, float stopThreshold = 0.2f)
        {
            controller.State.currentBattleState = BeastBattleState.Executing;
            agent.isStopped = true;
            agent.enabled = false;

            while (!controller.State.IsDefeated)
            {
                transform.position = Vector3.MoveTowards(transform.position, destination, dashSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, destination) <= stopThreshold)
                {
                    break;
                }
                yield return null;
            }

            agent.enabled = true;
            ApplyStatsToAgent();
            yield return null;
        }

        public void StopMoving()
        {
            if (agent == null || !agent.enabled)
            {
                return;
            }

            agent.isStopped = true;
            agent.ResetPath();
        }

        private void ApplyStatsToAgent()
        {
            if (agent == null || controller?.BeastData == null)
            {
                return;
            }

            float agility = controller.BeastData.baseStats.agility;
            agent.speed = 2.5f + agility * 0.15f;
            agent.acceleration = 8f + agility * 0.25f;
            agent.angularSpeed = 360f + agility * 10f;
        }
    }
}
