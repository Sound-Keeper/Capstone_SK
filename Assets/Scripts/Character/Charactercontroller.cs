using UnityEngine;
using UnityEngine.InputSystem;

public class Charactercontroller : MonoBehaviour
{
    CharacterController player;

    public Transform charModel;

    // --- Control switch toggled by DialogueManager ---
    [HideInInspector] public bool canControl = true;

    float moveSpeed = 2f;
    float sprintSpeed = 5f;
    float rotationSpeed = 100f;
    float gravity = -9.8f;
    float jumpForce = 3f;

    float yVelocity;
    float rot = 0f;

    void Start()
    {
        player = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        if (CoreManager.Instance != null && CoreManager.Instance.HasSavedPosition)
        {
            Debug.Log("Restoring position: " + CoreManager.Instance.SavedPlayerPosition);
            player.enabled = false;
            transform.position = CoreManager.Instance.SavedPlayerPosition;
            transform.rotation = CoreManager.Instance.SavedPlayerRotation;
            player.enabled = true;
        }
        else
        {
            Debug.Log("No saved position found. HasSavedPosition: " +
                (CoreManager.Instance != null ? CoreManager.Instance.HasSavedPosition.ToString() : "CoreManager is NULL"));
        }
    }

    void Update()
    {
        // Guard check: Stop movement and rotation tracking if player is in a conversation
        if (!canControl)
        {
            // Still apply default physics/gravity so they fall smoothly if talking mid-air
            if (player.isGrounded && yVelocity < 0)
            {
                yVelocity = -2f;
            }
            yVelocity += gravity * Time.deltaTime;
            player.Move(new Vector3(0, yVelocity, 0) * Time.deltaTime);
            return;
        }

        Vector2 move = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();
        Vector2 look = InputSystem.actions.FindAction("Look").ReadValue<Vector2>();

        // Sprint
        bool isSprinting = Keyboard.current.leftShiftKey.isPressed;
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // Movement
        Vector3 movement = new Vector3(move.x, 0, move.y);

        // Ground Check
        if (player.isGrounded && yVelocity < 0)
        {
            yVelocity = -2f;
        }

        // Jump
        if (InputSystem.actions.FindAction("Jump").triggered && player.isGrounded)
        {
            yVelocity = jumpForce;
        }

        // Gravity
        yVelocity += gravity * Time.deltaTime;

        // Apply Movement
        Vector3 finalMove = transform.TransformDirection(movement) * currentSpeed;
        finalMove.y = yVelocity;

        player.Move(finalMove * Time.deltaTime);

        // LEFT / RIGHT LOOK
        transform.Rotate(Vector3.up * look.x * rotationSpeed * Time.deltaTime);

        // UP / DOWN LOOK
        rot -= look.y * rotationSpeed * Time.deltaTime;
        rot = Mathf.Clamp(rot, -80f, 37f);

        // Apply Camera Rotation
        if (charModel != null)
        {
            charModel.localRotation = Quaternion.Euler(rot, 0, 0);
        }
    }
}