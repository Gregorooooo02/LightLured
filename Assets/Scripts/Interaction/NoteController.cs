using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class NoteController : MonoBehaviour
{
    private AudioSource noteAudioSource;

    [Header("Game Objects")]
    [SerializeField] private GameObject noteWithoutOutline;
    [SerializeField] private GameObject lanternOutline;

    [Header("UI")] 
    [SerializeField] private RawImage noteImage;
    [SerializeField] private Volume volume;
    private DepthOfField depthOfField = null;

    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private NoteRaycast noteRaycast;

    private bool isLanternOn = false;

    private void Awake() {
        noteAudioSource = GetComponent<AudioSource>();
        volume.profile.TryGet(out depthOfField);
    }

    public void OpenNote() {
        noteAudioSource.Play();
        if (GameManager.instance != null) {
            GameManager.instance.CollectNote();    
        }
        else if (GameManagerNT.instance != null) {
            GameManagerNT.instance.CollectNote();
        }
        
        noteImage.gameObject.SetActive(true);
        // Turn on the depth of field effect
        depthOfField.active = true;

        firstPersonController.CanMove = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Deactivate the note raycast script
        noteRaycast.enabled = false;
    }

    public void CloseNote() {
        noteImage.gameObject.SetActive(false);
        // Turn off the depth of field effect
        depthOfField.active = false;

        firstPersonController.CanMove = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        noteWithoutOutline.SetActive(true);
        if (!isLanternOn) {
            lanternOutline.SetActive(true);
            isLanternOn = true;
        }
        gameObject.SetActive(false);

        // Activate the note raycast script
        noteRaycast.enabled = true;
    }
}
