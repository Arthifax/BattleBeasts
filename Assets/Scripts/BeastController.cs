using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Action
{
    Idle,
    Attack,
    Block,
    Dodge,
    Move,
    RunAway
}

public enum Strategy
{
    Aggressive,
    Defensive,
    Evasive
}

public class BeastController : MonoBehaviour
{
    public List<GameObject> enemyBeasts;
    public List<Action> availableActions;
    public Queue<Action> actionQueue;
    public Strategy strategy;
    public Action currentAction;

    // Start is called before the first frame update
    void Start()
    {
        // Get all the enemy beasts with tag beast except the one that is attached to this script
        GameObject[] beasts = GameObject.FindGameObjectsWithTag("Beast");
        foreach (GameObject beast in beasts)
        {
            if (beast != this.gameObject)
            {
                enemyBeasts.Add(beast);
            }
        }

        // Set the strategy
        strategy = Strategy.Aggressive;

        // Set the available actions
        availableActions.Add(Action.Attack);
        availableActions.Add(Action.Block);
        availableActions.Add(Action.Dodge);
        availableActions.Add(Action.Move);
        availableActions.Add(Action.RunAway);

        // Set the action queue
        actionQueue = new Queue<Action>();
        actionQueue.Enqueue(Action.Attack);
        actionQueue.Enqueue(Action.Block);
        actionQueue.Enqueue(Action.Dodge);
        actionQueue.Enqueue(Action.Move);
        actionQueue.Enqueue(Action.RunAway);
        
        // Set the current action
        currentAction = actionQueue.Dequeue();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentAction)
        {
            case Action.Idle:
                break;
            case Action.Attack:
                break;
            case Action.Block:
                break;
            case Action.Dodge:
                break;
            case Action.Move:
                break;
            case Action.RunAway:
                break;
            default:
                break;
        }
    }
}
