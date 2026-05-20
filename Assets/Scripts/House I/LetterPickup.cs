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
        startScale = transform.lossyScale; 
        // i change from scale to lossyscale since if the puzzle is wrong nag iisqueeze yung box or asset just wanna test this
    }
    public void ReturnToStart()
    {
        transform.SetParent(null);
        transform.localScale = startScale;

        transform.position = startPosition;
        transform.rotation = startRotation;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = true;
    }
}