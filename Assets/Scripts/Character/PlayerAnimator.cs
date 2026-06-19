using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimator : MonoBehaviour
{
    private Animator anim;
    private CharacterController controller;

    void Start()
    {
        anim = GetComponent<Animator>();

        // Find the CharacterController on the parent "Player" object
        controller = GetComponentInParent<CharacterController>();
    }

    void Update()
    {
        if (anim == null) return;

        // 1. DYNAMIC MOVEMENT & SPRINT CHECK
        if (controller != null)
        {
            // Calculate how fast the player is physically sliding on the X and Z axes
            Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
            float physicalSpeed = horizontalVelocity.magnitude;

            // If moving at all, set the "Walk" float parameter to 1, otherwise 0
            anim.SetFloat("Walk", physicalSpeed > 0.1f ? 1f : 0f);

            // Set Sprint parameter based on actual movement speed threshold (e.g., greater than walk speed)
            bool isSprinting = physicalSpeed > 2.5f;
            anim.SetBool("Sprint", isSprinting);
        }

        // 2. JUMP TRIGGER
        // We still use input actions for immediate triggers like jumps
        if (InputSystem.actions.FindAction("Jump").triggered)
        {
            anim.SetTrigger("Jump");
        }

        // 3. INTERACT TRIGGER
        if (InputSystem.actions.FindAction("Interact") != null && InputSystem.actions.FindAction("Interact").triggered)
        {
            anim.SetTrigger("Interact");
        }
        else if (Input.GetKeyDown(KeyCode.E)) // Fallback if action isn't bound yet
        {
            anim.SetTrigger("Interact");
        }

        // 4. ATTACK STATE
        if (InputSystem.actions.FindAction("Attack").IsPressed())
        {
            anim.SetBool("Attack", true);
        }
        else
        {
            anim.SetBool("Attack", false);
        }
    }
}