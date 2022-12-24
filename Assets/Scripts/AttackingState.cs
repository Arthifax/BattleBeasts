using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Ability { 
    Swipe,
    Flame
}

public class AttackingState : State
{
    Ability ability;

    public AttackingState(BeastController beast) : base(beast)
    {}
    public override void Tick() {
        // Attack the enemy
        if (!(beast.enemyBeasts.Count > 0 && beast.targetBeast)) return;
        
        Vector3 enemyPosition = beast.targetBeast.transform.position;
        beast.transform.LookAt(enemyPosition);
        
        if (!beast.IsWithinAttackRange())
        {
            beast.ExecuteNextFromQueue();
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
                    Debug.Log(ability.ToString());
                    beast.animator.SetBool($"ability{ability.ToString()}", true);
                    beast.targetBeast?.Hit(beast.attack);
                    beast.targetBeast?.EnableFlicker();
                    beast.cooldown = 2; // in seconds
                    // beast.SetState(new MovingState(beast));
                    // beast.navAgent.SetDestination(beast.GetRandomTargetPosition(enemyPosition, beast.engageRange, beast.engageRange * 2));
                    beast.ExecuteNextFromQueue();
                }
            }
        }
    }

    public override void OnStateEnter() {
        ability = (Ability)Random.Range(0, 2);
        beast.animator.SetBool("isAttacking", true);
        beast.ClearAndAddToQueue(beast.preparedStates["Dodging"]);
        beast.AddToQueue(((MovingState)beast.preparedStates["Moving"]).SetTargetPosition(beast.GetRandomTargetPosition(beast.targetBeast.transform.position, beast.engageRange, beast.engageRange * 2)));
    }
    public override void OnStateExit() { 
        beast.windup = Random.Range(0.5f, 1.5f);
        beast.cooldown = 0;
        beast.animator.SetBool("isAttacking", false);
        beast.animator.SetBool($"ability{ability.ToString()}", false);
    }
}
