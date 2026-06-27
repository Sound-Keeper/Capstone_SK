using UnityEngine;

public class PlayerHold : MonoBehaviour
{
    public Transform holdPoint;
    public float followSpeed = 10f;
    public LetterPickup held;

    [Header("Floor Collision Settings")]
    public LayerMask groundLayer = ~0;
    public float itemRadiusOffset = 0.4f;

    private Camera playerCam;

    void Start()
    {
        playerCam = GetComponentInChildren<Camera>();
        if (playerCam == null) playerCam = Camera.main;
    }

    void Update()
    {
        if (held == null || holdPoint == null) return;

        // --- FIXED PARENT SCALE BREAK ---
        // Strip the parent so it doesn't skew when looking around
        if (held.transform.parent != null)
        {
            held.transform.SetParent(null);
        }

        // Force it to use its true, unwarped world scale size
        held.transform.localScale = held.startScale;

        Vector3 targetPosition = holdPoint.position;

        // Floor collision check
        if (playerCam != null)
        {
            Vector3 rayStart = playerCam.transform.position;
            Vector3 rayDirection = targetPosition - rayStart;
            float rayLength = rayDirection.magnitude;

            if (Physics.Raycast(rayStart, rayDirection.normalized, out RaycastHit hit, rayLength, groundLayer))
            {
                targetPosition = hit.point + (hit.normal * itemRadiusOffset);
            }
        }

        // Smoothly follow position and camera tilt rotation
        held.transform.position = Vector3.Lerp(held.transform.position, targetPosition, followSpeed * Time.deltaTime);
        held.transform.rotation = Quaternion.Slerp(held.transform.rotation, holdPoint.rotation, followSpeed * Time.deltaTime);
    }

    public bool IsHolding() { return held != null; }

    public void PickUp(LetterPickup letter)
    {
        if (held != null) return;

        held = letter;
        held.transform.SetParent(null); // Detach immediately from the squished House I group

        Collider col = letter.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }

    public void ClearHeld()
    {
        held = null;
    }
}