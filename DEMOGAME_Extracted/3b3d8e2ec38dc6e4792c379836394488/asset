using UnityEngine;

public class PlayerHold : MonoBehaviour
{
    public Transform holdPoint;
    public float followSpeed = 10f;
    public LetterG held;

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

    public bool IsHolding()
    {
        return held != null;
    }

    public void PickUp(LetterG box)
    {
        if (held != null) return;

        held = box;

        Collider col = box.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }

    public void ClearHeld()
    {
        held = null;
    }
}