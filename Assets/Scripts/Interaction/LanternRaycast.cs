using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternRaycast : MonoBehaviour
{
    [SerializeField] private int rayLength = 5;
    [SerializeField] private LayerMask layerMaskInteract;
    [SerializeField] private string excludeLayerName = null;

    private LanternRaycastController raycastedObject;

    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [SerializeField] private GameObject grabText;
    private bool isTextActive;
    private bool doOnce;

    private const string interactableTag = "Interactable";

    void Update()
    {
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        int mask = 1 << LayerMask.NameToLayer(excludeLayerName) | layerMaskInteract.value;

        if (Physics.Raycast(transform.position, fwd, out hit, rayLength, mask)) {
            if (hit.collider.CompareTag(interactableTag)) {
                if (!doOnce) {
                    raycastedObject = hit.collider.gameObject.GetComponent<LanternRaycastController>();
                    ActivateText(true);
                }

                isTextActive = true;
                doOnce = true;

                if (GameManager.instance != null) {
                    if (Input.GetKeyDown(interactKey) && !raycastedObject.lanternGrabbed && GameManager.instance.hasNote) {
                        raycastedObject.GrabLantern();
                    }    
                }
                else if (GameManagerNT.instance != null) {
                    if (Input.GetKeyDown(interactKey) && !raycastedObject.lanternGrabbed && GameManagerNT.instance.hasNote) {
                        raycastedObject.GrabLantern();
                    }
                }
            }
        }
        else {
            if (isTextActive) {
                ActivateText(false);
                doOnce = false;
            }
        }
    }

    private void ActivateText(bool activate) {
        if (GameManager.instance != null) {
            if (GameManager.instance.hasNote) {
                if (activate && !doOnce && !raycastedObject.lanternGrabbed) {
                    grabText.SetActive(true);
                }
                else if (!raycastedObject.lanternGrabbed) {
                    grabText.SetActive(false);
                    isTextActive = false;
                }
                else {
                    grabText.SetActive(false);
                    isTextActive = false;
                }
                
            }
        }
        else if (GameManagerNT.instance != null) {
            if (GameManagerNT.instance.hasNote) {
                if (activate && !doOnce && !raycastedObject.lanternGrabbed) {
                    grabText.SetActive(true);
                }
                else if (!raycastedObject.lanternGrabbed) {
                    grabText.SetActive(false);
                    isTextActive = false;
                }
                else {
                    grabText.SetActive(false);
                    isTextActive = false;
                }
            }
        }
    }
}
