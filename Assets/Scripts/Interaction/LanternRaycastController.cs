using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternRaycastController : MonoBehaviour
{
    public bool lanternGrabbed = false;

    public void GrabLantern() {
        GameManager.instance.CollectLantern();
        StartCoroutine(GameManager.instance.DisplayText(GameManager.instance.obtainedLanternText));

        lanternGrabbed = !lanternGrabbed;

        if (lanternGrabbed) {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            gameObject.GetComponent<BoxCollider>().enabled = false;
            foreach (Transform child in transform) {
                child.gameObject.SetActive(false);
            }
        }
    }
}
