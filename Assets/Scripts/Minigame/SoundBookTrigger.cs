using UnityEngine;

public class SoundBookTrigger : MonoBehaviour, IInteractable
{
    [Header("Target Configurations")]
    public GameObject vineToDisable;
    public GameObject minigameUI;

    [Header("Achievement Reward UI")]
    [Tooltip("Drag the minigame completion achievement badge/icon from your UI canvas here.")]
    public GameObject completedAchievementBadge;

    [Header("Locked Dialogue Settings")]
    public string speakerName = "The Sound Book";

    [TextArea(2, 5)]
    public string[] lockedDialogueLines = new string[] {
        "The pages of the Sound Book remain bound tight by magical vines...",
        "You must discover all of the hidden clues around the map first!",
        "Come back once you have found them all to break the seal."
    };

    [TextArea(2, 5)]
    public string[] completedBookDialogueLines = new string[] {
        "You have already completed all the quizzes in the Sound Book!",
        "The knowledge inside is fully unlocked."
    };

    public static bool IsBookMinigameOpen { get; private set; } = false;
    private bool isGamePaused = false;

    void Start()
    {
        // ─── UI ACTIVATOR ON SCENE LOAD ───
        // Whenever the scene loads, sync the badge's visibility with static progress!
        if (completedAchievementBadge != null)
        {
            completedAchievementBadge.SetActive(PuzzleProgress.IsSoundBookCompleted);
        }
    }

    void Update()
    {
        if (vineToDisable != null && vineToDisable.activeSelf)
        {
            if (SignDiscoveryManager.Instance != null &&
                SignDiscoveryManager.Instance.discoveredSignsCount >= SignDiscoveryManager.Instance.totalSignsInMap)
            {
                vineToDisable.SetActive(false);
                Debug.Log("[SoundBook] All signs found! Vines cleared from the world view.");
            }
        }
    }

    public void Interact()
    {
        if (isGamePaused) return;

        // 1. BLOCK RE-ENTRY: Check if completed via static flag or cleared count
        if (PuzzleProgress.IsSoundBookCompleted || BookPageQuizManager.totalClearedPages >= BookPageQuizManager.totalPagesInBook)
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(speakerName, completedBookDialogueLines);
            }
            return;
        }

        // 2. Check if the sign milestone is met
        if (SignDiscoveryManager.Instance != null)
        {
            if (SignDiscoveryManager.Instance.discoveredSignsCount < SignDiscoveryManager.Instance.totalSignsInMap)
            {
                TriggerLockedDialogue();
                return;
            }
        }

        // 3. Open minigame UI
        TriggerMinigameSetup();
    }

    // Called automatically by BookPageQuizManager when all pages are complete
    public void OnAllPagesCompleted()
    {
        // 1. Save static flag so it persists across scene loads
        PuzzleProgress.IsSoundBookCompleted = true;

        // 2. Turn on the badge image immediately
        if (completedAchievementBadge != null)
        {
            completedAchievementBadge.SetActive(true);
        }

        Debug.Log("[SoundBook] All pages completed! Static flag saved & Achievement Badge activated.");
    }

    private void TriggerLockedDialogue()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd = null;
            DialogueManager.Instance.StartDialogue(speakerName, lockedDialogueLines);
        }
    }

    private void TriggerMinigameSetup()
    {
        isGamePaused = true;
        IsBookMinigameOpen = true;

        if (minigameUI != null)
        {
            minigameUI.SetActive(true);
        }

        Charactercontroller player = Object.FindAnyObjectByType<Charactercontroller>();
        if (player != null)
        {
            player.canControl = false;
        }

        Time.timeScale = 0f;
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