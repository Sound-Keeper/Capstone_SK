using UnityEngine;

// Goes on the Player (alongside PlayerHold). The wand drops a BrokenPiece here and
// it floats to holdPoint - same feel as carrying a letter in House I.
public class PieceHold : MonoBehaviour
{
    public Transform holdPoint;
    public float followSpeed = 10f;
    public BrokenPiece held;

    void Update()
    {
        if (held == null) return;

        held.transform.position = Vector3.Lerp(
            held.transform.position,
            holdPoint.position,
            followSpeed * Time.deltaTime
        );

        held.transform.rotation = Quaternion.Slerp(
            held.transform.rotation,
            holdPoint.rotation,
            followSpeed * Time.deltaTime
        );
    }

    public bool IsHolding() { return held != null; }

    public void PickUp(BrokenPiece piece)
    {
        if (held != null) return;
        if (piece == null || piece.IsBusy) return; // ignore a piece that's flying home

        held = piece;

        Collider col = piece.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    public void ClearHeld() { held = null; }
}
