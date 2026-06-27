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

    private Camera cachedCam;

    void Start()
    {
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
        if (cachedCam == null) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PerformInteraction(true);
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PerformInteraction(false);
        }
    }

    void PerformInteraction(bool isPressingE)
    {
        if (cachedCam == null) return;

        Ray ray = new Ray(cachedCam.transform.position, cachedCam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, range, interactableLayer))
        {
            Debug.Log("Ray hit: " + hit.collider.name);

            // =============================================================
            // UNIVERSAL INTERACTABLE CHECK (NPCs, Pip, items)
            // =============================================================
            if (isPressingE)
            {
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                    return; // Break execution immediately so child blocks aren't skipped or double evaluated
                }
            }

            // =============================================================
            // HOUSE U
            // =============================================================
            if (isPressingE)
            {
                BookChoice.HouseUInteractiveBook targetBook = hit.collider.GetComponentInParent<BookChoice.HouseUInteractiveBook>();
                if (targetBook != null)
                {
                    targetBook.StartInspectionViaRaycast();
                    return;
                }
            }
            else
            {
                BookChoice.HouseUChoiceButton targetChoice = hit.collider.GetComponentInParent<BookChoice.HouseUChoiceButton>();
                if (targetChoice != null)
                {
                    targetChoice.SelectChoice();
                    return;
                }
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