using UnityEngine;

public class PieceHold : MonoBehaviour
{
    public Transform holdPoint;
    public float followSpeed = 10f;
    public BrokenPiece held;

    [Header("Floor Collision Settings")]
    public LayerMask Ground = ~0;
    public float itemRadiusOffset = 0.25f;

    [Header("Visual Effects")]
    public ParticleSystem holdParticle;

    // --- NEW PICKUP AUDIO SLOT ---
    [Header("Audio")]
    [Tooltip("Drag the sound effect that plays when picking up a broken piece.")]
    public AudioClip pickupSFX;

    private Camera playerCam;

    void Start()
    {
        playerCam = GetComponentInChildren<Camera>();
        if (playerCam == null) playerCam = Camera.main;
    }

    void Update()
    {
        if (held == null || holdPoint == null) return;

        // --- NEW: Sync the particle system to the hold point position while carrying an object ---
        if (holdParticle != null && holdParticle.isPlaying)
        {
            holdParticle.transform.position = holdPoint.position;
            holdParticle.transform.rotation = holdPoint.rotation;
        }

        Vector3 targetPosition = holdPoint.position;

        if (playerCam != null)
        {
            Vector3 rayStart = playerCam.transform.position;
            Vector3 rayDirection = targetPosition - rayStart;
            float rayLength = rayDirection.magnitude;

            if (Physics.Raycast(rayStart, rayDirection.normalized, out RaycastHit hit, rayLength, Ground))
            {
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

        if (holdParticle != null)
        {
            holdParticle.Play();
        }

        // --- PLAY PICKUP SFX ---
        if (pickupSFX != null)
        {
            CoreAudioManager.PlaySFX(pickupSFX);
        }
    }

    public void ClearHeld()
    {
        held = null;

        if (holdParticle != null)
        {
            holdParticle.Stop();
        }
    }
}