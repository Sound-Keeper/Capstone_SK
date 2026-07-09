using UnityEngine;

public class PipFly : MonoBehaviour
{
    [SerializeField] private GameObject minimapUI;
    private bool hasSnappedToFountain = false;
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

    // --- INTEGRATED SMOOTH LOOK SETTINGS ---
    [Header("Smooth Player Look (Only When Stationary)")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float turnSpeed = 3.5f;

    private GameObject[] players;
    private float searchTimer = 0f;
    private float searchInterval = 0.5f;
    // ----------------------------------------

    private Vector3 startPos;
    private Transform currentTarget;
    private Quaternion homeRotation; // Tracks the idle rotation position target
    private System.Action onArriveCallback;

    void Start()
    {
        startPos = transform.position;
        homeRotation = transform.rotation; // Default to spawn rotation

        FindPlayers(); // Find players initially

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
            if (minimapUI != null)
            {
                minimapUI.SetActive(false);
            }
        }
        onArriveCallback = onArrive;
    }

    void Update()
    {
        // --- UPDATED CONDITIONS TO PREVENT SNAPPING IF A PUZZLE IS ALREADY SOLVED ---
        bool zeroHousesSolved = !(PuzzleProgress.HouseASolved || PuzzleProgress.HouseESolved ||
                                  PuzzleProgress.HouseISolved || PuzzleProgress.HouseOSolved ||
                                  PuzzleProgress.HouseUSolved);

        if (!hasSnappedToFountain && DialogueManager.hasPlayedPipIntroFinished && zeroHousesSolved)
        {
            DialogueManager dialogueMgr = FindFirstObjectByType<DialogueManager>();
            if (dialogueMgr != null && dialogueMgr.fountainTarget != null)
            {
                // Snap Pip directly to the fountain!
                transform.position = dialogueMgr.fountainTarget.position;
                transform.rotation = dialogueMgr.fountainTarget.rotation;
                homeRotation = dialogueMgr.fountainTarget.rotation;
                startPos = transform.position;

                hasSnappedToFountain = true; // Ensure this only happens once!
            }
        }

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

                if (minimapUI != null)
                {
                    minimapUI.SetActive(true);
                }

                onArriveCallback?.Invoke();
                onArriveCallback = null;
            }
        }
        else
        {
            // 3. IDLE LOOK: Smoothly look at closest player if stationary
            HandleStationaryLook();
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

    private void HandleStationaryLook()
    {
        // Periodic search for players if array is empty or missing
        if (players == null || players.Length == 0)
        {
            searchTimer += Time.deltaTime;
            if (searchTimer >= searchInterval)
            {
                searchTimer = 0f;
                FindPlayers();
            }

            if (players == null || players.Length == 0)
            {
                // Fallback to home spot direction if absolutely no player is found
                transform.rotation = Quaternion.Slerp(transform.rotation, homeRotation, rotationSpeed * Time.deltaTime);
                return;
            }
        }

        // Find the closest player
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject playerObj in players)
        {
            if (playerObj == null || !playerObj.activeInHierarchy) continue; 

            float distance = Vector3.Distance(transform.position, playerObj.transform.position); 
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = playerObj;
            }
        }

        // Track player if they are inside the specified look distance threshold
        if (closestPlayer != null && closestDistance <= detectionRadius)
        {
            Vector3 lookDir = closestPlayer.transform.position - transform.position; 
            lookDir.y = 0; // Lock rotation axis

            if (lookDir != Vector3.zero) 
            {
                Quaternion targetLook = Quaternion.LookRotation(lookDir); 
                transform.rotation = Quaternion.Slerp(transform.rotation, targetLook, Time.deltaTime * turnSpeed); 
            }
        }
        else
        {
            // If the player walks away, smoothly turn back to your target slot's forward alignment direction[cite: 4, 13]
            transform.rotation = Quaternion.Slerp(transform.rotation, homeRotation, Time.deltaTime * turnSpeed);
        }
    }

    private void FindPlayers()
    {
        players = GameObject.FindGameObjectsWithTag(playerTag); 
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; 
        Gizmos.DrawWireSphere(transform.position, detectionRadius); 
    }
}