using UnityEngine;
using UnityEngine.InputSystem;

public class Movemen : MonoBehaviour
{
    CharacterController player;

    float moveSpeed = 2f;
    float sprintSpeed = 5f;   // sprint speed
    float rotationSpeed = 100f;

    float gravity = -9.81f;
    float jumpForce = 3f;

    float yVelocity;

    void Start()
    {
        player = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector2 move = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();

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

        float rotY = InputSystem.actions.FindAction("Look").ReadValue<Vector2>().x;
        transform.Rotate(0, rotY * rotationSpeed * Time.deltaTime, 0);
    }
}