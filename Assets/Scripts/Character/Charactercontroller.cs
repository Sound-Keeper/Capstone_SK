using UnityEngine;
using UnityEngine.InputSystem;

public class Charactercontroller : MonoBehaviour
{
    CharacterController player;

    [Header("Hierarchy Assignments")]
    [Tooltip("Drag the 'Camera' child object here.")]
    public Transform charModel;

    [Tooltip("Drag the 'Paige_Mesh' child object here.")]
    public Transform characterVisualMesh;

    [Header("Third-Person Camera Settings")]
    [Tooltip("How far away from the player the camera should ideally orbit.")]
    public float cameraDistance = 5.0f;
    [Tooltip("Minimum distance the camera can get to the player when pushed by a wall.")]
    public float minCameraDistance = 0.5f;
    [Tooltip("Height offset so the camera looks at the player's head/chest instead of their feet.")]
    public float cameraHeightOffset = 1.5f;

    [Header("Camera Collision Settings")]
    [Tooltip("Select the layers the camera should collide with (e.g., Default, Environment). DO NOT select the Player layer!")]
    public LayerMask collisionLayers;
    [Tooltip("How far away from walls the camera should stay to prevent near-clip plane seeing inside walls.")]
    public float cameraCollisionBuffer = 0.2f;

    // --- Control switch toggled by DialogueManager ---
    [HideInInspector] public bool canControl = true;
    public static float MouseSensitivityMultiplier = 1.0f;

    float moveSpeed = 2f;
    float sprintSpeed = 5f;
    float rotationSpeed = 100f;
    float gravity = -9.8f;
    float jumpForce = 3f;

    float yVelocity;
    float rot = 0f;

    private float cameraYaw = 0f;

    void Start()
    {
        player = GetComponent<CharacterController>();

        if (DialogueManager.Instance != null && DialogueManager.Instance.dialoguePanel != null)
        {
            if (DialogueManager.Instance.dialoguePanel.activeSelf)
            {
                canControl = false;
            }
        }

        if (charModel != null)
        {
            cameraYaw = charModel.localEulerAngles.y;

            float initialX = charModel.localEulerAngles.x;
            if (initialX > 180f) initialX -= 360f;
            rot = initialX;
        }

        if (canControl)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void Update()
    {
        if (!canControl)
        {
            if (player.isGrounded && yVelocity < 0)
            {
                yVelocity = -2f;
            }
            yVelocity += gravity * Time.deltaTime;
            player.Move(new Vector3(0, yVelocity, 0) * Time.deltaTime);
            return;
        }
        if (PauseMenu.GameisPaused)
        {
            return;
        }

        Vector2 move = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();
        Vector2 look = InputSystem.actions.FindAction("Look").ReadValue<Vector2>();

        float baselineScale = 0.1f;
        float finalSensitivity = rotationSpeed * MouseSensitivityMultiplier * baselineScale * Time.unscaledDeltaTime;

        // Sprint
        bool isSprinting = Keyboard.current.leftShiftKey.isPressed;
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

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

        // ============================================================
        // 1. TRUE THIRD-PERSON CAMERA ORBIT WITH COLLISION
        // ============================================================
        if (charModel != null)
        {
            cameraYaw += look.x * finalSensitivity;
            rot -= look.y * finalSensitivity;
            rot = Mathf.Clamp(rot, -80f, 80f);

            Quaternion cameraRotation = Quaternion.Euler(rot, cameraYaw, 0f);

            // Calculate the pivot point (player center) and the ideal max position of the camera
            Vector3 targetPivotPoint = transform.position + Vector3.up * cameraHeightOffset;
            Vector3 idealCameraDirection = cameraRotation * Vector3.forward;

            // This is where the camera wants to sit if no walls are in the way
            Vector3 maxCameraPosition = targetPivotPoint - (idealCameraDirection * cameraDistance);

            // Current distance default to max
            float adjustedDistance = cameraDistance;

            // Shoot a ray from the player pivot out to the maximum camera range to check for obstacles
            RaycastHit hit;
            Vector3 rayDirection = (maxCameraPosition - targetPivotPoint).normalized;

            if (Physics.Raycast(targetPivotPoint, rayDirection, out hit, cameraDistance, collisionLayers))
            {
                // If we hit something, pull the camera closer, leaving a buffer space away from the wall surface
                adjustedDistance = Mathf.Clamp(hit.distance - cameraCollisionBuffer, minCameraDistance, cameraDistance);
            }

            // Calculate final safe position
            Vector3 finalCameraPosition = targetPivotPoint - (idealCameraDirection * adjustedDistance);

            // Apply global positions and rotations explicitly
            charModel.position = finalCameraPosition;
            charModel.rotation = cameraRotation;
        }

        // ============================================================
        // 2. CAMERA-RELATIVE CARDINAL MOVEMENT
        // ============================================================
        Vector3 finalMove = Vector3.zero;

        if (move.sqrMagnitude > 0.01f && charModel != null)
        {
            Vector3 camForward = charModel.forward;
            Vector3 camRight = charModel.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 targetMovementDirection = (camForward * move.y + camRight * move.x).normalized;
            finalMove = targetMovementDirection * currentSpeed;

            if (characterVisualMesh != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetMovementDirection);
                characterVisualMesh.rotation = Quaternion.Slerp(characterVisualMesh.rotation, targetRotation, Time.deltaTime * 15f);
            }
        }

        finalMove.y = yVelocity;
        player.Move(finalMove * Time.deltaTime);
    }
}