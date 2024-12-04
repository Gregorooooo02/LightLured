using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class NoteController : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] private GameObject noteWithoutOutline;
    [SerializeField] private GameObject lanternOutline;

    [Header("UI")] 
    [SerializeField] private RawImage noteImage;
    [SerializeField] private Volume volume;
    private DepthOfField depthOfField = null;

    [SerializeField] private FirstPersonController firstPersonController;

    private void Awake() {
        volume.profile.TryGet(out depthOfField);
    }

    public void OpenNote() {
        GameManager.instance.CollectNote();
        
        noteImage.gameObject.SetActive(true);
        // Turn on the depth of field effect
        depthOfField.active = true;

        firstPersonController.CanMove = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseNote() {
        noteImage.gameObject.SetActive(false);
        // Turn off the depth of field effect
        depthOfField.active = false;

        firstPersonController.CanMove = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        noteWithoutOutline.SetActive(true);
        lanternOutline.SetActive(true);
        gameObject.SetActive(false);
    }
}
