using UnityEngine;

public class PipFly : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 3f;
    public float stopDistance = 0.2f;

    private bool moveToTarget = false;
    private bool followPlayer = false;

    public Transform player;
    public Vector3 followOffset = new Vector3(1.5f, 1.5f, -1.5f);

    void Update()
    {
        if (moveToTarget && target != null)
        {
            Vector3 targetPos = target.position;
            targetPos.y += Mathf.Sin(Time.time * 2f) * 0.15f;

            transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) <= stopDistance)
            {
                moveToTarget = false;
            }
        }
        else if (followPlayer && player != null)
        {
            Vector3 targetPos = player.position + player.TransformDirection(followOffset);
            targetPos.y += Mathf.Sin(Time.time * 2f) * 0.15f;

            transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
    }

    public void MoveToTarget(Transform newTarget)
    {
        target = newTarget;
        moveToTarget = true;
        followPlayer = false;
    }

    public void StartFollowing()
    {
        followPlayer = true;
        moveToTarget = false;
    }
}