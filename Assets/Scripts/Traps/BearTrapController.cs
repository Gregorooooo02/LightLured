using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearTrapController : MonoBehaviour
{
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private List<EnemyController> enemies;

    private Animator animator;
    private AudioSource trapAudioSource;

    public static bool isTriggered = false;

    private void Awake() {
        animator = GetComponent<Animator>();
        trapAudioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !isTriggered) {
            isTriggered = true;
            animator.Play("Beartrap");
            trapAudioSource.Play();

            // Damage the player
            playerController.TakeDamage(70);
            playerController.isHurtLegs = true;

            TriggerTrap();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && isTriggered) {
            isTriggered = false;
            animator.Play("CloseBeartrap");
        }
    }

    private void TriggerTrap() {
        if (enemies == null || enemies.Count == 0) return;

        EnemyController nearestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (EnemyController enemy in enemies) {
            if (enemy == null) continue;

            float distanceToTrap = Vector3.Distance(enemy.transform.position, transform.position);
            if (distanceToTrap < minDistance) {
                minDistance = distanceToTrap;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null) {
            nearestEnemy.SetTrapTarget(transform);
        }
    }
}
