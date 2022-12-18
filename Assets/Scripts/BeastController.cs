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
    public bool isPlayer;
    public List<GameObject> enemyBeasts;
    public Strategy strategy;
    public TMP_Text healthText;

    public State currentState;

    public bool rotateClockwise;
    public float health;
    public float attack;
    public Color color;

    public float cooldown;
    public float windup;

    public float engageRange;
    public float attackRange;

    public NavMeshAgent navAgent;

    public bool isFlickerEnabled = false;
    public bool block = false;
    public float blockTimer = 0;

    public bool IsWithinEngageRange()
    {
        return Vector3.Distance(transform.position, enemyBeasts[0].transform.position) < engageRange;
    }

    public bool IsWithinAttackRange()
    {
        return Vector3.Distance(transform.position, enemyBeasts[0].transform.position) < attackRange;
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
        return new Vector3(center.x + rndPos.x, center.y, center.z + rndPos.y);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get the nav agent
        navAgent = GetComponent<NavMeshAgent>();
        color = GetComponent<Renderer>().material.color;
        windup = Random.Range(0.5f, 1.5f);
        healthText.text = health.ToString();

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

        SetState(new MoveState(this));
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

        if (blockTimer > 0) {
            blockTimer -= Time.deltaTime;
        } else {
            block = false;
        }

        if (enemyBeasts[0].GetComponent<BeastController>().currentState is AttackState && currentState is not AttackState && !block)
        {
            if (Random.Range(0, 2) == 0 && IsWithinAttackRange())
            {
                SetState(new AttackState(this));
            } else {
                block = true;
                blockTimer = 2;
                SetState(new MoveState(this));
            }
        }

        currentState.Tick();
    }

    public void SetState(State state)
    {
        if (currentState != null)
            currentState.OnStateExit();

        currentState = state;
        gameObject.name = "Beast - " + state.GetType().Name;

        if (currentState != null)
            currentState.OnStateEnter();
    }

    public void Hit(float damage)
    {
        if (block) {
            block = false;
            return;
        }

        // if (strategy == Strategy.Defensive)
        //     damage = damage / 2;
        // else if (strategy == Strategy.Evasive)
        //     damage = damage * 2;
        
        health -= damage;
        // Get text element with name "BeastHP" and set its text to health
        healthText.text = health.ToString();
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
