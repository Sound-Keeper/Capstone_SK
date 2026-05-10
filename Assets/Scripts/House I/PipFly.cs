using UnityEngine;

public class PipFly : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Transform target;
    private bool isMoving = false;

    void Update()
    {
        if (!isMoving || target == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        // Look at target while moving
        Vector3 direction = (target.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Stop when close enough
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            isMoving = false;
        }
    }

    public void MoveToTarget(Transform newTarget)
    {
        target = newTarget;
        isMoving = true;
    }
}
