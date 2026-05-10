using UnityEngine;
using UnityEngine.InputSystem;

public class WandAttack : MonoBehaviour
{
    [SerializeField] private ParticleSystem magicEffect;

    void Update()
    {
        if (InputSystem.actions.FindAction("Attack").triggered)
        {
            magicEffect.Play();
        }

        
        // if (Mouse.current.leftButton.wasPressedThisFrame)
        // {
        //     magicEffect.Play();
        // }
    }
}