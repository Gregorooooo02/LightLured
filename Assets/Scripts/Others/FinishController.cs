using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishController : MonoBehaviour
{
    [Header("Finish Components")]
    [SerializeField] private Light finishLight;
    [SerializeField] private ParticleSystem particles;

    [Header("Target")]
    [SerializeField] private Light lanternLight;
    [SerializeField] private GameObject player;
    [SerializeField] private float distanceToPlayer;
    [SerializeField] private float particlesCap = 10f;
    private bool isLanternOn;

    private void Update() {
        CheckLantern();
    }

    private void LateUpdate() {
        distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if (distanceToPlayer <= particlesCap) {
            particles.Stop();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            GameManager.instance.isGameWon = true;
            LevelManager.instance.FadeIntoScene(4);
        }
    }

    private void CheckLantern() {
        if (lanternLight.intensity > 0)
            isLanternOn = true;
        else
            isLanternOn = false;

        if (isLanternOn) {
            finishLight.enabled = true;

            if (!particles.isPlaying)
                particles.Play();
        }
        else {
            finishLight.enabled = false;

            if (particles.isPlaying)
                particles.Stop();
        }
    }
}
