using UnityEngine;

public class FloatingChar : MonoBehaviour
{
    public float speed = 2f;
    public float height = 0.2f;

    private Vector3 startLocalPosition;

    private void Start()
    {
        startLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        float newY = startLocalPosition.y + Mathf.Sin(Time.time * speed) * height;
        transform.localPosition = new Vector3(startLocalPosition.x, newY, startLocalPosition.z);
    }

    public void ResetFloatOrigin()
    {
        startLocalPosition = transform.localPosition;
    }
}