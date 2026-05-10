using UnityEngine;
using UnityEngine.InputSystem;

public class WandPickUpRay : MonoBehaviour
{
    public float range = 20f;
    public PlayerHold carry;
    public Transform rayOrigin;

    void Update()
    {
        #region
        // Only allow interaction when ANY puzzle is active
        // if (PuzzleManager.ActivePuzzle == null)
        //     return;

        // if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        // {
        //     Interact();
        // }
        #endregion
        
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {Interact();}
    }

    void Interact()
    {
        Ray ray;

        if (rayOrigin != null)
            ray = new Ray(rayOrigin.position, rayOrigin.forward);
        else
            ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
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
