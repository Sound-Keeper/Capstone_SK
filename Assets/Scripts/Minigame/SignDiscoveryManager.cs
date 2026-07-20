using UnityEngine;

public class SignDiscoveryManager : MonoBehaviour
{
    public static SignDiscoveryManager Instance { get; private set; }

    [Header("Target Configurations")]
    [Tooltip("Drag the vine_2 GameObject here so it can be automatically removed when all signs are found.")]
    public GameObject vineToDisable;

    [Header("Tracking Status")]
    public int totalSignsInMap = 0;

    // Expose the static count to the inspector or other scripts
    public int discoveredSignsCount => PuzzleProgress.DiscoveredSignsCount;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Find how many signs are actually present in this scene layout
        InteractableSign[] allSigns = FindObjectsByType<InteractableSign>(FindObjectsSortMode.None);
        totalSignsInMap = allSigns.Length;
    }

    void Start()
    {
        // Check on startup if they were already all found previously (e.g., returning from House A scene)
        CheckAndRemoveVines();
    }

    public void ReportSignDiscovered(string signID)
    {
        // Save directly into our global, scene-surviving static list!
        if (!PuzzleProgress.DiscoveredSignIDs.Contains(signID))
        {
            PuzzleProgress.DiscoveredSignIDs.Add(signID);

            Debug.Log($"[Sign Manager] Discovered sign: {signID}! Total collected: ({discoveredSignsCount}/{totalSignsInMap})");

            CheckAndRemoveVines();
        }
    }

    private void CheckAndRemoveVines()
    {
        if (discoveredSignsCount >= totalSignsInMap && totalSignsInMap > 0)
        {
            Debug.Log("[Sign Manager] Milestone Reached: Every single sign found! Clearing vines.");
            if (vineToDisable != null)
            {
                vineToDisable.SetActive(false);
            }
        }
    }
}