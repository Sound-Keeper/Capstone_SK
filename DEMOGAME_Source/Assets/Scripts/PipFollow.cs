using UnityEngine;

public class PipFollow : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 3f;
    public Vector3 offset = new Vector3(1.5f, 1.5f, -1.5f);

    private bool followPlayer = false;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (!followPlayer || player == null) return;

        Vector3 targetPosition = player.position + player.TransformDirection(offset);

        float bob = Mathf.Sin(Time.time * 2f) * 0.15f;
        targetPosition.y += bob;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.unscaledDeltaTime
        );
    }

    public void StartFollowing()
    {
        followPlayer = true;
    }

    public void StopFollowing()
    {
        followPlayer = false;
    }
}