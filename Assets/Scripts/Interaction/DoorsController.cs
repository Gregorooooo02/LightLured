using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorsController : MonoBehaviour
{
    private Animator animator;
    private bool doorOpen = false;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    public void PlayAnimation() {
        if (doorOpen) {
            animator.Play("Close");
            doorOpen = false;
        }
        else {
            animator.Play("Open");
            doorOpen = true;
        }
    }
}
