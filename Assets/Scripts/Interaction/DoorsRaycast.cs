using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class DoorsRaycast : MonoBehaviour
{
    [SerializeField] private int rayLength = 5;
    [SerializeField] private LayerMask layerMaskInteract;
    [SerializeField] private string excludeLayerName = null;

    private DoorsController raycastedObject;

    [SerializeField] private KeyCode openDoorKey = KeyCode.E;

    [SerializeField] private GameObject openText;
    private bool isTextActive;
    private bool doOnce;
    
    private const string interactableTag = "Interactable";

    private void Update() {
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        int mask = 1 << LayerMask.NameToLayer(excludeLayerName) | layerMaskInteract.value;

        if (Physics.Raycast(transform.position, fwd, out hit, rayLength, mask)) {
            if (hit.collider.CompareTag(interactableTag)) {
                if (!doOnce) {
                    raycastedObject = hit.collider.gameObject.GetComponent<DoorsController>();
                    ActivateText(true);
                }

                isTextActive = true;
                doOnce = true;

                if (Input.GetKeyDown(openDoorKey)) {
                    raycastedObject.PlayAnimation();
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
        if (activate && !doOnce) {
            openText.SetActive(true);
        }
        else {
            openText.SetActive(false);
            isTextActive = false;
        }
    }
}
