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

    [Header("Audio Setup")]
    [Tooltip("Sound clip played when the player jumps.")]
    public AudioClip jumpSFX;
    [Tooltip("Sound clip played when the player takes a step.")]
    public AudioClip footstepSFX;
    [Tooltip("How fast footsteps play while walking (lower numbers = faster steps).")]
    public float walkStepInterval = 0.5f;
    [Tooltip("How fast footsteps play while sprinting (lower numbers = faster steps).")]
    public float sprintStepInterval = 0.3f;

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
    private float footstepTimer = 0f; // Track footstep pacing

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

            // --- AUDIO TRIGGER: Play Jump SFX ---
            if (jumpSFX != null)
            {
                CoreAudioManager.PlaySFX(jumpSFX);
            }
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

            Vector3 targetPivotPoint = transform.position + Vector3.up * cameraHeightOffset;
            Vector3 idealCameraDirection = cameraRotation * Vector3.forward;

            Vector3 maxCameraPosition = targetPivotPoint - (idealCameraDirection * cameraDistance);

            float adjustedDistance = cameraDistance;

            RaycastHit hit;
            Vector3 rayDirection = (maxCameraPosition - targetPivotPoint).normalized;

            if (Physics.Raycast(targetPivotPoint, rayDirection, out hit, cameraDistance, collisionLayers))
            {
                adjustedDistance = Mathf.Clamp(hit.distance - cameraCollisionBuffer, minCameraDistance, cameraDistance);
            }

            Vector3 finalCameraPosition = targetPivotPoint - (idealCameraDirection * adjustedDistance);

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

            // --- AUDIO TRIGGER: Handle Footstep Timing ---
            if (player.isGrounded)
            {
                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0f)
                {
                    if (footstepSFX != null)
                    {
                        CoreAudioManager.PlaySFX(footstepSFX, isSprinting ? 1.0f : 0.7f); // Slightly quieter when walking
                    }

                    // Reset timer based on current pacing
                    footstepTimer = isSprinting ? sprintStepInterval : walkStepInterval;
                }
            }
        }
        else
        {
            // Reset the timer instantly if they stop moving, so their next first step plays immediately
            footstepTimer = 0f;
        }

        finalMove.y = yVelocity;
        player.Move(finalMove * Time.deltaTime);
    }
}