using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockingState : State
{
    private float timer = 0f;

    public BlockingState(BeastController beast) : base(beast)
    {
    }
    public override void Tick() {
        timer += Time.deltaTime;
        if (timer >= beast.blockingTime) {
            beast.ExecuteNextFromQueue();
        }
    }

    public override void OnStateEnter() {
        timer = 2f;
        // beast.animator.SetBool("Blocking", true);
    }
    public override void OnStateExit() {
        timer = 0f;
        // beast.animator.SetBool("Blocking", false);
    }
}
