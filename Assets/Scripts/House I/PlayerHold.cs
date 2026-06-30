using UnityEngine;

public class PlayerHold : MonoBehaviour
{
    public Transform holdPoint;
    public float followSpeed = 10f;
    public LetterPickup held;

    [Header("Floor Collision Settings")]
    public LayerMask groundLayer = ~0;
    public float itemRadiusOffset = 0.4f;

    // --- PARTICLE ADDITIONS ---
    [Header("Visual Effects")]
    public ParticleSystem holdParticle;

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
        if (held.transform.parent != null)
        {
            held.transform.SetParent(null);
        }

        held.transform.localScale = held.startScale;

        Vector3 targetPosition = holdPoint.position;

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

        held.transform.position = Vector3.Lerp(held.transform.position, targetPosition, followSpeed * Time.deltaTime);
        held.transform.rotation = Quaternion.Slerp(held.transform.rotation, holdPoint.rotation, followSpeed * Time.deltaTime);
    }

    public bool IsHolding() { return held != null; }

    public void PickUp(LetterPickup letter)
    {
        if (held != null) return;

        held = letter;
        held.transform.SetParent(null);

        Collider col = letter.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // --- PARTICLE ADDITIONS ---
        // Turn the magic on!
        if (holdParticle != null)
        {
            holdParticle.Play();
        }
    }

    public void ClearHeld()
    {
        held = null;

        // --- PARTICLE ADDITIONS ---
        // Turn the magic off smoothly
        if (holdParticle != null)
        {
            holdParticle.Stop();
        }
    }
}