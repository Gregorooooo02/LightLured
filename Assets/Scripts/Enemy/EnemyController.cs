using System.Collections;
using System.ComponentModel;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public bool isWithoutMechanic = false;

    private NavMeshAgent agent;
    
    [SerializeField] private Transform target;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Transform currentTarget;

    [Header("Lantern")]
    [SerializeField] private Light lanternLight;
    [SerializeField] private float aggroTime = 6.0f;
    [SerializeField] private float time = 0.0f;
    [SerializeField] private float timeToTarget = 0.0f;
    private float timeToAggro = 0.0f;
    private float lanternIntensity;

    [Header("Ranges")]
    [SerializeField] private float chaseRange = 1000000f;
    [SerializeField] private float chaseRangeNM = 100f;
    [SerializeField] private float stopChaseRangeNM = 150f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float postAttackPause = 2.0f; // Time to stop after an attack
    [SerializeField] private float wanderRange = 10f;
    [SerializeField] private float minimumWanderRange = 5f;

    [Header("Speeds")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float fasterChaseSpeed = 10f;
    [SerializeField] private float wanderSpeed = 2f;

    [Header("Booleans")]
    public bool isWandering = false;
    public bool isChasing = false;
    public bool isFasterChasing = false;
    public bool isTargetingPlayer = false;
    private bool isTargetingTrap = false;
    private bool hasAttacked = false;

    [Header("Teleport Settings")]
    [SerializeField] private float teleportRadius = 60f;
    [SerializeField] private float playerTooFarDistance = 100f;

    [Header("Chase Settings")]
    [SerializeField] private float overshootDistance = 5f;

    [Header("Footstep Parameters")]
    [SerializeField] private float wanderStepSpeed = 0.5f;
    [SerializeField] private float chaseStepMultiplier = 0.3f;
    [SerializeField] private float fasterChaseStepMultiplier = 0.2f;
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip[] woodClips;
    [SerializeField] private AudioClip[] stoneClips;
    [SerializeField] private AudioClip[] grassClips;
    private float footstepTimer = 0.0f;
    private float GetCurrentOffset => isChasing ? wanderStepSpeed * chaseStepMultiplier : isFasterChasing ? wanderStepSpeed * fasterChaseStepMultiplier : wanderStepSpeed;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource screamAudioSource;
    [SerializeField] private AudioClip[] screamClips;

    [SerializeField] private AudioSource idleAudioSource;
    [SerializeField] private AudioClip[] idleClips;
    private float idleTime = 7f;

    private bool isScreamingCoroutine = false;
    private bool isAttacking = false;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        lanternIntensity = lanternLight.intensity;
        agent.speed = wanderSpeed;

        playerTarget = target;
        currentTarget = playerTarget;
    }

    private void Start() {
        StartCoroutine(Wander());
        isWandering = false;
    }

    private void Update() {
        if (!isWithoutMechanic) {
            if (GameManager.instance != null) {
                if (!GameManager.instance.isGameWon) {
                    EnemyLanternBehaviour();
                }
            }
            else if (GameManagerNT.instance != null) {
                if (!GameManagerNT.instance.isGameWon) {
                    EnemyLanternBehaviour();
                }
            }    
        }
        else {
            EnemyNoMechanicBevahiour();
        }
    }

    private void EnemyNoMechanicBevahiour() {
        // Enemy should target the player, if the player is in the range of the enemy
        // If the player is too far from the enemy, the enemy should stop chasing the player
        float distanceToTarget = Vector3.Distance(currentTarget.position, transform.position);

        if (distanceToTarget > playerTooFarDistance) {
            TeleportToRandomPoint();
        }

        timeToAggro = 0.5f;

        if (distanceToTarget <= chaseRangeNM) {
            timeToTarget += Time.deltaTime;

            if (timeToTarget >= timeToAggro / 1.5f) {
                StopCoroutine(Wander());
                agent.SetDestination(transform.position);
            }
            if (timeToTarget >= timeToAggro && !isScreamingCoroutine) {
                StartCoroutine(ScreamAndTarget());
                isWandering = false;
            }

            if (isScreamingCoroutine) {
                ChasePlayer(distanceToTarget);
            }
        }
        else if (distanceToTarget > stopChaseRangeNM) {
            if (isScreamingCoroutine) {
                StopCoroutine(ScreamAndTarget());
                isScreamingCoroutine = false;
            }

            if (isAttacking) {
                StopCoroutine(AttackPlayer());
                StartCoroutine(ResetAttackState());
            }

            if (!isWandering) {
                StartCoroutine(Wander());
                isWandering = true;
                isChasing = false;
                isFasterChasing = false;
                isTargetingPlayer = false;
            }

            // If the player is too close to the enemy, attack the player
            if (!isAttacking && distanceToTarget <= attackRange) {
                StartCoroutine(AttackPlayer());
            }

            // Check if the agent is at the position of the target - if so, reset the isTargetinPlayer
            if (Vector3.Distance(transform.position, currentTarget.position) < 1f) {
                isTargetingPlayer = false;
            }
            
            timeToTarget = 0.0f;
            time = 0.0f;
        }

        HandleFootsteps();

        if (!isChasing) {
            HandleIdleSounds();
        }
    }

    private void EnemyLanternBehaviour() {
        float distanceToTarget = Vector3.Distance(currentTarget.position, transform.position);
        bool isLanternOn = lanternLight.intensity > 0;

        if (distanceToTarget > playerTooFarDistance) {
            TeleportToRandomPoint();
        }

        if (isLanternOn) {
            timeToTarget += Time.deltaTime;

            // Increase the aggro time based on the number of books collected
            if (GameManager.instance != null) {
                switch (GameManager.instance.booksCollected) {
                    case 1:
                        timeToAggro = Random.Range(8f, 12f);
                        break;
                    case 2:
                        timeToAggro = Random.Range(6f, 8f);
                        break;
                    case 3:
                        timeToAggro = Random.Range(4f, 7f);
                        break;
                    case 4:
                        timeToAggro = Random.Range(2f, 6f);
                        break;
                    case 5:
                        timeToAggro = Random.Range(0.5f, 4f);
                        break;
                }    
            }
            else if (GameManagerNT.instance != null) {
                timeToAggro = Random.Range(2f, 12f);
            }

            if (timeToTarget >= timeToAggro / 1.5f) {
                // Agent should stop moving when it's targeting the player
                StopCoroutine(Wander());
                agent.SetDestination(transform.position);
            }
            if (timeToTarget >= timeToAggro && !isScreamingCoroutine) {
                StartCoroutine(ScreamAndTarget());
                isWandering = false;
            }

            if (isScreamingCoroutine) {
                ChasePlayer(distanceToTarget);
            }
        }
        else {
            if (isScreamingCoroutine) {
                StopCoroutine(ScreamAndTarget());
                isScreamingCoroutine = false;
            }

            if (isAttacking) {
                StopCoroutine(AttackPlayer());
                StartCoroutine(ResetAttackState());
            }

            if (!isWandering) {
                StartCoroutine(Wander());
                isWandering = true;
                isChasing = false;
                isFasterChasing = false;
                isTargetingPlayer = false;
            }

            // If the player is too close to the enemy, attack the player
            if (!isAttacking && distanceToTarget <= attackRange) {
                StartCoroutine(AttackPlayer());
            }

            // Check if the agent is at the position of the target - if so, reset the isTargetinPlayer
            if (Vector3.Distance(transform.position, currentTarget.position) < 1f) {
                isTargetingPlayer = false;
            }
            
            timeToTarget = 0.0f;
            time = 0.0f;
        }

        HandleFootsteps();

        if (!isChasing) {
            HandleIdleSounds();
        }
    }

    public void SetTrapTarget(Transform trapTransform) {
        StartCoroutine(ScreamAndTarget());
    }

    private void TeleportToRandomPoint() {
        Vector2 randomPoint = Random.insideUnitCircle.normalized * teleportRadius;
        Vector3 teleportPosition = new Vector3(
            currentTarget.position.x + randomPoint.x,
            transform.position.y,
            currentTarget.position.z + randomPoint.y
        );

        transform.position = teleportPosition;
        agent.Warp(teleportPosition);
    }

    private IEnumerator ScreamAndTarget() {
        screamAudioSource.PlayOneShot(screamClips[Random.Range(0, screamClips.Length - 1)]);
        isTargetingPlayer = true;
        transform.LookAt(new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z));

        yield return new WaitForSeconds(1f);
        isScreamingCoroutine = true;
        yield return new WaitForSeconds(1f);
    }

    private void ChasePlayer(float distanceToTarget) {
        agent.acceleration = 20f;

        time += Time.deltaTime;
        // Rotate the enemy to face the player in the X and Z axes
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(
                new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z) - transform.position
            ).normalized,
            180f * Time.deltaTime
        );

        // If in chase range and hasn't attacked, pursue the player
        if (distanceToTarget <= chaseRange && !hasAttacked) {
            Vector3 directionToPlayer = (currentTarget.position - transform.position).normalized;
            Vector3 extendedTarget = currentTarget.position + directionToPlayer * overshootDistance;

            agent.SetDestination(extendedTarget);
            agent.speed = (time >= aggroTime) ? fasterChaseSpeed : chaseSpeed;
            isChasing = true;
            isFasterChasing = time >= aggroTime;
            
            // If in attack range and hasn't attacked yet, attack the player
            if (distanceToTarget <= attackRange) {
                StartCoroutine(AttackPlayer());
            }
        }
    }

    private void HandleFootsteps() {
        // If the agent is stopped, don't play footsteps
        if (agent.isStopped) return;
        // If the agent is not moving, don't play footsteps
        if (agent.velocity.magnitude <= 0) return;        

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0) {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 4)) {
                switch (hit.collider.tag) {
                    case "Footsteps/Grass":
                        footstepAudioSource.PlayOneShot(grassClips[Random.Range(0, grassClips.Length - 1)]);
                        break;
                    case "Footsteps/Wood":
                        footstepAudioSource.PlayOneShot(woodClips[Random.Range(0, woodClips.Length - 1)]);
                        break;
                    case "Footsteps/Stone":
                        footstepAudioSource.PlayOneShot(stoneClips[Random.Range(0, stoneClips.Length - 1)]);
                        break;
                    default:
                        footstepAudioSource.PlayOneShot(grassClips[Random.Range(0, grassClips.Length - 1)]);
                        break;
                }
            }

            footstepTimer = GetCurrentOffset;
        }
    }

    private void HandleIdleSounds() {
        idleTime -= Time.deltaTime;

        if (idleTime <= 0) {
            idleAudioSource.PlayOneShot(idleClips[Random.Range(0, idleClips.Length - 1)]);
            idleTime = 7f;
        }
    }

    private IEnumerator AttackPlayer() {
        isAttacking = true;
        hasAttacked = true;

        // Stop the monster and face the player
        agent.isStopped = true;
        transform.LookAt(new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z));
        // Perform the attack (e.g., apply damage)
        currentTarget.GetComponent<FirstPersonController>().TakeDamage(75); // Replace with your damage logic
        // Wait for the post-attack pause
        yield return new WaitForSeconds(postAttackPause);
        // Reset speed to initial chase speed after attack
        agent.speed = chaseSpeed;
        // Allow the monster to resume movement
        agent.isStopped = false;
        // Ensure the agent resumes chasing after the attack
        if (Vector3.Distance(currentTarget.position, transform.position) <= chaseRange) {
            agent.SetDestination(currentTarget.position); // Explicitly set the destination again to resume chasing
        }
        isAttacking = false;

        // Reset attack state after the player leaves the attack range
        StartCoroutine(ResetAttackState());
    }

    private IEnumerator ResetAttackState() {
        yield return new WaitUntil(() => Vector3.Distance(currentTarget.position, transform.position) > attackRange);

        hasAttacked = false;
        isAttacking = false;

        time = 0f; // Reset chase timer for aggro
        agent.isStopped = false;
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
        Gizmos.DrawWireSphere(transform.position, chaseRangeNM);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
