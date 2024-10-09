using System.Collections;
using System.ComponentModel;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private NavMeshAgent agent;
    
    [SerializeField] private Transform target;
    private bool isWandering;

    [Header("Lantern")]
    [SerializeField] private Light lanternLight;
    [SerializeField] private float aggroTime = 6.0f;
    [SerializeField] private float time = 0.0f;
    private bool isLanternOn;

    [Header("Ranges")]
    [SerializeField] private float chaseRange = 1000000f;
    [SerializeField] private float fasterChaseRange = 100f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float wanderRange = 10f;
    [SerializeField] private float minimumWanderRange = 5f;

    [Header("Speeds")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float fasterChaseSpeed = 10f;
    [SerializeField] private float attackSpeed = 15f;
    [SerializeField] private float wanderSpeed = 2f;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        
        isLanternOn = lanternLight.enabled;

        agent.speed = wanderSpeed;
    }

    private void Start() {
        StartCoroutine(Wander());
        isWandering = false;
    }

    private void Update() {
        float distanceToTarget = Vector3.Distance(target.position, transform.position);
        isLanternOn = lanternLight.enabled;

        if (isLanternOn) {
            time += Time.deltaTime;

            StopCoroutine(Wander());
            isWandering = true;

            ChasePlayer(distanceToTarget);
        }
        else if (isWandering && !isLanternOn) {
            StartCoroutine(Wander());
            // This is to prevent the coroutine from running multiple times
            isWandering = false;
        }
    }

    private void ChasePlayer(float distanceToTarget) {
        agent.acceleration = 20f;
        if (distanceToTarget <= chaseRange) {
            agent.SetDestination(target.position);
        }

        if (distanceToTarget <= fasterChaseRange) {
            agent.speed = fasterChaseSpeed;
        } else {
            agent.speed = chaseSpeed;
        }

        // If the aggro time is up, start chasing faster
        if (time >= aggroTime) {
            agent.speed = fasterChaseSpeed;
        }
    }

    // Wandering should be a coroutine that runs every few seconds
    private IEnumerator Wander() {
        yield return new WaitForSeconds(Random.Range(3f, 6f));
        agent.speed = wanderSpeed;
        agent.acceleration = 8f;
        time = 0.0f;

        while (true) {
            Vector2 randomDirection = Random.insideUnitCircle * (wanderRange - minimumWanderRange);

            if (randomDirection.magnitude < minimumWanderRange) {
                randomDirection = randomDirection.normalized * minimumWanderRange;
            }

            randomDirection += new Vector2(transform.position.x, transform.position.z);

            agent.SetDestination(new Vector3(randomDirection.x, transform.position.y, randomDirection.y));

            yield return new WaitForSeconds(Random.Range(3f, 6f));
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fasterChaseRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
