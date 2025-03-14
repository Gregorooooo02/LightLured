using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternRaycastController : MonoBehaviour
{
    private AudioSource lanternAudioSource;
    public bool lanternGrabbed = false;

    private void Awake() {
        lanternAudioSource = GetComponent<AudioSource>();
    }

    public void GrabLantern() {
        if (GameManager.instance != null) {
            GameManager.instance.CollectLantern();    
            StartCoroutine(GameManager.instance.DisplayText(GameManager.instance.obtainedLanternText));
            StartCoroutine(GameManager.instance.ChangeLightIntensity());
        }
        else if (GameManagerNT.instance != null) {
            GameManagerNT.instance.CollectLantern();
            StartCoroutine(GameManagerNT.instance.DisplayText(GameManagerNT.instance.obtainedLanternText));
            StartCoroutine(GameManagerNT.instance.ChangeLightIntensity());
        }

        lanternGrabbed = !lanternGrabbed;

        if (lanternGrabbed) {
            lanternAudioSource.Play();
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            gameObject.GetComponent<BoxCollider>().enabled = false;
            foreach (Transform child in transform) {
                child.gameObject.SetActive(false);
            }
        }
    }
}
