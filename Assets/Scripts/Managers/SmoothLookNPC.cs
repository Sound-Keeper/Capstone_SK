using UnityEngine;

public class SmoothNPCLook : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float turnSpeed = 3.5f;

    private GameObject[] players;
    private Quaternion initialRotation;

    // Timer to prevent searching for tags every single frame (saves performance)
    private float searchTimer = 0f;
    private float searchInterval = 0.5f;

    void Start()
    {
        initialRotation = transform.rotation;
        FindPlayers();
    }

    void Update()
    {
        // If we don't have players yet (or if a player was swapped), search every 0.5 seconds
        if (players == null || players.Length == 0)
        {
            searchTimer += Time.deltaTime;
            if (searchTimer >= searchInterval)
            {
                searchTimer = 0f;
                FindPlayers();
            }

            // If we STILL haven't found anyone, stop here and wait for the next frame
            if (players == null || players.Length == 0) return;
        }

        // Find the closest player character
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            // Extra safety: check if the player gameobject is active in the hierarchy
            if (player == null || !player.activeInHierarchy) continue;

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        // Check if the closest player is within talking/detection range
        if (closestPlayer != null && closestDistance <= detectionRadius)
        {
            Vector3 direction = closestPlayer.transform.position - transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
            }
        }
        else
        {
            // If players walk away (or haven't approached), smoothly turn back to initial direction
            transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, Time.deltaTime * turnSpeed);
        }
    }

    void FindPlayers()
    {
        players = GameObject.FindGameObjectsWithTag(playerTag);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}