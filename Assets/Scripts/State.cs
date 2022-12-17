using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    protected BeastController beast;

    public abstract void Tick();

    public virtual void OnStateEnter() { }
    public virtual void OnStateExit() { }

    public State(BeastController beast)
    {
        this.beast = beast;
    }
}
