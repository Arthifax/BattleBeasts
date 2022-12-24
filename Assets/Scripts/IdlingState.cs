using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdlingState : State
{
    private float timer = 0f;

    public IdlingState(BeastController beast) : base(beast)
    {
    }
    public override void Tick() {
        timer -= Time.deltaTime;
        if (timer <= 0f) {
            beast.SetState(beast.DecideNextState());
        }
    }

    public override void OnStateEnter() {
        timer = Random.Range(0f, 3f);
    }
    public override void OnStateExit() {
        timer = 0f;
    }
}
