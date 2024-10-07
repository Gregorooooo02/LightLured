using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private NavMeshAgent agent;
    
    [SerializeField] private Transform target;

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
    }

    private void Update() {
        float distanceToTarget = Vector3.Distance(target.position, transform.position);
        isLanternOn = lanternLight.enabled;

        if (isLanternOn) {
            time += Time.deltaTime;
            ChasePlayer(distanceToTarget);
        }
        else {
            StartCoroutine(Wander());
        }
    }

    private void ChasePlayer(float distanceToTarget) {
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
        // Wait for a few seconds before wandering again
        yield return new WaitForSeconds(2f);
        agent.speed = wanderSpeed;
        time = 0.0f;

        while (true) {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRange;

            if (randomDirection.magnitude < minimumWanderRange) {
                randomDirection = randomDirection.normalized * minimumWanderRange;
            }

            randomDirection += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRange, 1)) {
                Vector3 finalPosition = hit.position;
                agent.SetDestination(finalPosition);

                while (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance) {
                    yield return null;
                }

                yield return new WaitForSeconds(1f);
            }
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
