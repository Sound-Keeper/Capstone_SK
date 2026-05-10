using UnityEngine;
using UnityEngine.InputSystem;

public class PennyCilTrigger : MonoBehaviour
{
    public GameObject interactPrompt;
    public DialogueManager dialogueManager;

    bool playerInside = false;

    void Start()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    void Update()
    {
        if (!playerInside) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (interactPrompt != null)
                interactPrompt.SetActive(false);

            if (dialogueManager != null)
            {
                dialogueManager.StartDialogue(
                    "Penny Cil",
                    new string[]
                    {
                        "Hi Penn! Can you help me and my citizens?",
                        "I need you to spell the word correctly.",
                        "Are you ready?"
                    }
                );
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            if (interactPrompt != null)
                interactPrompt.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }
    }
}