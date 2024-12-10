using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampScare : MonoBehaviour
{
    public static LampScare instance;

    private AudioSource lampAudioSource;
    [SerializeField] private AudioClip lampBreakSound;
    
    [SerializeField] private GameObject lightSource;
    [SerializeField] private ParticleSystem lightSparks;
    [SerializeField] private Animator scareAnimator;

    private void Awake() {
        instance = this;
        lampAudioSource = GetComponent<AudioSource>();
    }

    public IEnumerator BreakLight() {
        lightSource.SetActive(false);
        scareAnimator.enabled = false;
        lightSparks.Play();
        lampAudioSource.enabled = true;
        lampAudioSource.PlayOneShot(lampBreakSound);
        yield return new WaitForSeconds(0.15f);
        lightSparks.Stop();
        yield return new WaitForSeconds(0.5f);
        lampAudioSource.Stop();
    }
}
