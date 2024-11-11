using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateController : MonoBehaviour
{
    private Animator animator;
    private EnemyController enemyController;

    private void Start() {
        animator = GetComponent<Animator>();
        enemyController = GetComponent<EnemyController>();
    }

    private void Update() {
        if (enemyController.isWanderingPlayer) {
            animator.SetBool("isWandering", true);
        }
        else {
            animator.SetBool("isWandering", false);
        }

        if (enemyController.isTargetingPlayer) {
            animator.SetBool("isTargeting", true);
        }
        else {
            animator.SetBool("isTargeting", false);
        }

        if (enemyController.isChasingPlayer) {
            animator.SetBool("isRunning", true);
        }
        else {
            animator.SetBool("isRunning", false);
        }

        if (enemyController.isFasterChasingPlayer) {
            animator.SetBool("isSprinting", true);
        }
        else {
            animator.SetBool("isSprinting", false);
        }
    }
}
