using UnityEngine;

public class PieceHold : MonoBehaviour
{
    public Transform holdPoint;
    public float followSpeed = 10f;
    public BrokenPiece held;

    [Header("Floor Collision Settings")]
    public LayerMask Ground = ~0;
    public float itemRadiusOffset = 0.25f;

    private Camera playerCam;

    void Start()
    {
        playerCam = GetComponentInChildren<Camera>();
        if (playerCam == null) playerCam = Camera.main;
    }

    void Update()
    {
        if (held == null || holdPoint == null) return;

        Vector3 targetPosition = holdPoint.position;

        // Prevent clipping by raycasting from camera to hold point
        if (playerCam != null)
        {
            Vector3 rayStart = playerCam.transform.position;
            Vector3 rayDirection = targetPosition - rayStart;
            float rayLength = rayDirection.magnitude;

            if (Physics.Raycast(rayStart, rayDirection.normalized, out RaycastHit hit, rayLength, Ground))
            {
                // Push position out slightly away from the floor surface
                targetPosition = hit.point + (hit.normal * itemRadiusOffset);
            }
        }

        held.transform.position = Vector3.Lerp(
            held.transform.position,
            targetPosition,
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
        if (piece == null || piece.IsBusy) return;

        held = piece;

        Collider col = piece.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    public void ClearHeld() { held = null; }
}