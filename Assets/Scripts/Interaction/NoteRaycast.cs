using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRaycast : MonoBehaviour
{
    [SerializeField] private int rayLength = 5;
    [SerializeField] private LayerMask layerMaskInteract;
    [SerializeField] private string excludeLayerName = null;

    private NoteController raycastedObject;

    [SerializeField] private KeyCode openNoteKey = KeyCode.E;

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
                    raycastedObject = hit.collider.gameObject.GetComponent<NoteController>();
                    ActivateText(true);
                }

                isTextActive = true;
                doOnce = true;

                if (Input.GetKeyDown(openNoteKey)) {
                    raycastedObject.OpenNote();
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
