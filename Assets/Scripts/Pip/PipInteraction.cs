using UnityEngine;

public class PipInteraction : MonoBehaviour, IInteractable
{
    [Header("UI Setup")]
    public string pipName = "Pip";
    public Sprite pipPortrait;

    private PipHint pipHintSystem;

    void Start()
    {
        pipHintSystem = FindFirstObjectByType<PipHint>();
    }

    public void Interact()
    {
        if (DialogueManager.Instance == null || pipHintSystem == null) return;

        // 1. Get the current active uncompleted house (e.g., if A is done, this returns E)
        PipHint.HintObjective nextObjective = pipHintSystem.GetActiveObjective();
        string[] lines;
        bool shouldTriggerFlight = false;

        if (!DialogueManager.HasPlayedPipIntroFinished)
        {
            // Pip is waiting at the fountain for the first time
            lines = pipHintSystem.fountainIntroDialogue.ToArray();
            shouldTriggerFlight = true;
        }
        else if (nextObjective != null)
        {
            // Check if Pip has already physically flown to this house's hover location
            float distanceToObjective = Vector3.Distance(transform.position, nextObjective.hoverLocation.position);

            if (distanceToObjective > 2.0f)
            {
                // Pip is still physically standing at the old house, meaning a puzzle was JUST completed!
                lines = new string[] {
                    $"Fantastic work solving that puzzle!",
                    $"Let's head over to House {nextObjective.houseLetter} next!"
                };
                shouldTriggerFlight = true;
            }
            else
            {
                // Pip is already at the house waiting for you to solve it. Just show hints!
                lines = nextObjective.dialogueHints.ToArray();
                shouldTriggerFlight = false;
            }
        }
        else
        {
            // All houses complete!
            lines = new string[] { "Amazing work! Every single valley house is saved!" };
            shouldTriggerFlight = false;
        }

        // --- PUT THE FIX HERE: Setup the Cutscene Trigger BEFORE starting the dialogue ---
        if (shouldTriggerFlight)
        {
            DialogueManager.Instance.OnDialogueEnd = () => {

                if (!DialogueManager.HasPlayedPipIntroFinished)
                {
                    DialogueManager.hasPlayedPipIntroFinished = true;
                    TriggerFlightSequence(nextObjective);
                }
                else
                {
                    TriggerFlightSequence(nextObjective);
                }
            };
        }
        else
        {
            // If he's just giving hints, clear any leftover callbacks so the player unlocks normally
            DialogueManager.Instance.OnDialogueEnd = null;
        }

        // --- PUT THE FIX HERE: Start the dialogue AFTER the callback layout is secured ---
        DialogueManager.Instance.StartDialogue(pipName, lines, pipPortrait);
    }

    private void TriggerFlightSequence(PipHint.HintObjective nextObjective)
    {
        if (nextObjective == null || nextObjective.hoverLocation == null) return;

        // Lock Player
        Charactercontroller activePlayer = FindFirstObjectByType<Charactercontroller>();
        if (activePlayer != null) activePlayer.canControl = false;

        // Toggle Cameras
        Camera mainCam = Camera.main;
        if (DialogueManager.Instance.pipCutsceneCamera != null)
        {
            if (mainCam != null) mainCam.gameObject.SetActive(false);
            DialogueManager.Instance.pipCutsceneCamera.gameObject.SetActive(true);
        }

        // Fly!
        pipHintSystem.pip.MoveToTarget(nextObjective.hoverLocation, () => {

            // Return Cameras
            if (DialogueManager.Instance.pipCutsceneCamera != null)
            {
                DialogueManager.Instance.pipCutsceneCamera.gameObject.SetActive(false);
                if (mainCam != null) mainCam.gameObject.SetActive(true);
            }

            // Unlock Player
            if (activePlayer != null) activePlayer.canControl = true;

            // Lock Mouse
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        });
    }
}