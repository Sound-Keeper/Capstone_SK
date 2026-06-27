using UnityEngine;

public class LetterPickup : MonoBehaviour
{
    public string letter;

    [HideInInspector] public Vector3 startPosition;
    [HideInInspector] public Quaternion startRotation;
    [HideInInspector] public Vector3 startScale;
    private Vector3 startLocalScale;

    // --- ADD THIS TO TRACK ITS HOME GROUP ---
    private Transform originalParent;

    void Start()
    {
        // Remember the original parent group (e.g., House I) so it can get squished correctly again
        originalParent = transform.parent;

        startPosition = transform.position;
        startRotation = transform.rotation;

        startScale = transform.lossyScale;
        startLocalScale = transform.localScale;
    }

    public void ReturnToStart()
    {
        // Put it back inside its original parent group first
        transform.SetParent(originalParent);

        // Now restore its original local positions and scales relative to that group
        transform.localPosition = originalParent != null ? originalParent.InverseTransformPoint(startPosition) : startPosition;
        transform.localRotation = originalParent != null ? Quaternion.Inverse(originalParent.rotation) * startRotation : startRotation;
        transform.localScale = startLocalScale;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = true;
    }
}