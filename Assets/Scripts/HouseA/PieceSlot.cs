using UnityEngine;

public class PieceSlot : MonoBehaviour
{
    public Transform slotPoint;
    public string expectedPieceID;
    public bool isFilled = false;

    public BrokenPiece held; // holds the carried piece

    public bool IsHolding() { return held != null; }

    public void ClearHeld() { held = null; }

    public void PickUp(BrokenPiece piece)
    {
        if (held != null) return;
        held = piece;
        Collider col = piece.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }

    public void PlacePiece(PieceSlot hold)
    {
        if (hold == null || hold.held == null) return;
        if (isFilled) return;

        BrokenPiece piece = hold.held;

        if (piece.pieceID == expectedPieceID)
        {
            piece.transform.SetParent(null);
            piece.transform.position = slotPoint.position;
            piece.transform.rotation = slotPoint.rotation;
            piece.transform.localScale = piece.GetStartScale();
            piece.transform.SetParent(slotPoint);

            Collider col = piece.GetComponent<Collider>();
            if (col != null)
                col.enabled = false;

            hold.ClearHeld();
            isFilled = true;

            Debug.Log("Correct piece placed: " + expectedPieceID);
        }
        else
        {
            Debug.Log("Wrong piece!");
            hold.ClearHeld();
            piece.ReturnToStart();
        }
    }
}