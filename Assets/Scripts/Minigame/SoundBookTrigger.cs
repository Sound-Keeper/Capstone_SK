using UnityEngine;

public class SoundBookTrigger : MonoBehaviour, IInteractable
{
    [Header("Target Configurations")]
    [Tooltip("Drag the vine_2 GameObject here from the stone stand hierarchy.")]
    public GameObject vineToDisable;

    [Tooltip("Drag the Minigame GameObject here from the DialogueSystem hierarchy.")]
    public GameObject minigameUI;

    [Header("Locked Dialogue Settings")]
    public string speakerName = "The Sound Book";

    [TextArea(2, 5)]
    public string[] lockedDialogueLines = new string[] {
        "The pages of the Sound Book remain bound tight by magical vines...",
        "You must discover all of the hidden clues around the map first!",
        "Come back once you have found them all to break the seal."
    };

    public static bool IsBookMinigameOpen { get; private set; } = false;
    private bool isGamePaused = false;

    // ─── NEW: Check every frame if the player broke the seal ───
    void Update()
    {
        if (vineToDisable != null && vineToDisable.activeSelf)
        {
            if (SignDiscoveryManager.Instance != null &&
                SignDiscoveryManager.Instance.discoveredSignsCount >= SignDiscoveryManager.Instance.totalSignsInMap)
            {
                // The moment the last sign is read, the vines vanish in the world!
                vineToDisable.SetActive(false);
                Debug.Log("[SoundBook] All signs found! Vines cleared from the world view.");
            }
        }
    }

    public void Interact()
    {
        if (isGamePaused) return;

        // Check if the sign milestone is met
        if (SignDiscoveryManager.Instance != null)
        {
            if (SignDiscoveryManager.Instance.discoveredSignsCount < SignDiscoveryManager.Instance.totalSignsInMap)
            {
                TriggerLockedDialogue();
                return;
            }
        }

        // MILESTONE MET: Open the puzzle book layout
        TriggerMinigameSetup();
    }

    private void TriggerLockedDialogue()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd = null;
            DialogueManager.Instance.StartDialogue(speakerName, lockedDialogueLines, null, null);
        }
    }

    private void TriggerMinigameSetup()
    {
        isGamePaused = true;
        IsBookMinigameOpen = true;

        // 1. Open the minigame UI
        if (minigameUI != null)
        {
            minigameUI.SetActive(true);
        }

        // 2. Freeze the player completely using your existing control flag
        Charactercontroller player = Object.FindAnyObjectByType<Charactercontroller>();
        if (player != null)
        {
            player.canControl = false;
        }

        // 3. Freeze world physics/animations
        Time.timeScale = 0f;

        // 4. Free the cursor for the UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGameplayWorld()
    {
        if (!isGamePaused) return;
        isGamePaused = false;
        IsBookMinigameOpen = false;

        Time.timeScale = 1f;

        if (minigameUI != null)
        {
            minigameUI.SetActive(false);
        }

        Charactercontroller player = Object.FindAnyObjectByType<Charactercontroller>();
        if (player != null)
        {
            player.canControl = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}