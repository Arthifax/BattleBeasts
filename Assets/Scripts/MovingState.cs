using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovingState : State
{
    Vector3? targetPosition;

    public MovingState(BeastController beast, [Optional] Vector3 target) : base(beast)
    {
        // Make sure target is on the navmesh or get nearest point on navmesh
        if (target != null) {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(target, out hit, 1.0f, NavMesh.AllAreas)) {
                target = hit.position;
            } else {
                Debug.Log("Target position is not on the navmesh");
            }
        }

        this.targetPosition = target;
    }
    public override void Tick() {
        // Move NavAgent circle around the enemy
        if (beast.targetBeast && targetPosition == null)
        {
            Vector3 enemyPosition = beast.targetBeast.transform.position;
            beast.transform.LookAt(enemyPosition);

            if (beast.IsWithinAttackRange()) {
                beast.AddToQueue(beast.preparedStates["Attacking"]);
                beast.ExecuteNextFromQueue();
            }
            
            if (!beast.IsWithinEngageRange()) {
                Debug.Log("Moving towards enemy");
                beast.navAgent.SetDestination(Vector3.MoveTowards(beast.transform.position, enemyPosition, beast.speed));
            } 
            
            if (beast.IsWithinEngageRange()) {
            // Rotate to face the enemy
                beast.AddToQueue(beast.preparedStates["Engaging"]);
                beast.ExecuteNextFromQueue();
            }

        }
        else if (targetPosition != null)
        {
            beast.transform.LookAt((Vector3)targetPosition);
            beast.navAgent.SetDestination((Vector3)targetPosition);
            if (Vector3.Distance(beast.transform.position, (Vector3)targetPosition) < 1)
            {
                beast.ExecuteNextFromQueue();
            }
        } else {
                beast.ExecuteNextFromQueue();
        }
    }

    public MovingState SetTargetPosition(Vector3 target)
    {
        // Make sure target is on the navmesh or get nearest point on navmesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 1.0f, NavMesh.AllAreas)) {
            target = hit.position;
        } else {
            Debug.Log("Target position is not on the navmesh");
        }
        this.targetPosition = target;
        return this;
    }

    public override void OnStateEnter() { }
    public override void OnStateExit() {
        this.targetPosition = null;
     }
}
