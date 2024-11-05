using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternController : MonoBehaviour
{
    private GameObject lantern;

    [Header("Functional Options")]
    [SerializeField] private bool turnOnOffLantern = true;

    [Header("Controls")]
    [SerializeField] private KeyCode toggleLanternKey = KeyCode.F;

    [Header("Materials")]
    [SerializeField] private Material litMaterial;
    [SerializeField] private Material unlitMaterial;

    private void Awake() {
        lantern = GameObject.Find("Lantern");
    }

    private void Update() {
        if (turnOnOffLantern) {
            HandleLanternLight();    
        }
    }

    private void HandleLanternLight() {
        if (Input.GetKeyDown(toggleLanternKey)) {
            lantern.GetComponent<Light>().enabled = !lantern.GetComponent<Light>().enabled;

            if (lantern.GetComponent<Light>().enabled) {
                lantern.GetComponent<Renderer>().materials[2] = litMaterial;
            } else {
                lantern.GetComponent<Renderer>().materials[2] = unlitMaterial;
            }
        }
    }
}
