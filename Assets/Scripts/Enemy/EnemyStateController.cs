using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateController : MonoBehaviour
{
    private Animator animator;
    private EnemyController enemyController;
    private NavMeshAgent agent;

    [Header("Speed thresholds")]
    [SerializeField] private float wanderThreshold = 1f;
    [SerializeField] private float runThreshold = 3f;
    [SerializeField] private float sprintThreshold = 6f;

    private void Awake() {
        animator = GetComponent<Animator>();
        enemyController = GetComponent<EnemyController>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update() {
        float currentSpeed = agent.velocity.magnitude;

        animator.SetBool("isWandering", currentSpeed > 0 && currentSpeed < wanderThreshold);
        animator.SetBool("isRunning", currentSpeed > wanderThreshold && currentSpeed < runThreshold);
        animator.SetBool("isSprinting", currentSpeed > runThreshold && currentSpeed < sprintThreshold);

        animator.SetBool("isTargeting", enemyController.isTargetingPlayer);
    }
}
