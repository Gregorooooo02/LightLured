using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearTrapController : MonoBehaviour
{
    [SerializeField] private FirstPersonController playerController;

    private Animator animator;
    private bool isTriggered = false;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !isTriggered) {
            isTriggered = true;
            animator.Play("Beartrap");

            // Damage the player
            playerController.TakeDamage(50);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && isTriggered) {
            isTriggered = false;
            animator.Play("CloseBeartrap");
        }
    }
}
