using System;
using UnityEngine;

public class PipFly : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Pip Trail (fly beside the player)")]
    [Tooltip("Offset from the player: x = side, y = height, z = front/back.")]
    public Vector3 followOffset = new Vector3(1.2f, 1.8f, 0f);
    [Tooltip("Bigger = snappier follow.")]
    public float followSmooth = 5f;

    private Transform target;
    private bool isMoving = false;
    private Action onArrive;

    private Transform followPlayer;
    private bool isFollowing = false;

    void Update()
    {
        if (isFollowing && followPlayer != null)
        {
            FollowUpdate();
            return;
        }

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

        // Stop when close enough, then run the arrival callback once
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            isMoving = false;
            Action cb = onArrive;
            onArrive = null;
            cb?.Invoke();
        }
    }

    // fly to a point; onArrive runs once when Pip gets there (used to chain the fountain dialogue)
    public void MoveToTarget(Transform newTarget, Action onArrive = null)
    {
        target = newTarget;
        this.onArrive = onArrive;
        isMoving = true;
        isFollowing = false;
    }

    // pip trail - start flying alongside the player using followOffset (relative to player facing)
    public void FollowPlayerStart(Transform player)
    {
        followPlayer = player;
        isFollowing = true;
        isMoving = false;
    }

    public void FollowStop()
    {
        isFollowing = false;
    }

    void FollowUpdate()
    {
        Vector3 desired = followPlayer.position
            + followPlayer.right   * followOffset.x
            + followPlayer.up      * followOffset.y
            + followPlayer.forward * followOffset.z;

        transform.position = Vector3.Lerp(transform.position, desired, followSmooth * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, followPlayer.rotation, followSmooth * Time.deltaTime);
    }
}
