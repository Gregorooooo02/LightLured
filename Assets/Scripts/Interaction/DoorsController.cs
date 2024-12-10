using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorsController : MonoBehaviour
{
    private AudioSource doorAudioSource;
    [SerializeField] private AudioClip[] doorClips;
    private Animator animator;
    private bool doorOpen = false;

    private void Awake() {
        animator = GetComponent<Animator>();
        doorAudioSource = GetComponent<AudioSource>();
    }

    public void PlayAnimation() {
        if (doorOpen) {
            animator.Play("Close");
            doorOpen = false;
            doorAudioSource.PlayOneShot(doorClips[0]);
        }
        else {
            animator.Play("Open");
            doorOpen = true;
            doorAudioSource.PlayOneShot(doorClips[1]);
        }
    }
}
