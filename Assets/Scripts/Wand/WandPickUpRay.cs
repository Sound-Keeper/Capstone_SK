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

    // --- LASER VARIABLES REMOVED FROM HERE ---

    private Camera cachedCam;

    void Start()
    {
        // Cache the camera component to prevent Camera.main null crashes
        cachedCam = GetComponent<Camera>();
        if (cachedCam == null) cachedCam = Camera.main;
    }

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
        if (cachedCam == null) return; // Ultimate safety guard

        // --- DRAW LASER CALL REMOVED FROM HERE ---

        // 1. E KEY PRESS: Pop out the 3D Book asset
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PerformInteraction(true);
        }

        // 2. LEFT CLICK: Select vowel cubes / pick up objects
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PerformInteraction(false);
        }
    }

    // --- DRAWLASERBEAM() FUNCTION ENTIRELY REMOVED FROM HERE ---

    void PerformInteraction(bool isPressingE)
    {
        if (cachedCam == null) return;

        Ray ray = new Ray(cachedCam.transform.position, cachedCam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 2f);

        // We use the specified layout mask here for precise selections
        if (Physics.Raycast(ray, out RaycastHit hit, range, interactableLayer))
        {
            Debug.Log("Ray hit: " + hit.collider.name);

            if (isPressingE)
            {
                // =============================================================
                // 3D BOOK POP OUT ACTION (E KEY)
                // =============================================================
                Uhouse3DManager bookManager = hit.collider.GetComponentInParent<Uhouse3DManager>();
                if (bookManager != null)
                {
                    bookManager.InteractWithBook();
                    return;
                }
            }
            else
            {
                // =============================================================
                // HOUSE U (3D CUBE CLICK)
                // =============================================================
                VowelCube3D cube3D = hit.collider.GetComponentInParent<VowelCube3D>();
                if (cube3D != null)
                {
                    cube3D.OnCubeClicked();
                    return;
                }

                // BACKWARDS COMPATIBILITY (If you still have 2D paper objects active)
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
}