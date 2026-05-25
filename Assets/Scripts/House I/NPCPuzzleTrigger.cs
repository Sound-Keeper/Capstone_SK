using UnityEngine;
using UnityEngine.InputSystem;

public class NPCPuzzleTrigger : MonoBehaviour
{
    [Header("NPC Info")]
    public string npcName = "Penny Cil";

    

    [Header("References")]
    public DialogueManager dialogueManager;
    public PuzzleManager puzzleManager;

    [Header("UI")]
    public GameObject interactPrompt;

    [Header("Settings")]
    public bool oneTimeOnly = true;

    private bool playerInside = false;
    private bool hasTriggered = false;

    void Start()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    void Update()
    {
        if (!playerInside) return;
        if (oneTimeOnly && hasTriggered) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TriggerDialogue();
        }
    }

    void TriggerDialogue()
    {
        hasTriggered = true;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        if (dialogueManager != null && puzzleManager != null)
        {
            // Set the callback: when dialogue ends, activate THIS NPC's puzzle
            dialogueManager.OnDialogueEnd = () =>
            {
                puzzleManager.ActivatePuzzle();
            };

            // Start the dialogue
            dialogueManager.StartDialogue(npcName, dialogueLines);
        }
        else
        {
            // Fallback: just activate the puzzle directly if no dialogue manager
            if (puzzleManager != null)
                puzzleManager.ActivatePuzzle();

            Debug.LogWarning($"NPCPuzzleTrigger on {gameObject.name}: Missing DialogueManager or PuzzleManager reference!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneTimeOnly && hasTriggered) return;

        playerInside = true;

        if (interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }
}
