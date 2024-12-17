using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternController : MonoBehaviour
{
    private GameObject lantern;
    private GameObject[] lanternLights;

    private bool lanternOn = false;

    [SerializeField] private Animator lanternAnimator;

    [Header("Functional Options")]
    [SerializeField] public bool turnOnOffLantern = true;

    [Header("Controls")]
    [SerializeField] private KeyCode toggleLanternKey = KeyCode.F;

    [Header("Light Settings")]
    [SerializeField] private Light lanternLight;
    [SerializeField] private Light additionalLight;
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private float lanternLightIntensity = 30f;
    [SerializeField] private float additionalLightIntensity = 1f;

    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownTime = 1f;
    private bool canToggleLantern = true;

    [Header("Audio")]
    [SerializeField] private AudioSource lanternAudioSource;
    [SerializeField] private AudioClip[] lanternOnOffSounds;

    private Coroutine lightCoroutine;
    private Coroutine additionalLightCoroutine;

    private void Awake() {
        lantern = GameObject.Find("Lantern");
        lanternLights = GameObject.FindGameObjectsWithTag("LanternLight");

        lightCoroutine = StartCoroutine(GradualLightChange(lanternLight, 0f));
        additionalLightCoroutine = StartCoroutine(GradualLightChange(additionalLight, 0f));
        lanternAnimator.Play("Hide");
    }

    private void Update() {
        if (turnOnOffLantern && GameManager.instance.hasLantern) {
            HandleLanternLight();
        }
    }

    private void HandleLanternLight() {
        if (Input.GetKeyDown(toggleLanternKey) && canToggleLantern) {
            StartCoroutine(ToggleLight());
        }
    }

    private IEnumerator ToggleLight() {
        canToggleLantern = false;
        lanternOn = !lanternOn;

        if (lightCoroutine != null) {
            StopCoroutine(lightCoroutine);
        }

        if (additionalLightCoroutine != null) {
            StopCoroutine(additionalLightCoroutine);
        }

        if (lanternOn) {
            lightCoroutine = StartCoroutine(GradualLightChange(lanternLight, lanternLightIntensity));
            additionalLightCoroutine = StartCoroutine(GradualLightChange(additionalLight, additionalLightIntensity));

            lanternAnimator.Play("TurnOn");
            lanternAudioSource.PlayOneShot(lanternOnOffSounds[0]);
        } else {
            lightCoroutine = StartCoroutine(GradualLightChange(lanternLight, 0f));
            additionalLightCoroutine = StartCoroutine(GradualLightChange(additionalLight, 0f));

            lanternAnimator.Play("Hide");
            lanternAudioSource.PlayOneShot(lanternOnOffSounds[1]);
        }

        yield return new WaitForSeconds(cooldownTime);
        canToggleLantern = true;
    }

    private IEnumerator GradualLightChange(Light light, float targetIntensity) {
        float startIntensity = light.intensity;
        float time = 0f;

        while (time < 1f) {
            time += Time.deltaTime * transitionSpeed;
            light.intensity = Mathf.Lerp(startIntensity, targetIntensity, time);
            yield return null;
        }
    }
}
