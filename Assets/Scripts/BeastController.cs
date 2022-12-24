using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public enum Strategy
{
    Aggressive,
    Defensive,
    Evasive
}

public class BeastController : MonoBehaviour
{
    private const string COMMAND_TEXT = "Commands\n>";
    private const string HP_TEXT = "HP: ";

    public bool isPlayer;
    private string beastName;
    public List<GameObject> enemyBeasts;
    public BeastController targetBeast;
    public Strategy strategy;
    public TMP_Text healthText;
    public TMP_Text queueText;

    public Queue<State> stateQueue;
    public IDictionary<string, State> preparedStates;
    public State currentState;

    public bool rotateClockwise;
    public float health;
    public float attack;
    public float speed = 2f;
    public Color color;

    public float cooldown;
    public float windup;

    public float engageRange;
    public float attackRange;

    public NavMeshAgent navAgent;
    public Animator animator;

    public bool isFlickerEnabled = false;
    public float blockingTime = 2f;

    public State GetNextStateOrIdle()
    {
        if (stateQueue.Count <= 0)
        {
            return preparedStates["Idling"];
        }

        return stateQueue.Dequeue();
    }

    public void ExecuteImmediately(State state)
    {
        if (!currentState.isInteruptable) {
            ClearAndAddToQueue(state);
            return;
        }

        SetState(state);
    }

    public void ExecuteNextFromQueue()
    {
        SetState(GetNextStateOrIdle());
    }

    public State DecideNextState()
    {
        if (!(targetBeast != null && enemyBeasts.Count > 0))
        {
            return preparedStates["Idling"];
        }

        if (targetBeast.currentState is AttackingState && targetBeast.IsWithinAttackRange())
        {
            // coinflip
            State[] states = { preparedStates["Dodging"], preparedStates["Blocking"] };
            return RandomUtil.EitherOr(states);
        }

        if (IsWithinAttackRange())
        {
            return preparedStates["Attacking"];
        }

        if (IsWithinEngageRange())
        {
            return preparedStates["Engaging"];
        }

        return preparedStates["Moving"];
    }

    public void ClearAndAddToQueue(State state)
    {
        ClearQueue();
        AddToQueue(state);
    }

    public void ClearQueue()
    {
        stateQueue.Clear();
    }

    public void AddToQueue(State state)
    {
        stateQueue.Enqueue(state);
    }

    public bool IsWithinEngageRange()
    {
        return Vector3.Distance(transform.position, targetBeast.transform.position) < engageRange;
    }

    public bool IsWithinAttackRange()
    {
        return Vector3.Distance(transform.position, targetBeast.transform.position) < attackRange;
    }

    public bool IsEnemyAttacking()
    {
        return targetBeast.currentState is AttackingState;
    }

    public void TargetClosestBeast()
    {
        float closestDistance = Mathf.Infinity;
        BeastController closestBeast = null;
        foreach (GameObject enemyBeast in enemyBeasts)
        {
            float distance = Vector3.Distance(transform.position, enemyBeast.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBeast = GetEnemyBeastController(enemyBeast);
            }
        }
        targetBeast = closestBeast;
    }

    public BeastController GetEnemyBeastController(GameObject beast)
    {
        return beast.GetComponent<BeastController>();
    }

    public IEnumerator ColorFlickerRoutine()
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

    public void EnableFlicker()
    {
        isFlickerEnabled = true;
        StartCoroutine(ColorFlickerRoutine());
    }

    public Vector3 GetRandomTargetPosition(Vector3 center, float minRadius, float maxRadius)
    {

        Vector2 rndPos = Random.insideUnitCircle * (maxRadius - minRadius);
        rndPos += rndPos.normalized * minRadius;
        // Make sure the position is on the navmesh
        NavMeshHit hit;
        NavMesh.SamplePosition(new Vector3(center.x + rndPos.x, center.y, center.z + rndPos.y), out hit, maxRadius, NavMesh.AllAreas);
        return hit.position;
    }

    private void UpdateQueueText()
    {
        if (!isPlayer) return;

        string text = COMMAND_TEXT + currentState.GetName();

        foreach (State state in stateQueue)
        {
            text += $"\n{state.GetName()}";
        }
        queueText.text = text;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get the nav agent
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        color = GetComponent<Renderer>().material.color;
        windup = Random.Range(0.5f, 1.5f);
        beastName = gameObject.name;
        queueText = (isPlayer) ? GameObject.FindGameObjectsWithTag("QueueText")[0].GetComponent<TMP_Text>() : null;
        healthText.text = $"{beastName}\n{HP_TEXT}{health.ToString()}";
        stateQueue = new Queue<State>();

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
        TargetClosestBeast();

        preparedStates = new Dictionary<string, State>();
        preparedStates.Add("Idling", new IdlingState(this) as State);
        preparedStates.Add("Moving", new MovingState(this) as State);
        preparedStates.Add("Engaging", new EngagingState(this) as State);
        preparedStates.Add("Attacking", new AttackingState(this) as State);
        preparedStates.Add("Dodging", new DodgingState(this) as State);
        preparedStates.Add("Blocking", new BlockingState(this) as State);
        preparedStates.Add("Roaming", new RoamingState(this) as State);
        preparedStates.Add("Fleeing", new FleeingState(this) as State);

        AddToQueue(preparedStates["Idling"]);
        ExecuteNextFromQueue();
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Triggered");
        if (other.gameObject.tag == "Attack") {
            Hit(other.gameObject.GetComponent<BeastController>().attack);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateQueueText();

        // Move NavAgent to mouse click hit
        if (isPlayer && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                ExecuteImmediately((State)((MovingState)preparedStates["Moving"]).SetTargetPosition(hit.point));
            }
        }

        // Did child object Swipe box collider trigger?
        if (currentState is AttackingState && GetComponentInChildren<BoxCollider>().isTrigger)
        {
            // If so, deal damage to enemy
            targetBeast.Hit(attack);
            // GetComponentInChildren<BoxCollider>().isTriggered = false;
        }
        if (currentState is not null) {
            currentState.Tick();
        }
    }

    public void SetState(State state)
    {
        if (currentState != null)
            currentState.OnStateExit();

        currentState = state;
        gameObject.name = $"{beastName} - {state.GetName()}";

        

        if (currentState != null)
            currentState.OnStateEnter();
    }

    public void Hit(float damage)
    {
        if (currentState is BlockingState) {
            health -= damage / 2;
            GetNextStateOrIdle();
        } else if (currentState is DodgingState) {
            health -= damage * 2;
        } else {
            health -= damage;
        }
        
        // Get text element with name "BeastHP" and set its text to health
        healthText.text = beastName + "\n" + HP_TEXT + health.ToString();
        if (health <= 0)
        {
            foreach (GameObject enemyBeastObject in enemyBeasts) {
                BeastController enemyBeast = GetEnemyBeastController(enemyBeastObject);
                foreach (GameObject beastObject in enemyBeast.enemyBeasts) {
                    BeastController beast = GetEnemyBeastController(beastObject);
                    if (beast == this) {
                        enemyBeast.enemyBeasts.Remove(beastObject);
                        if (enemyBeast.targetBeast == this) {
                            enemyBeast.targetBeast = null;
                        }
                        break;
                    }
                }
            }
            Destroy(gameObject);
        }
    }
}
