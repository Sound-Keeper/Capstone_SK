using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimator : MonoBehaviour
{
    // Sample Animation for the character using this wizard as beta char
    Animator anim;
    void Start()
    {
        anim  = GetComponent<Animator>();
    }
    void Update()
    {
        Vector2 move = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();

        //check lang if moving in diff directions
        //testing lang
        bool isMoving = false;
        if (move.x != 0 || move.y != 0)
        {
            isMoving = true;
        }

        //test if tatakbo
        bool isSprinting = Keyboard.current.leftShiftKey.isPressed;

        //walk animation
        if (isMoving)
        {
            { anim.SetFloat("Walk", 1f); }
        }
        else
        {
            { anim.SetFloat("Walk", 0); }
        }

        //takbo
        if (isSprinting & isMoving)
        {
            anim.SetBool("Sprint", true);
        }
        else
        {
            anim.SetBool("Sprint", false);
        }

        //jump animation
        if (InputSystem.actions.FindAction("Jump").triggered)
        {
            anim.SetTrigger("Jump");
        }

        //Test for interaction Systerm
        if (Keyboard.current.eKey.wasPressedThisFrame)
        { anim.SetTrigger("Interact"); }

        // for Attack and interaction with puzzle
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
