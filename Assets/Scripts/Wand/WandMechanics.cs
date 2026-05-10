using UnityEngine;
using UnityEngine.InputSystem;

public class WandMechanics : MonoBehaviour
{
    [SerializeField] private ParticleSystem magicEffect; 

    void Update()
    {
        if (InputSystem.actions.FindAction("Attack").triggered)
        {
            magicEffect.Play(); // Wand pointer
        }
    }
}
