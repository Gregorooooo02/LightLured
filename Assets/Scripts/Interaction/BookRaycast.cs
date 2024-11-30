using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookRaycast : MonoBehaviour
{
    [SerializeField] private int rayLength = 5;
    [SerializeField] private LayerMask layerMaskInteract;
    [SerializeField] private string excludeLayerName = null;

    private BookController raycastedObject;

    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [SerializeField] private GameObject grabText;
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
                    raycastedObject = hit.collider.gameObject.GetComponent<BookController>();
                    ActivateText(true);
                }

                isTextActive = true;
                doOnce = true;

                if (Input.GetKeyDown(interactKey) && !raycastedObject.bookGrabbed) {
                    raycastedObject.GrabBook();
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
        if (activate && !doOnce && !raycastedObject.bookGrabbed) {
            grabText.SetActive(true);
        }
        else if (!raycastedObject.bookGrabbed) {
            grabText.SetActive(false);
            isTextActive = false;
        }
        else {
            grabText.SetActive(false);
            isTextActive = false;
        }
    }
}
