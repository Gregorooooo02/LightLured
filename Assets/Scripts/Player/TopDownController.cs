using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class TopDownController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;
    private bool isSprinting => canSprint && Input.GetKey(sprintKey);
    
    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float walkRotation = 3.0f;
    [SerializeField] private float sprintRotation = 1.5f;

    private CharacterController characterController;
    private Vector3 moveDirection;
    private Vector2 currentInput;

    private void Awake() {
        characterController = GetComponent<CharacterController>();
    }

    private void FixedUpdate() {
        if (CanMove) {
            HandleMovementInput();

            ApplyFinalMovements();
        }
    }

    private void HandleMovementInput() {
        currentInput = new Vector2((isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxisRaw("Vertical"),
                                    (isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxisRaw("Horizontal"));

        float moveDirectionY = moveDirection.y;

        moveDirection = transform.TransformDirection(Vector3.forward) * currentInput.x;

        // Rotate the player with A and D
        transform.Rotate(Vector3.up, currentInput.y * (isSprinting ? sprintRotation : walkRotation) * Time.deltaTime);

        moveDirection.y = moveDirectionY;
    }

    private void ApplyFinalMovements() {
        characterController.Move(moveDirection * Time.deltaTime);
    }
}
