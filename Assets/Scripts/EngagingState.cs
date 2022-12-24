using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngagingState : State
{
    private float engageTimer = 0;
    private bool isAttacking = false;

    public EngagingState(BeastController beast) : base(beast)
    {
    }
    public override void Tick() {
        Vector3 enemyPosition = beast.targetBeast.transform.position;
        if (engageTimer > 0) {
            beast.transform.RotateAround(enemyPosition, Vector3.forward, (beast.rotateClockwise ? 20 : -20) * Time.deltaTime);
            engageTimer -= Time.deltaTime;
            return;
        }

        if (isAttacking) {
            beast.navAgent.SetDestination(enemyPosition);
            beast.SetState(new AttackingState(beast));
        } else {
            // beast.navAgent.SetDestination();
            // Move away from the enemy
            Vector3 direction = beast.transform.position - enemyPosition;
            direction.Normalize();
            Vector3 newPosition = beast.transform.position + direction * 5;
            beast.navAgent.SetDestination(newPosition);
            beast.SetState(new MovingState(beast));
        }
        
        // if ((beast.navAgent.remainingDistance < beast.engageRange && beast.stateTransitioned == true) || (beast.navAgent.remainingDistance < 1)) {
        //     beast.navAgent.SetDestination(beast.GetRandomTargetPosition(enemyPosition, beast.attackRange, beast.engageRange));
        // }
    }

    public override void OnStateEnter() {
        engageTimer = Random.Range(1, 3);
        isAttacking = Random.Range(0, 2) == 0;
     }
    public override void OnStateExit() {
        engageTimer = 0;
     }
}
