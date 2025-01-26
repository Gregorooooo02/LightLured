using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishController : MonoBehaviour
{
    public bool isMechanicOff = false;

    [Header("Finish Components")]
    [SerializeField] private Light finishLight;
    [SerializeField] private ParticleSystem particles;

    [Header("Target")]
    [SerializeField] private Light lanternLight;
    [SerializeField] private GameObject player;
    [SerializeField] private float distanceToPlayer;
    [SerializeField] private float particlesCap = 10f;
    private bool isLanternOn;

    [SerializeField] private int sceneChange;

    private void Update() {
        if (!isMechanicOff) {
            CheckLantern();
        }
        else {
            NoMechanicBehaviour();
        }
    }

    private void LateUpdate() {
        distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if (distanceToPlayer <= particlesCap) {
            particles.Stop();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (GameManager.instance != null) {
                GameManager.instance.isGameWon = true;
            }
            else if (GameManagerNT.instance != null) {
                GameManagerNT.instance.isGameWon = true;
            }
            LevelManager.instance.FadeIntoScene(sceneChange);
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

    private void NoMechanicBehaviour() {
        finishLight.enabled = true;
        if (!particles.isPlaying)
            particles.Play();
    }
}
