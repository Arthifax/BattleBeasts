using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgingState : State
{
    const float dodgeDistance = 5f;
    Vector3 dodgeDirection = new Vector3(0, 0, 0);
    private float timer = 0f;

    public DodgingState(BeastController beast) : base(beast)
    {
    }

    public override void Tick() {
        if (timer >= 2f) {
            beast.ExecuteNextFromQueue();
            return;
        }
        if (Vector3.Distance(beast.transform.position, dodgeDirection) > 1f) {
            beast.transform.position += dodgeDirection * dodgeDistance * Time.deltaTime;
        } else {
            beast.ExecuteNextFromQueue();
        }
        timer += Time.deltaTime;

    }

    public override void OnStateEnter() {
        if (beast.targetBeast == null) {
            beast.ExecuteNextFromQueue();
            return;
        }
        dodgeDirection = beast.transform.position - beast.targetBeast.transform.position;
        dodgeDirection = dodgeDirection.normalized;
        timer = 0f;
     }
    public override void OnStateExit() { }
}
