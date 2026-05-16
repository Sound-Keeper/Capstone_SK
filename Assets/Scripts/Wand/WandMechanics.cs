using UnityEngine;
using UnityEngine.InputSystem;

public class WandMechanics : MonoBehaviour
{
    [SerializeField] private ParticleSystem magicEffect;

    void Update()
    {
        if (InputSystem.actions.FindAction("Attack").IsPressed())
        {
            if (!magicEffect.isPlaying)
                magicEffect.Play();
        }
        else
        {
            if (magicEffect.isPlaying)
                magicEffect.Stop();
        }
    }
}