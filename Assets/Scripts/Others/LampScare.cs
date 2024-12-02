using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampScare : MonoBehaviour
{
    public static LampScare instance;

    [SerializeField] private GameObject lightSource;
    [SerializeField] private ParticleSystem lightSparks;

    private void Awake() {
        instance = this;
    }

    public IEnumerator BreakLight() {
        lightSource.SetActive(false);
        lightSparks.Play();
        yield return new WaitForSeconds(0.15f);
        lightSparks.Stop();
    }
}
