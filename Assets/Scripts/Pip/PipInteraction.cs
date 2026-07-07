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

        PipHint.HintObjective nextObjective = pipHintSystem.GetActiveObjective();
        string[] lines;
        bool shouldTriggerFlight = false;

        // --- FIXED CONDITION: Check if he hasn't delivered his fountain line yet ---
        if (!DialogueManager.HasPlayedPipIntroFinished)
        {
            // Pip is sitting at the fountain, waiting to deliver your custom inspector intro!
            lines = pipHintSystem.fountainIntroDialogue.ToArray();
            shouldTriggerFlight = true;
        }
        else if (nextObjective != null)
        {
            float distanceToObjective = Vector3.Distance(transform.position, nextObjective.hoverLocation.position);

            if (distanceToObjective > 2.0f)
            {
                lines = new string[] {
                    $"Fantastic work solving that puzzle!",
                    $"Let's head over to House {nextObjective.houseLetter} next!"
                };
                shouldTriggerFlight = true;
            }
            else
            {
                lines = nextObjective.dialogueHints.ToArray();
                shouldTriggerFlight = false;
            }
        }
        else
        {
            lines = new string[] { "Amazing work! Every single valley house is saved!" };
            shouldTriggerFlight = false;
        }

        if (shouldTriggerFlight)
        {
            DialogueManager.Instance.OnDialogueEnd = () => {

                // --- FIXED HERE: Mark the intro finished ONLY after this fountain conversation ends ---
                if (!DialogueManager.HasPlayedPipIntroFinished)
                {
                    DialogueManager.hasPlayedPipIntroFinished = true;
                    TriggerFlightSequence(nextObjective); // Flies to House A!
                }
                else
                {
                    TriggerFlightSequence(nextObjective);
                }
            };
        }
        else
        {
            DialogueManager.Instance.OnDialogueEnd = null;
        }

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

            // --- FIXED HERE ---
            // Instead of manually changing lock states here which resets the input system x-axis vector lookup, 
            // call your built-in dynamic control re-enabler inside the dialogue manager framework.
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.SetPlayerControlState(true);
            }
            else if (activePlayer != null)
            {
                activePlayer.canControl = true;
            }
        });
    }
}