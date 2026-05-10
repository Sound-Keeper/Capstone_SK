using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    CharacterController player;
    float moveSpeed = 2f;
    float rotationSpeed = 100f;
    float gravity = -9.81f;
    float jumpForce = 3f;

    float yVelocity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.AddComponent<CharacterController>();
        player = GetComponent<CharacterController>();
    }

   
    // Update is called once per frame
    void Update()
    {
        Vector2 move = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();
        Vector3 movement = new Vector3(move.x, 0, move.y);

        // Ground check
        if (player.isGrounded && yVelocity < 0)
        {
            yVelocity = -2f;
        }

        // Simple jump
        if (InputSystem.actions.FindAction("Jump").triggered && player.isGrounded)
        {
            yVelocity = jumpForce;
        }

        // Apply gravity
        yVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = transform.TransformDirection(movement) * moveSpeed;
        finalMove.y = yVelocity;

        player.Move(finalMove * Time.deltaTime);

        float rotY = InputSystem.actions.FindAction("Look").ReadValue<Vector2>().x;
        transform.Rotate(0, rotY * rotationSpeed * Time.deltaTime, 0);
    }
}
    
