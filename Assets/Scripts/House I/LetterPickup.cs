using UnityEngine;

public class LetterPickup : MonoBehaviour
{
    public string letter;

    [HideInInspector] public Vector3 startPosition;
    [HideInInspector] public Quaternion startRotation;
    [HideInInspector] public Vector3 startScale;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale = transform.localScale;
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
