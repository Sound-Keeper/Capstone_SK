using System.Collections;
using UnityEngine;

// A single broken shard/piece (e.g. a vase fragment).
// Carried by the wand via PieceHold. If placed in the WRONG slot it vibrates
// in place, then slowly floats back to where it started.
public class BrokenPiece : MonoBehaviour
{
    public string pieceID;

    [Header("Wrong-slot feedback")]
    [Tooltip("How long the piece shakes before it floats home.")]
    public float shakeDuration = 0.4f;
    [Tooltip("How far it jitters while shaking (world units).")]
    public float shakeStrength = 0.15f;
    [Tooltip("How long the slow float back to start takes.")]
    public float returnDuration = 0.8f;

    Vector3 startPosition;
    Quaternion startRotation;
    Vector3 startScale;
    Collider col;
    bool isBusy = false;

    void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale = transform.localScale;
        col = GetComponent<Collider>();
    }

    public Vector3 GetStartScale() { return startScale; }

    // true while it is animating home, so it can't be grabbed/placed mid-return
    public bool IsBusy { get { return isBusy; } }

    // instant reset (kept for safety / other callers)
    public void ReturnToStart()
    {
        StopAllCoroutines();
        isBusy = false;
        transform.SetParent(null);
        transform.position = startPosition;
        transform.rotation = startRotation;
        transform.localScale = startScale;
        if (col != null) col.enabled = true;
    }

    // wrong slot: vibrate, then slowly float back to where it started
    public void VibrateAndReturn()
    {
        StopAllCoroutines();
        StartCoroutine(VibrateAndReturnRoutine());
    }

    IEnumerator VibrateAndReturnRoutine()
    {
        isBusy = true;
        transform.SetParent(null);
        if (col != null) col.enabled = false; // can't re-grab while it's flying home

        // 1) shake in place
        Vector3 basePos = transform.position;
        float t = 0f;
        while (t < shakeDuration)
        {
            t += Time.deltaTime;
            Vector2 r = Random.insideUnitCircle * shakeStrength;
            transform.position = basePos + new Vector3(r.x, r.y, 0f);
            yield return null;
        }

        // 2) slowly float back to start
        Vector3 fromPos = basePos;
        Quaternion fromRot = transform.rotation;
        Vector3 fromScale = transform.localScale;
        t = 0f;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / returnDuration);
            transform.position = Vector3.Lerp(fromPos, startPosition, k);
            transform.rotation = Quaternion.Slerp(fromRot, startRotation, k);
            transform.localScale = Vector3.Lerp(fromScale, startScale, k);
            yield return null;
        }

        transform.position = startPosition;
        transform.rotation = startRotation;
        transform.localScale = startScale;
        if (col != null) col.enabled = true;
        isBusy = false;
    }
}
