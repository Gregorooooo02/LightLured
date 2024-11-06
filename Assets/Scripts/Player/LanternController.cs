using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternController : MonoBehaviour
{
    private GameObject lantern;
    private GameObject[] lanternLights;

    private bool lanternOn = true;

    [Header("Functional Options")]
    [SerializeField] private bool turnOnOffLantern = true;

    [Header("Controls")]
    [SerializeField] private KeyCode toggleLanternKey = KeyCode.F;

    [Header("Light Settings")]
    [SerializeField] private Light lanternLight;
    [SerializeField] private Light additionalLight;
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private float lanternLightIntensity = 30f;
    [SerializeField] private float additionalLightIntensity = 1f;

    private Coroutine lightCoroutine;
    private Coroutine additionalLightCoroutine;

    private void Awake() {
        lantern = GameObject.Find("Lantern");
        lanternLights = GameObject.FindGameObjectsWithTag("LanternLight");
    }

    private void Update() {
        if (turnOnOffLantern) {
            HandleLanternLight();
        }
    }

    private void HandleLanternLight() {
        if (Input.GetKeyDown(toggleLanternKey)) {
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
            } else {
                lightCoroutine = StartCoroutine(GradualLightChange(lanternLight, 0f));
                additionalLightCoroutine = StartCoroutine(GradualLightChange(additionalLight, 0f));
            }
        }
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
