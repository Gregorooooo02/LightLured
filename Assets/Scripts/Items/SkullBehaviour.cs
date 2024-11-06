using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullBehaviour : MonoBehaviour
{
    [Header("Skull Components")]
    [SerializeField] private GameObject meshRenderer;
    [SerializeField] private Light skullLight;
    [SerializeField] private ParticleSystem particles;

    [Header("Target")]
    [SerializeField] private Light lanternLight;
    private bool isLanternOn;

    private void Update() {
        CheckLantern();
    }

    private void CheckLantern() {
        if (lanternLight.intensity > 0)
            isLanternOn = true;
        else
            isLanternOn = false;

        if (isLanternOn) {
            meshRenderer.layer = 30;
            skullLight.enabled = true;

            if (!particles.isPlaying)
                particles.Play();
        }
        else {
            meshRenderer.layer = 7;
            skullLight.enabled = false;

            if (particles.isPlaying)
                particles.Stop();
        }
    }
}
