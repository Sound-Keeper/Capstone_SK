using UnityEngine;

public class BrokenPiece : MonoBehaviour
{
    public string pieceID;

    Vector3 startPosition;
    Quaternion startRotation;
    Vector3 startScale;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale = transform.localScale;
    }

    public Vector3 GetStartScale()
    {
        return startScale;
    }

    public void ReturnToStart()
    {
        transform.SetParent(null);
        transform.position = startPosition;
        transform.rotation = startRotation;
        transform.localScale = startScale;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = true;
    }
}