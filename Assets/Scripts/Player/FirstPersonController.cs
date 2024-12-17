using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstPersonController : MonoBehaviour
{
    public bool CanMove { get; set; } = true;
    private bool isSprinting => canSprint && Input.GetKey(sprintKey) && Input.GetAxis("Vertical") > 0 && !isHurtLegs;
    private bool shouldCrouch => canCrouch && Input.GetKeyDown(crouchKey) && !duringCrouchAnuimation;

    [Header("Functional Options")]
    [SerializeField] private bool hasHealth = true;
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool useFootsteps = true;

    [Header("Health Parameters")]
    [SerializeField] private Material screenDamageMaterial;
    [SerializeField] private float maxHealth = 50.0f;
    [SerializeField] private float currentHealth = 50.0f;
    [SerializeField] private float healthRegenRate = 0.5f;
    public bool isHurtLegs = false;
    private float nerfedLegsTimer = 0.0f;
    [SerializeField] private float nerfedLegsDuration = 5.0f;
    private bool isDead = false;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] public float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] public float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
    private bool duringCrouchAnuimation;

    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14.0f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18.0f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    [SerializeField] private float crouchBobSpeed = 10.0f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private float defaultYPos = 0;
    private float timer = 0;

    [Header("Footstep Parameters")]
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float crouchStepMultiplier = 1.5f;
    [SerializeField] private float sprintStepMultiplier = 0.6f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] woodClips = default;
    [SerializeField] private AudioClip[] stoneClips = default;
    [SerializeField] private AudioClip[] grassClips = default;
    private float footstepTimer = 0.0f;
    private float GetCurrentOffset => isCrouching ? baseStepSpeed * crouchStepMultiplier : isSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;

    [Header("Damage Sounds Parameters")]
    [SerializeField] private float breathingInterval = 2.0f;
    [SerializeField] private AudioSource breathingAudioSource = default;
    [SerializeField] private AudioClip[] breathingClips = default;
    [SerializeField] private AudioSource hurtAudioSource = default;
    [SerializeField] private AudioClip[] hurtClips = default;
    private float breathingTimer = 0.0f;

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currentInput;

    private float rotationX = 0;

    private bool isReloading = false;

    private void Awake() {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();

        currentHealth = maxHealth;
        isDead = false;

        defaultYPos = playerCamera.transform.localPosition.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        if (CanMove) {
            HandleMovementInput();
            HandleMouseLook();

            if (canCrouch) {
                HandleCrouch();
            }

            if (canUseHeadbob) {
                HandleHeadbob();
            }

            if (useFootsteps) {
                HandleFootsteps();
            }

            ApplyFinalMovements();
        }
    }

    private void FixedUpdate() {
        if (hasHealth) {
            if (currentHealth > -50) {
                RegenerateHealth();
                HandleHurtSound();
            }
            else {
                currentHealth = -50;
                isDead = true;
            }
            ScreenDamage();
        }

        if (isDead && !isReloading) {
            isReloading = true;
            CanMove = false;
            LevelManager.instance.FadeIntoScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (isHurtLegs) {
            nerfedLegsTimer += Time.deltaTime;
            if (nerfedLegsTimer >= nerfedLegsDuration) {
                isHurtLegs = false;
                nerfedLegsTimer = 0.0f;
            }
        }
    }

    private void RegenerateHealth() {
        if (currentHealth < maxHealth) {
            currentHealth += healthRegenRate * Time.deltaTime;
        }
    }

    private void ScreenDamage() {
        // If the vignette radius is 1, then the screen is clear
        // If the vignette radius is -1, then the screen is fully red
        float currentVignetteRadius = 1f;
        float targetRadius = isDead ? -1f : Mathf.Clamp(currentHealth / maxHealth, -1f, 1f);
        
        currentVignetteRadius = targetRadius;

        if (isDead) {
            currentVignetteRadius = -1f;
        }

        screenDamageMaterial.SetFloat("_VignetteRadius", currentVignetteRadius);
    }

    public void TakeDamage(float damage) {
        currentHealth -= damage;

        // Shake the camera with a random offset
        StartCoroutine(CameraShake(0.1f, 0.2f));

        // Play a random hurt sound
        hurtAudioSource.PlayOneShot(hurtClips[Random.Range(0, hurtClips.Length - 1)]);
    }

    private IEnumerator CameraShake(float duration, float magnitude) {
        Vector3 originalPos = playerCamera.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration) {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            playerCamera.transform.localPosition = new Vector3(
                originalPos.x + x,
                originalPos.y + y,
                originalPos.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.localPosition = originalPos;
    }

    private void HandleMovementInput() {
        currentInput = new Vector2((isSprinting ? sprintSpeed : isCrouching ? crouchSpeed : walkSpeed) * Input.GetAxis("Vertical"), 
                                    (isSprinting ? sprintSpeed : isCrouching ? crouchSpeed : walkSpeed) * Input.GetAxis("Horizontal"));
        
        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x)
                        + (transform.TransformDirection(Vector3.right) * currentInput.y);
        moveDirection.y = moveDirectionY;
    }

    private void HandleMouseLook() {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void HandleCrouch() {
        if (shouldCrouch) {
            StartCoroutine(CrouchStand());
        }
    }

    private void HandleHeadbob() {
        if (!characterController.isGrounded) {
            return;
        }

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f) {
            timer += Time.deltaTime * (isSprinting ? sprintBobSpeed : isCrouching ? crouchBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isSprinting ? sprintBobAmount : isCrouching ? crouchBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z
            );
        }
    }

    private void HandleFootsteps() {
        if (!characterController.isGrounded) return;
        if (currentInput == Vector2.zero) return;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0) {
            if (Physics.Raycast(characterController.transform.position, Vector3.down, out RaycastHit hit, 4)) {
                switch (hit.collider.tag) {
                    case "Footsteps/Grass":
                        footstepAudioSource.PlayOneShot(grassClips[Random.Range(0, grassClips.Length - 1)]);
                        break;
                    case "Footsteps/Wood":
                        footstepAudioSource.PlayOneShot(woodClips[Random.Range(0, woodClips.Length - 1)]);
                        break;
                    case "Footsteps/Stone":
                        footstepAudioSource.PlayOneShot(stoneClips[Random.Range(0, stoneClips.Length - 1)]);
                        break;
                    default:
                        footstepAudioSource.PlayOneShot(grassClips[Random.Range(0, grassClips.Length - 1)]);
                        break;
                }
            }

            footstepTimer = GetCurrentOffset;
        }
    }

    private void HandleHurtSound() {
        breathingAudioSource.volume = Mathf.Clamp(1 - (currentHealth / maxHealth), 0.0f, 0.6f);

        breathingTimer -= Time.deltaTime;

        if (breathingTimer <= 0) {
            breathingAudioSource.PlayOneShot(breathingClips[Random.Range(0, breathingClips.Length - 1)]);
            breathingTimer = breathingInterval;
        }
    }

    private IEnumerator CrouchStand() {
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
            yield break;

        duringCrouchAnuimation = true;

        float timeElapsed = 0.0f;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;

        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        while (timeElapsed < timeToCrouch) {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnuimation = false;
    }

    private void ApplyFinalMovements() {
        if (!characterController.isGrounded) {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    public void SetMouseSensitivity(float sensitivity) {
        lookSpeedX = sensitivity;
        lookSpeedY = sensitivity;
    }
}
