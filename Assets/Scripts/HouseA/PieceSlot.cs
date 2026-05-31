using UnityEngine;

// One socket on the broken object (e.g. a spot on the vase). It only accepts the
// piece whose pieceID matches expectedPieceID. Correct -> snap + lock + tell the
// PuzzleManagerA. Wrong -> the piece vibrates and slowly floats back to its start.
public class PieceSlot : MonoBehaviour
{
    public string expectedPieceID;
    [Tooltip("Where the piece sits when placed. Leave empty to use this object's transform.")]
    public Transform slotPoint;
    public bool isFilled = false;

    [Header("Puzzle")]
    [Tooltip("The House A puzzle manager. Notified when a correct piece is placed.")]
    public PuzzleManagerA puzzle;

    void Awake()
    {
        if (slotPoint == null) slotPoint = transform;
    }

    // Called by WandPickUpRay when the player clicks this slot while carrying a piece.
    public void PlacePiece(PieceHold hold)
    {
        if (hold == null || hold.held == null) return;
        if (isFilled) return;

        BrokenPiece piece = hold.held;
        if (piece.IsBusy) return;

        if (piece.pieceID == expectedPieceID)
        {
            hold.ClearHeld();

            // SetParent(..., true) keeps the piece's world size so a scaled slot
            // doesn't squeeze/stretch it; then snap to the slot pose.
            piece.transform.SetParent(slotPoint, true);
            piece.transform.position = slotPoint.position;
            piece.transform.rotation = slotPoint.rotation;

            Collider col = piece.GetComponent<Collider>();
            if (col != null) col.enabled = false; // locked in place

            isFilled = true;
            Debug.Log("Correct piece placed: " + expectedPieceID);

            if (puzzle != null) puzzle.PiecePlaced();
        }
        else
        {
            Debug.Log("Wrong piece for slot: " + expectedPieceID);
            hold.ClearHeld();
            piece.VibrateAndReturn();
        }
    }
}
