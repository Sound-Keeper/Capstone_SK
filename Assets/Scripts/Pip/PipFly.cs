using UnityEngine;

public class PipFly : MonoBehaviour
{
    public float moveSpeed = 5f;
    [Tooltip("How fast Pip snaps or smooths his rotation towards his path.")]
    public float rotationSpeed = 10f;

    [Header("Ground Settings")]
    [Tooltip("How high above the ground should Pip float base line?")]
    public float floatHeight = 1.5f;
    [Tooltip("Which layers represent the ground/terrain?")]
    public LayerMask groundLayer;

    [Header("Hover Animation (FloatingChar Formula)")]
    public float floatSpeed = 2f;
    public float floatAmplitude = 0.2f;

    private Vector3 startPos;
    private Transform currentTarget;
    private Quaternion homeRotation; // Tracks the idle rotation position target
    private System.Action onArriveCallback;

    void Start()
    {
        startPos = transform.position;
        homeRotation = transform.rotation; // Default to spawn rotation

        PipHint hint = FindFirstObjectByType<PipHint>();
        if (hint != null && hint.objectives != null && hint.objectives.Count > 0)
        {
            Transform spawnTarget = null;

            for (int i = hint.objectives.Count - 1; i >= 0; i--)
            {
                var obj = hint.objectives[i];
                if (PuzzleProgress.IsHouseComplete(obj.houseLetter))
                {
                    spawnTarget = obj.hoverLocation;
                    break;
                }
            }

            if (spawnTarget != null)
            {
                transform.position = spawnTarget.position;
                transform.rotation = spawnTarget.rotation; // Respect immediate spawn orientation
                homeRotation = spawnTarget.rotation;
                startPos = transform.position;
            }
        }
    }

    public void MoveToTarget(Transform target, System.Action onArrive = null)
    {
        currentTarget = target;
        if (target != null)
        {
            homeRotation = target.rotation; // Cache the destination's home rotation orientation
        }
        onArriveCallback = onArrive;
    }

    void Update()
    {
        Vector3 targetPos = transform.position;

        if (currentTarget != null)
        {
            // 1. FLYING ROTATION: Face the path direction
            Vector3 direction = currentTarget.position - transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }

            // 2. HORIZONTAL MOVEMENT
            targetPos = Vector3.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);

            // Check distance arrival parameters
            Vector2 horizontalCurrent = new Vector2(transform.position.x, transform.position.z);
            Vector2 horizontalTarget = new Vector2(currentTarget.position.x, currentTarget.position.z);

            if (Vector2.Distance(horizontalCurrent, horizontalTarget) < 0.2f)
            {
                startPos = currentTarget.position;
                currentTarget = null;
                onArriveCallback?.Invoke();
                onArriveCallback = null;
            }
        }
        else
        {
            // 3. IDLE ROTATION: Rotate back smoothly to look exactly where the target spot points
            transform.rotation = Quaternion.Slerp(transform.rotation, homeRotation, rotationSpeed * Time.deltaTime);
        }

        // 4. GROUND HEIGHT CALCULATION + SINE BOBBING
        Ray ray = new Ray(new Vector3(targetPos.x, targetPos.y + 5f, targetPos.z), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 20f, groundLayer))
        {
            float animatedBob = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            targetPos.y = hit.point.y + floatHeight + animatedBob;
        }
        else
        {
            targetPos.y += Mathf.Sin(Time.time * floatSpeed) * floatAmplitude * Time.deltaTime;
        }

        transform.position = targetPos;
    }
}