using UnityEngine;
using UnityEngine.InputSystem;

public class WandPickUpRay : MonoBehaviour
{
    public float range = 20f;
    public PlayerHold carry;
    public Transform rayOrigin; // drag your wand tip (pointer) here

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Interact();
        }
    }

    void Interact()
    {
        // always shoots from the wand tip forward
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            // not holding anything — try to pick up a letter
            if (!carry.IsHolding())
            {
                LetterPickup letter = hit.collider.GetComponentInParent<LetterPickup>();
                if (letter != null)
                {
                    carry.PickUp(letter);
                    return;
                }
            }
            // already holding — try to place on a pillar
            else
            {
                Pillar slot = hit.collider.GetComponentInParent<Pillar>();
                if (slot != null)
                {
                    slot.PlaceLetter(carry);
                    return;
                }
            }
        }
    }
}