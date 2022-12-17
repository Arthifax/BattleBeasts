using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum Action
{
    Idle,
    Attack,
    Block,
    Dodge,
    Move
}

public enum Attack
{
    Bite,
    Scratch,
    Spit
}

public enum Block
{
    Head,
    Body,
    Legs
}

public enum Dodge
{
    Left,
    Right
}

public enum Move
{
    Circle,
    Random,
    Towards,
    Away,
    Target
}

public enum Strategy
{
    Aggressive,
    Defensive,
    Evasive
}

public class ActionContext
{
    public Action action;
    public Attack attack;
    public Strategy strategy;
    public Vector3 targetPosition;
    public float minRadius;
    public float maxRadius;
    public bool rotateClockwise;
}

public class BeastController : MonoBehaviour
{
    public bool isPlayer;
    public List<GameObject> enemyBeasts;
    public List<Action> availableActions;
    public Queue<Action> actionQueue;
    public Strategy strategy;
    public Action currentAction;
    public bool rotateClockwise;
    public float health;
    public float attack;
    public Color color;

    public float cooldown;
    public float windup;

    public float engageRange;
    public float attackRange;

    public bool stateTransitioned = false;

    NavMeshAgent navAgent;

    public bool isFlickerEnabled = false;

    IEnumerator colorFlickerRoutine()
    {
        while (isFlickerEnabled == true)
        {
            GetComponent<Renderer>().material.color = Color.white;
            yield return new WaitForSeconds(0.5f);
            GetComponent<Renderer>().material.color = color;
            yield return new WaitForSeconds(0.5f);
            isFlickerEnabled = false;
        }
    }

    public void enableFlicker()
    {
        isFlickerEnabled = true;
        StartCoroutine(colorFlickerRoutine());
    }

    Vector3 GetRandomTargetPosition(Vector3 center, float minRadius, float maxRadius)
    {
        Vector2 rndPos = Random.insideUnitCircle * (maxRadius - minRadius);
        rndPos += rndPos.normalized * minRadius;
        return new Vector3(center.x + rndPos.x, center.y, center.z + rndPos.y);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get the nav agent
        navAgent = GetComponent<NavMeshAgent>();
        color = GetComponent<Renderer>().material.color;
        windup = Random.Range(0.5f, 1.5f);

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

        // Set the action queue
        actionQueue = new Queue<Action>();
        actionQueue.Enqueue(Action.Attack);
        actionQueue.Enqueue(Action.Block);
        actionQueue.Enqueue(Action.Dodge);
        actionQueue.Enqueue(Action.Move);
        
        // Set the current action
        // currentAction = actionQueue.Dequeue();
    }

    // Update is called once per frame
    void Update()
    {
        // Move NavAgent to mouse click hit
        if (isPlayer && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                navAgent.SetDestination(hit.point);
            }
        }

        switch (currentAction)
        {
            case Action.Idle:
                break;
            case Action.Attack:
                // Attack the enemy
                if (enemyBeasts.Count > 0)
                {
                    Vector3 enemyPosition = enemyBeasts[0].transform.position;
                    transform.LookAt(enemyPosition);
                    if (Vector3.Distance(transform.position, enemyPosition) > attackRange)
                    {
                        // Move towards the enemy
                        currentAction = Action.Move;
                        windup = Random.Range(0.5f, 1.5f);
                        cooldown = 0;
                    }
                    else
                    {
                        if (cooldown > 0)
                        {
                            cooldown -= Time.deltaTime;
                            break;
                        }
                        else
                        {
                            if (windup > 0)
                            {
                                windup -= Time.deltaTime;
                                break;
                            }
                            else
                            {
                                enemyBeasts[0].GetComponent<BeastController>().hit(attack);
                                enemyBeasts[0].GetComponent<BeastController>().enableFlicker();
                                cooldown = 2; // in seconds
                                currentAction = Action.Move;
                                stateTransitioned = true;
                                navAgent.SetDestination(GetRandomTargetPosition(enemyPosition, engageRange, engageRange * 2));
                                windup = Random.Range(0.5f, 1.5f);
                            }
                        }
                    }
                    stateTransitioned = false;
                }
                break;
            case Action.Block:
                break;
            case Action.Dodge:
                break;
            case Action.Move:
                // Move NavAgent circle around the enemy
                if (enemyBeasts.Count > 0)
                {
                    Vector3 enemyPosition = enemyBeasts[0].transform.position;
                    transform.LookAt(enemyPosition);

                    if (Vector3.Distance(transform.position, enemyPosition) < attackRange) {
                        currentAction = Action.Attack;
                        stateTransitioned = true;
                    } else if (Vector3.Distance(transform.position, enemyPosition) > engageRange) {
                        // Move towards the enemy
                        Vector3 direction = enemyPosition - transform.position;
                        direction.Normalize();
                        Vector3 newPosition = enemyPosition + direction * 5;
                        // if (navAgent.remainingDistance < 1) {
                            navAgent.SetDestination(newPosition);
                        // }
                    } else {
                    // Rotate to face the enemy
                        // transform.RotateAround(enemyPosition, Vector3.up, (rotateClockwise ? 20 : -20) * Time.deltaTime);
                        if ((navAgent.remainingDistance < engageRange && stateTransitioned == true) || (navAgent.remainingDistance < 1)) {
                            navAgent.SetDestination(GetRandomTargetPosition(enemyPosition, attackRange, engageRange));
                        }
                    }

                }
                stateTransitioned = false;
                break;
            default:
                break;
        }
    }

    void hit(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
