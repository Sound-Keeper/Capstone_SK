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

        PipHint.HintObjective activeObjective = pipHintSystem.GetActiveObjective();
        string[] lines;

        // 1. Determine what Pip says
        if (!DialogueManager.HasPlayedPipIntroFinished)
        {
            // Pip is waiting at the fountain for the first time
            lines = pipHintSystem.fountainIntroDialogue.ToArray();
        }
        else if (activeObjective != null)
        {
            // Pip is at a house. Use the hint dialogue list
            lines = activeObjective.dialogueHints.ToArray();
        }
        else
        {
            // All houses complete!
            lines = new string[] { "Amazing work! Every single valley house is saved!" };
        }

        // 2. Start the dialogue
        DialogueManager.Instance.StartDialogue(pipName, lines, pipPortrait);

        // 3. Setup the Cutscene Trigger
        DialogueManager.Instance.OnDialogueEnd = () => {

            // If this was the intro, mark it as finished
            if (!DialogueManager.HasPlayedPipIntroFinished)
                DialogueManager.hasPlayedPipIntroFinished = true;

            // --- CHECK FOR COMPLETION ---
            // If we are at a house but the puzzle is already solved, 
            // trigger flight to the NEXT objective.
            if (activeObjective != null && PuzzleProgress.IsHouseComplete(activeObjective.houseLetter))
            {
                TriggerFlightSequence(pipHintSystem.GetActiveObjective());
            }
        };
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