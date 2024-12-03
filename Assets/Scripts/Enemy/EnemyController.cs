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
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float postAttackPause = 2.0f; // Time to stop after an attack
    [SerializeField] private float wanderRange = 10f;
    [SerializeField] private float minimumWanderRange = 5f;

    [Header("Speeds")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float fasterChaseSpeed = 10f;
    [SerializeField] private float wanderSpeed = 2f;

    [Header("Booleans")]
    private bool isWandering = false;
    public bool isTargetingPlayer = false;
    private bool hasAttacked = false;

    [Header("Teleport Settings")]
    [SerializeField] private float teleportRadius = 60f;
    [SerializeField] private float playerTooFarDistance = 100f;

    [Header("Chase Settings")]
    [SerializeField] private float overshootDistance = 5f;

    private bool isScreamingCoroutine = false;

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

        if (distanceToTarget > playerTooFarDistance) {
            TeleportToRandomPoint();
        }

        if (isLanternOn) {
            timeToTarget += Time.deltaTime;

            float timeToAggro = Random.Range(3f, 6f);

            if (timeToTarget >= timeToAggro / 1.5f) {
                // Agent should stop moving when it's targeting the player
                StopCoroutine(Wander());
                agent.SetDestination(transform.position);
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
            StopAllCoroutines();
            StartCoroutine(Wander());
            // This is to prevent the coroutine from running multiple times
            isWandering = false;
            isTargetingPlayer = false;
            isScreamingCoroutine = false;

            timeToTarget = 0.0f;
            time = 0.0f;
        }

        if (!isLanternOn) {
            timeToTarget = 0.0f;
            time = 0.0f;
            // If the player is too close to the enemy, attack the player
            if (distanceToTarget <= attackRange) {
                StartCoroutine(AttackPlayer());
            }
        }
    }

    private void TeleportToRandomPoint() {
        Vector2 randomPoint = Random.insideUnitCircle.normalized * teleportRadius;
        Vector3 teleportPosition = new Vector3(
            target.position.x + randomPoint.x,
            transform.position.y,
            target.position.z + randomPoint.y
        );

        transform.position = teleportPosition;
        agent.Warp(teleportPosition);
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
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(
                new Vector3(target.position.x, transform.position.y, target.position.z) - transform.position
            ).normalized,
            180f * Time.deltaTime
        );

        // If in chase range and hasn't attacked, pursue the player
        if (distanceToTarget <= chaseRange && !hasAttacked) {
            Vector3 directionToPlayer = (target.position - transform.position).normalized;
            Vector3 extendedTarget = target.position + directionToPlayer * overshootDistance;

            agent.SetDestination(extendedTarget);

            // Increase speed after aggro time
            time += Time.deltaTime;
            if (time >= aggroTime) {
                agent.speed = fasterChaseSpeed;
            } else {
                agent.speed = chaseSpeed;
            }

            // If in attack range and hasn't attacked yet, attack the player
            if (distanceToTarget <= attackRange) {
                StartCoroutine(AttackPlayer());
            }
        }
    }

    private IEnumerator AttackPlayer() {
        hasAttacked = true;

        // Stop the monster and face the player
        agent.isStopped = true;
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

        // Perform the attack (e.g., apply damage)
        target.GetComponent<FirstPersonController>().TakeDamage(75); // Replace with your damage logic

        // Wait for the post-attack pause
        yield return new WaitForSeconds(postAttackPause);

        // Reset speed to initial chase speed after attack
        agent.speed = chaseSpeed;

        // Allow the monster to resume movement
        agent.isStopped = false;

        // Reset attack state after the player leaves the attack range
        StartCoroutine(ResetAttackState());
    }

    private IEnumerator ResetAttackState() {
        yield return new WaitUntil(() => Vector3.Distance(target.position, transform.position) > attackRange);
        hasAttacked = false;
        time = 0f; // Reset chase timer for aggro
    }

    // Wandering should be a coroutine that runs every few seconds
    private IEnumerator Wander() {
        yield return new WaitForSeconds(Random.Range(3f, 6f));
        agent.speed = wanderSpeed;
        agent.acceleration = 8f;

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

            // Wait until the enemy reaches the destination or a short timeout to prevent being stuck
            float wanderTimeout = 5f;
            while (Vector3.Distance(transform.position, wanderTarget) > 1f && wanderTimeout > 0f) {
                yield return null;
                wanderTimeout -= Time.deltaTime;
            }
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

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
