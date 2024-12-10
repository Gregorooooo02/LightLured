using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearTrapController : MonoBehaviour
{
    [SerializeField] private FirstPersonController playerController;

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
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && isTriggered) {
            isTriggered = false;
            animator.Play("CloseBeartrap");
        }
    }
}
