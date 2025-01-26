using System.Collections;
using UnityEngine;

public class SkullBehaviour : MonoBehaviour
{
    public bool isMechanicOff = false;

    [Header("Skull Components")]
    [SerializeField] private GameObject meshRenderer;
    [SerializeField] private Light skullLight;
    [SerializeField] private ParticleSystem particles;

    [Header("Target")]
    [SerializeField] private Light lanternLight;
    [SerializeField] private GameObject player;
    [SerializeField] private float distanceToPlayer;
    [SerializeField] private float range = 25f;
    [SerializeField] private float particlesCap = 10f;
    private bool isLanternOn;

    private System.Random random = new System.Random();

    private void Awake() {
        meshRenderer.layer = 7;
        skullLight.enabled = false;
        if (particles.isPlaying)
            particles.Stop();
    }

    private void Update() {
        if (!isMechanicOff) {
            CheckLantern();    
        }
        else {
            if (GameManager.instance.shouldBooksWork) {
                NoMechanicBehaviour();
            }
        }
    }

    private void LateUpdate() {
        distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if (distanceToPlayer <= particlesCap) {
            particles.Stop();
        }
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
                StartCoroutine(SkullParticles());
        }
        else {
            meshRenderer.layer = 7;
            skullLight.enabled = false;

            if (particles.isPlaying)
                particles.Stop();
        }
    }

    private void NoMechanicBehaviour() {
        // If the player is near the skull, the skull will turn on
        if (distanceToPlayer <= range) {
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

    private IEnumerator SkullParticles() {
        yield return new WaitForSeconds(random.Next(1, 5));
        particles.Play();
    }
}
