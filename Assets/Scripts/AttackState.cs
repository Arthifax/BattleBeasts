using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{
    public AttackState(BeastController beast) : base(beast)
    {}
    public override void Tick() {
        // Attack the enemy
        if (beast.enemyBeasts.Count <= 0) return;
        
        Vector3 enemyPosition = beast.enemyBeasts[0].transform.position;
        beast.transform.LookAt(enemyPosition);
        
        if (!beast.IsWithinAttackRange())
        {
            if (beast.IsWithinEngageRange())
                beast.SetState(new EngageState(beast));
            else 
                beast.SetState(new MoveState(beast));
        }

        if (beast.IsWithinAttackRange())
        {
            if (beast.cooldown > 0)
            {
                beast.cooldown -= Time.deltaTime;
                return;
            }

            if (beast.cooldown <= 0)
            {
                if (beast.windup > 0)
                {
                    beast.windup -= Time.deltaTime;
                    return;
                }

                if (beast.windup <= 0)
                {
                    beast.enemyBeasts[0].GetComponent<BeastController>().Hit(beast.attack);
                    beast.enemyBeasts[0].GetComponent<BeastController>().EnableFlicker();
                    beast.cooldown = 2; // in seconds
                    beast.SetState(new MoveState(beast));
                    beast.navAgent.SetDestination(beast.GetRandomTargetPosition(enemyPosition, beast.engageRange, beast.engageRange * 2));
                }
            }
        }
    }

    public override void OnStateEnter() { }
    public override void OnStateExit() { 
        beast.windup = Random.Range(0.5f, 1.5f);
        beast.cooldown = 0;
    }
}
