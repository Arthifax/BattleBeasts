using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    protected BeastController beast;
    public bool isInteruptable;

    public abstract void Tick();

    public virtual void OnStateEnter() { }
    public virtual void OnStateExit() { }

    public State(BeastController beast, bool isInteruptable = true)
    {
        this.beast = beast;
        this.isInteruptable = isInteruptable;
    }

    public string GetName()
    {
        return this.GetType().Name.Split("State")[0];
    }
}
