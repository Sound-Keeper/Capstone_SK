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

    private BookChoice.HouseUChoiceButton lastHoveredChoice;
    private BookChoice.HouseUInteractiveBook lastHoveredBook;

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

        ProcessHoverCheck();

        // 'E' Key pressed -> Exclusively passes parameters indicating an E press
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PerformInteraction(isPressingE: true, isLeftClick: false);
        }

        // Left Mouse Click pressed -> Exclusively passes parameters indicating a Mouse click
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PerformInteraction(isPressingE: false, isLeftClick: true);
        }
    }

    void ProcessHoverCheck()
    {
        Ray ray = new Ray(cachedCam.transform.position, cachedCam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, interactableLayer))
        {
            // --- 1. HOVERING OVER A CHOICE PAPER/BUTTON ---
            BookChoice.HouseUChoiceButton currentChoice = hit.collider.GetComponent<BookChoice.HouseUChoiceButton>();
            if (currentChoice == null)
            {
                currentChoice = hit.collider.GetComponentInParent<BookChoice.HouseUChoiceButton>();
            }

            if (currentChoice != null)
            {
                ResetBookHoverState();

                // If it's a new choice look, or if we re-entered it after looking away
                if (currentChoice != lastHoveredChoice)
                {
                    lastHoveredChoice = currentChoice;

                    // Request clip (the button script will return null if its cooldown is still active)
                    AudioClip clipToPlay = currentChoice.GetHoverSFX();
                    if (clipToPlay != null)
                    {
                        CoreAudioManager.PlaySFX(clipToPlay);
                    }
                }
                return;
            }

            // --- 2. HOVERING OVER AN INTERACTIVE SHELF BOOK ---
            BookChoice.HouseUInteractiveBook currentBook = hit.collider.GetComponent<BookChoice.HouseUInteractiveBook>();
            if (currentBook == null)
            {
                currentBook = hit.collider.GetComponentInParent<BookChoice.HouseUInteractiveBook>();
            }

            if (currentBook != null)
            {
                lastHoveredChoice = null; // Clear choice memory

                if (currentBook != lastHoveredBook)
                {
                    ResetBookHoverState();
                    lastHoveredBook = currentBook;
                    lastHoveredBook.SetOutlineHover(true);
                }
                return;
            }

            ClearAllHoverStates();
        }
        else
        {
            ClearAllHoverStates();
        }
    }

    void ClearAllHoverStates()
    {
        lastHoveredChoice = null;
        ResetBookHoverState();
    }

    void ResetBookHoverState()
    {
        if (lastHoveredBook != null)
        {
            lastHoveredBook.SetOutlineHover(false);
            lastHoveredBook = null;
        }
    }

    void PerformInteraction(bool isPressingE, bool isLeftClick)
    {
        if (cachedCam == null) return;

        Ray ray = new Ray(cachedCam.transform.position, cachedCam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, interactableLayer))
        {
            // =============================================================
            // HOUSE U (Choices & Books) -> MOUSE CLICK ONLY
            // =============================================================
            if (isLeftClick)
            {
                BookChoice.HouseUChoiceButton targetChoice = hit.collider.GetComponent<BookChoice.HouseUChoiceButton>();
                if (targetChoice == null) targetChoice = hit.collider.GetComponentInParent<BookChoice.HouseUChoiceButton>();

                if (targetChoice != null)
                {
                    targetChoice.SelectChoice();
                    return;
                }

                BookChoice.HouseUInteractiveBook targetBook = hit.collider.GetComponent<BookChoice.HouseUInteractiveBook>();
                if (targetBook == null) targetBook = hit.collider.GetComponentInParent<BookChoice.HouseUInteractiveBook>();

                if (targetBook != null)
                {
                    targetBook.StartInspectionViaRaycast();
                    return;
                }
            }

            // =============================================================
            // UNIVERSAL INTERACTABLE CHECK (NPCs, Pip) -> E KEY ONLY
            // =============================================================
            if (isPressingE)
            {
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                    return;
                }
            }

            // =============================================================
            // HOUSE I & A PICKUPS -> MOUSE CLICK ONLY
            // =============================================================
            if (isLeftClick && carry != null)
            {
                if (!carry.IsHolding())
                {
                    LetterPickup letter = hit.collider.GetComponentInParent<LetterPickup>();
                    if (letter != null) { carry.PickUp(letter); return; }
                }
                else
                {
                    Pillar slot = hit.collider.GetComponentInParent<Pillar>();
                    if (slot != null) { slot.PlaceLetter(carry); return; }
                }
            }

            if (isLeftClick && pieceCarry != null)
            {
                if (!pieceCarry.IsHolding())
                {
                    BrokenPiece piece = hit.collider.GetComponentInParent<BrokenPiece>();
                    if (piece != null) { pieceCarry.PickUp(piece); return; }
                }
                else
                {
                    PieceSlot slot = hit.collider.GetComponentInParent<PieceSlot>();
                    if (slot != null) { slot.PlacePiece(pieceCarry); return; }
                }
            }
        }
    }
}