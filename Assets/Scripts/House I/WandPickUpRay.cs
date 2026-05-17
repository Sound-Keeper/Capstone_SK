using UnityEngine;
using UnityEngine.InputSystem;

public class WandPickUpRay : MonoBehaviour
{
    public float range = 50f;
    public PlayerHold carry;

    void Update()
    {
        if (carry == null) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Interact();
        }
    }

    void Interact()
    {
        Camera cam = Camera.main;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        // this shows in Scene view while playing
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("Ray hit: " + hit.collider.name);

            if (!carry.IsHolding())
            {
                LetterPickup letter = hit.collider.GetComponentInParent<LetterPickup>();
                if (letter != null)
                {
                    carry.PickUp(letter);
                    return;
                }
            }
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