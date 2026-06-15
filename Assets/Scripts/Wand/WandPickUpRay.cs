using UnityEngine;
using UnityEngine.InputSystem;

public class WandPickUpRay : MonoBehaviour
{
    public float range = 50f;
    public PlayerHold carry;
    public PieceHold pieceCarry;
    public float crosshairSize = 10f;
    public Color crosshairColor = Color.white;

    [Header("Raycast Masking")]
    public LayerMask interactableLayer;

    // ADDED: References for the visual laser line
    [Header("Visual Laser Settings")]
    public LineRenderer laserLine;
    public Transform laserOrigin; // Optional: Drag your Wand Tip here. If empty, it shoots from the camera center.

    void OnGUI()
    {
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;
        GUI.color = crosshairColor;
        GUI.DrawTexture(new Rect(cx - crosshairSize, cy - 1, crosshairSize * 2, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1, cy - crosshairSize, 2, crosshairSize * 2), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void Update()
    {
        // Draw the laser beam every frame so you can see where you are aiming
        DrawLaserBeam();

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Interact();
        }
    }

    void DrawLaserBeam()
    {
        if (laserLine == null) return;

        Camera cam = Camera.main;

        // Determine where the laser starts (Wand tip or Camera)
        Vector3 startPoint = laserOrigin != null ? laserOrigin.position : cam.transform.position;
        Vector3 direction = cam.transform.forward;

        laserLine.SetPosition(0, startPoint);

        // Perform a constant passive raycast to find where the laser should stop
        Ray passiveRay = new Ray(cam.transform.position, direction);
        if (Physics.Raycast(passiveRay, out RaycastHit hit, range, interactableLayer))
        {
            // If it hits an interactable target, snap the laser end point to that target
            laserLine.SetPosition(1, hit.point);
        }
        else
        {
            // If it hits nothing, extend the laser out to its maximum range
            laserLine.SetPosition(1, cam.transform.position + (direction * range));
        }
    }

    void Interact()
    {
        Camera cam = Camera.main;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 2f);

        // 🎯 FIXED: Re-added 'RaycastHit' right here before 'hit'
        if (Physics.Raycast(ray, out RaycastHit hit, range, interactableLayer))
        {
            Debug.Log("Ray hit: " + hit.collider.name);

            // =============================================================
            // HOUSE U (VOWEL CHALLENGE CHOSEN PIECE)
            // =============================================================
            // Check this first! If the player clicks a piece of vowel paper, 
            // process the click immediately and exit the interaction.
            VowelPaper paper = hit.collider.GetComponentInParent<VowelPaper>();
            if (paper != null)
            {
                paper.OnPaperClicked();
                return;
            }

            // =============================================================
            // HOUSE I
            // =============================================================
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

            // =============================================================
            // HOUSE A
            // =============================================================
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