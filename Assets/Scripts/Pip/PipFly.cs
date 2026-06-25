using UnityEngine;

public class PipFly : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 startPos;
    private Transform currentTarget;
    private System.Action onArriveCallback;

    void Start()
    {
        startPos = transform.position;
        bool anyHouseSolved = PuzzleProgress.HouseASolved || PuzzleProgress.HouseESolved ||
                              PuzzleProgress.HouseISolved || PuzzleProgress.HouseOSolved ||
                              PuzzleProgress.HouseUSolved;

        if (anyHouseSolved)
        {
            PipHint hint = FindFirstObjectByType<PipHint>();
            var obj = hint?.GetActiveObjective();
            if (obj != null) { transform.position = obj.hoverLocation.position; startPos = transform.position; }
        }
    }

    public void MoveToTarget(Transform target, System.Action onArrive = null)
    {
        currentTarget = target;
        onArriveCallback = onArrive;
    }

    void Update()
    {
        if (currentTarget != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, currentTarget.position) < 0.1f)
            {
                startPos = currentTarget.position;
                currentTarget = null;
                onArriveCallback?.Invoke();
                onArriveCallback = null;
            }
        }
    }
}