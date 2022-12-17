using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : State
{
    public MoveState(BeastController beast) : base(beast)
    {
    }
    public override void Tick() {
        // Move NavAgent circle around the enemy
        if (beast.enemyBeasts.Count > 0)
        {
            Vector3 enemyPosition = beast.enemyBeasts[0].transform.position;
            beast.transform.LookAt(enemyPosition);

            if (beast.IsWithinAttackRange()) {
                beast.SetState(new AttackState(beast));
            }
            
            if (!beast.IsWithinEngageRange()) {
                Debug.Log("Moving towards enemy");
                beast.navAgent.SetDestination(Vector3.MoveTowards(beast.transform.position, enemyPosition, 5));
            } 
            
            if (beast.IsWithinEngageRange()) {
            // Rotate to face the enemy
                beast.SetState(new EngageState(beast));
            }

        }
    }

    public override void OnStateEnter() { }
    public override void OnStateExit() { }
}
