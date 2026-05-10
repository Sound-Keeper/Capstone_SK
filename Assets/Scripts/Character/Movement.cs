using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    // basic movement for the character - reference GD2 final project and g3 projects
    CharacterController player;
    public Transform cameraTransform;

    float moveSpeed = 2f;
    float sprintSpeed = 5f;
    float rotationSpeed = 100f;
    float gravity = -9.81f;
    float jumpForce = 3f;
    float yVelocity;
    float pitch = 0f;

    void Start()
    {
        player = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector2 move = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();
        Vector2 look = InputSystem.actions.FindAction("Look").ReadValue<Vector2>();

        // Check sprint
        bool isSprinting = Keyboard.current.leftShiftKey.isPressed;
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        Vector3 movement = new Vector3(move.x, 0, move.y);

        // Ground check
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

        Vector3 finalMove = transform.TransformDirection(movement) * currentSpeed;
        finalMove.y = yVelocity;

        player.Move(finalMove * Time.deltaTime);

        // Look left/right - rotate the player
        transform.Rotate(0, look.x * rotationSpeed * Time.deltaTime, 0);

        // Look up/down - rotate the camera only
        if (cameraTransform != null)
        {
            pitch -= look.y * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -80f, 80f);
            cameraTransform.localEulerAngles = new Vector3(pitch, 0, 0);
        }
    }
}