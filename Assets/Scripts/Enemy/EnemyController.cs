using System.Collections;
using System.ComponentModel;
using Unity.Collections;
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
    [SerializeField] private float timeToTarget = 0.0f;
    private float lanternIntensity;

    [Header("Ranges")]
    [SerializeField] private float chaseRange = 1000000f;
    [SerializeField] private float fasterChaseRange = 100f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float wanderRange = 10f;
    [SerializeField] private float minimumWanderRange = 5f;

    [Header("Speeds")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float fasterChaseSpeed = 10f;
    [SerializeField] private float wanderSpeed = 2f;

    [Header("Booleans")]
    private bool isWandering = false;
    public bool isTargetingPlayer = false;
    public bool isWanderingPlayer = false;
    public bool isChasingPlayer = false;
    public bool isFasterChasingPlayer = false;

    [SerializeField] private bool isScreamingCoroutine = false;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        lanternIntensity = lanternLight.intensity;
        agent.speed = wanderSpeed;
    }

    private void Start() {
        StartCoroutine(Wander());
        isWandering = false;
    }

    private void Update() {
        float distanceToTarget = Vector3.Distance(target.position, transform.position);
        bool isLanternOn = lanternLight.intensity > 0;

        if (isLanternOn) {
            timeToTarget += Time.deltaTime;

            float timeToAggro = Random.Range(3f, 6f);

            if (timeToTarget >= timeToAggro / 1.5f) {
                // Agent should stop moving when it's targeting the player
                StopCoroutine(Wander());
                agent.SetDestination(transform.position);
                isWanderingPlayer = false;
            }
            if (timeToTarget >= timeToAggro && !isScreamingCoroutine) {
                StartCoroutine(ScreamAndTarget());
                isWandering = true;
            }

            if (isScreamingCoroutine) {
                ChasePlayer(distanceToTarget);
            }
        }
        else if (isWandering && !isLanternOn) {
            StartCoroutine(Wander());
            isWanderingPlayer = false;
            // This is to prevent the coroutine from running multiple times
            isWandering = false;

            isTargetingPlayer = false;
            isChasingPlayer = false;
            isFasterChasingPlayer = false;

            isScreamingCoroutine = false;

            timeToTarget = 0.0f;
            time = 0.0f;
        }
    }

    private IEnumerator ScreamAndTarget() {
        isTargetingPlayer = true;
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

        yield return new WaitForSeconds(1f);

        isScreamingCoroutine = true;

        yield return new WaitForSeconds(1f);
    }

    private void ChasePlayer(float distanceToTarget) {
        agent.acceleration = 20f;
        
        // Rotate the enemy to face the player in the X and Z axes
        // Make sure the enemy is not looking up or down
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(
            new Vector3(target.position.x, transform.position.y, target.position.z) - transform.position).normalized, 180f * Time.deltaTime);

        time += Time.deltaTime;

        if (distanceToTarget <= chaseRange) {
            agent.SetDestination(target.position);
        }

        if (distanceToTarget <= fasterChaseRange || time >= aggroTime) {
            agent.speed = fasterChaseSpeed;
            isFasterChasingPlayer = true;
        } else {
            agent.speed = chaseSpeed;
            isChasingPlayer = true;
        }
    }

    // Wandering should be a coroutine that runs every few seconds
    private IEnumerator Wander() {
        isWanderingPlayer = false;
        yield return new WaitForSeconds(Random.Range(3f, 6f));
        agent.speed = wanderSpeed;
        agent.acceleration = 8f;

        isChasingPlayer = false;
        isFasterChasingPlayer = false;

        while (true) {
            // Calculate a new wander destination
            Vector2 randomDirection = Random.insideUnitCircle * (wanderRange - minimumWanderRange);
            if (randomDirection.magnitude < minimumWanderRange) {
                randomDirection = randomDirection.normalized * minimumWanderRange;
            }
            randomDirection += new Vector2(transform.position.x, transform.position.z);
            Vector3 wanderTarget = new Vector3(randomDirection.x, transform.position.y, randomDirection.y);

            // Rotate the enemy
            SmoothRotateTowards(wanderTarget);

            // Set the destination and enable wandering animation flag
            agent.SetDestination(wanderTarget);
            isWanderingPlayer = true;

            // Wait until the enemy reaches the destination or a short timeout to prevent being stuck
            float wanderTimeout = 5f;
            while (Vector3.Distance(transform.position, wanderTarget) > 1f && wanderTimeout > 0f) {
                yield return null;
                wanderTimeout -= Time.deltaTime;

                // Check agent velocity to ensure it’s actively moving
                if (agent.velocity.sqrMagnitude < 0.1f) {
                    isWanderingPlayer = false;  // Not moving, stop wandering animation
                } else {
                    isWanderingPlayer = true;  // Moving, play wandering animation
                }
            }

            // Stop wandering animation when reaching the destination
            isWanderingPlayer = false;
            yield return new WaitForSeconds(Random.Range(3f, 6f));
        }
    }
    private void SmoothRotateTowards(Vector3 targetPosition) {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        
        // Set rotation speed
        float rotationSpeed = 60f;  // Adjust this value to control the smoothness
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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
