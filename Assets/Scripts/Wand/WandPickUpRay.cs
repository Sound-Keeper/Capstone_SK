using UnityEngine;
using UnityEngine.InputSystem;

public class WandPickUpRay : MonoBehaviour
{
    public float range = 50f;
    public PlayerHold carry;
    public PieceHold pieceCarry; // the player's carrier for House A broken pieces
    public float crosshairSize = 10f;
    public Color crosshairColor = Color.white;

    void OnGUI()
    {
        // I added a crosshair since na duduling na ako HAHAHAHAHH
        //reference YT
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;
        GUI.color = crosshairColor;
        GUI.DrawTexture(new Rect(cx - crosshairSize, cy - 1, crosshairSize * 2, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1, cy - crosshairSize, 2, crosshairSize * 2), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void Update()
    {
        if (carry == null && pieceCarry == null) return;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Interact();
        }
    }

    void Interact()
    {
        Camera cam = Camera.main;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("Ray hit: " + hit.collider.name);

            // HOUSE I
            if (carry != null)
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

            // HOUSE A
            if (pieceCarry != null)
            {
                if (!pieceCarry.IsHolding())
                {
                    BrokenPiece piece = hit.collider.GetComponentInParent<BrokenPiece>();
                    if (piece != null)
                    {
                        pieceCarry.PickUp(piece);
                        return;
                    }
                }
                else
                {
                    PieceSlot slot = hit.collider.GetComponentInParent<PieceSlot>();
                    if (slot != null)
                    {
                        slot.PlacePiece(pieceCarry);
                        return;
                    }
                }
            }
        }
    }
}