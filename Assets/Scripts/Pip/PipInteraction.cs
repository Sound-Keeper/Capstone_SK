using NUnit.Framework.Constraints;
using UnityEngine;

public class PipInteraction : MonoBehaviour, IInteractable
{
    [Header("UI Setup")]
    public string pipName = "Pip";
    public Sprite pipPortrait;

    [Header("Interaction Prompt UI")]
    [SerializeField] private CanvasGroup promptCanvas;
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private float interactionDistance = 3f;

    [Header("References")]
    public Transform player; // Changed to Transform to mirror NpcInteraction's detection

    [Header("Finale Cutscene Target")]
    public Transform finaleFountainTarget;

    [Header("Inspector Editable Instructions")]
    public System.Collections.Generic.List<string> pipInstructionsLines = new System.Collections.Generic.List<string> {
        "We are here, Sound Keeper! The fountain sits at the absolute heart of Word Valley.",
        "All five Vowel Stones are glowing in your wand. But to break Miss Spell's curse completely, you must recite the Ancient Valley Sound Chant.",
        "Come talk to me again whenever you're ready."
    };

    private PipHint pipHintSystem;

    // Track exact ending progression states
    private bool hasSaidHouseUCompletionInMapTest = false;
    private bool arrivedAtFountainFinale = false;
    private bool hasTriggered = false; // Prevents the prompt from showing mid-dialogue
    private bool playerInRange = false;

    void Start()
    {
        pipHintSystem = FindFirstObjectByType<PipHint>();

        // Use the detection fallback logic instantly on start
        FindPlayerFallback();

        if (pipPortrait == null && DialogueManager.Instance != null)
        {
            pipPortrait = DialogueManager.Instance.pipIntroPortrait;
        }

        // Safety: If game is reloaded and Pip is already at the fountain destination, match states
        if (finaleFountainTarget != null && Vector3.Distance(transform.position, finaleFountainTarget.position) < 1f)
        {
            hasSaidHouseUCompletionInMapTest = true;
            arrivedAtFountainFinale = true;
        }

        // Initialize Canvas state
        if (promptCanvas != null)
        {
            promptCanvas.alpha = 0f;
            promptCanvas.interactable = false;
            promptCanvas.blocksRaycasts = false;
        }
    }

    void Update()
    {
        // Continuously runs the fallback function in case the player spawns or updates dynamically
        FindPlayerFallback();

        // Mirrored range detection logic from NpcInteraction
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            playerInRange = distance <= interactionDistance;
        }
        else
        {
            playerInRange = false;
        }

        // Handle prompt fade in/out based on player distance
        if (promptCanvas != null)
        {
            float targetAlpha = (playerInRange && !hasTriggered) ? 1f : 0f;
            promptCanvas.alpha = Mathf.MoveTowards(promptCanvas.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

            bool visible = promptCanvas.alpha > 0.001f;
            promptCanvas.interactable = visible;
            promptCanvas.blocksRaycasts = visible;
        }
    }

    // Exact detection method from your working NpcInteraction file
    void FindPlayerFallback()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    public void Interact()
    {
        if (DialogueManager.Instance == null || pipHintSystem == null) return;

        // Gated Safety: Don't allow this script to run inside any interior house scenes!
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MapTest")
        {
            return;
        }

        // Hide the prompt once interaction starts
        hasTriggered = true;

        // ============================================================
        // STEP 3: Pip is at the fountain AND has given instructions. START CHANT!
        // ============================================================
        if (arrivedAtFountainFinale && hasSaidHouseUCompletionInMapTest)
        {
            DialogueManager.Instance.SetPlayerControlState(false);
            if (EndGameCutscene.Instance != null)
            {
                EndGameCutscene.Instance.StartFountainRitual();
            }
            return;
        }

        PipHint.HintObjective nextObjective = pipHintSystem.GetActiveObjective();
        string[] lines;
        bool shouldTriggerFlight = false;
        bool headingToFountainFinale = false;

        if (!DialogueManager.HasPlayedPipIntroFinished)
        {
            lines = pipHintSystem.fountainIntroDialogue.ToArray();
            shouldTriggerFlight = true;
        }
        else if (nextObjective != null)
        {
            // Standard hint tracking loops before the finale
            float distanceToObjective = Vector3.Distance(transform.position, nextObjective.hoverLocation.position);

            if (distanceToObjective > 2.0f)
            {
                string completedHouseDialogue = "Fantastic work solving that puzzle!";

                for (int i = pipHintSystem.objectives.Count - 1; i >= 0; i--)
                {
                    var obj = pipHintSystem.objectives[i];
                    if (PuzzleProgress.IsHouseComplete(obj.houseLetter))
                    {
                        completedHouseDialogue = obj.completionDialogue;
                        break;
                    }
                }

                lines = new string[] {
                    completedHouseDialogue,
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
            // ALL HOUSES ARE COMPLETE!
            if (!hasSaidHouseUCompletionInMapTest)
            {
                // ============================================================
                // STEP 1: First interaction in MapTest. Say completion line & fly away.
                // ============================================================
                string finalHouseDialogue = "Sensational! House U is clear!";

                // Pull completion sentence string dynamically from your PipHint system configuration
                if (pipHintSystem.objectives != null && pipHintSystem.objectives.Count > 0)
                {
                    finalHouseDialogue = pipHintSystem.objectives[pipHintSystem.objectives.Count - 1].completionDialogue;
                }

                lines = new string[] {
                    finalHouseDialogue,
                    "I'll meet you over at the center fountain right away!"
                };

                shouldTriggerFlight = true;
                headingToFountainFinale = true;
                hasSaidHouseUCompletionInMapTest = true; // Flag checked so this text won't run again!
            }
            else
            {
                // ============================================================
                // STEP 2: Player walked over to the fountain. Give instructions!
                // ============================================================
                lines = pipInstructionsLines.ToArray();
                shouldTriggerFlight = false; // Already standing at fountain, no flying required!
                arrivedAtFountainFinale = true; // Ready for Step 3 interaction next time!
            }
        }

        // Set up dialogue callbacks to safely execute flight mechanics cleanly after UI box disappears
        // ============================================================
        // Set up dialogue callbacks to safely execute actions after UI box disappears
        // ============================================================
        // Set up dialogue callbacks to safely execute flight mechanics cleanly after UI box disappears
        if (shouldTriggerFlight)
        {
            DialogueManager.Instance.OnDialogueEnd = () => {
                ResetTriggerState();
                if (!DialogueManager.HasPlayedPipIntroFinished)
                {
                    DialogueManager.hasPlayedPipIntroFinished = true;
                    TriggerFlightSequence(nextObjective, false);
                }
                else
                {
                    TriggerFlightSequence(nextObjective, headingToFountainFinale);
                }
            };
        }
        else
        {
            DialogueManager.Instance.OnDialogueEnd = () => {
                ResetTriggerState();

                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.SetPlayerControlState(true);
                }
                else
                {
                    Charactercontroller activePlayer = FindFirstObjectByType<Charactercontroller>();
                    if (activePlayer != null) activePlayer.canControl = true;
                }
            };
        }

        // ============================================================
        // FIXED: Convert strings to DialogueLines and pass NULL for the camera
        // =// This forces DialogueManager to drop House U's camera reference!
        // ============================================================
        if (DialogueManager.Instance != null)
        {
            System.Collections.Generic.List<DialogueLine> structuredLines = new System.Collections.Generic.List<DialogueLine>();
            foreach (string textLine in lines)
            {
                DialogueLine newline = new DialogueLine();
                newline.text = textLine;
                // Matching your EndGameCutscene's structural design:
                newline.speaker = Speaker.NPC;
                structuredLines.Add(newline);
            }

            DialogueManager.Instance.StartDialogue(
                structuredLines.ToArray(),
                pipName,
                pipPortrait,
                null,                                  // No player portrait needed for Pip hints
                null,                                  // PASSING NULL CLEARS THE CACHED HOUSE U CAMERA!
                DialogueManager.Instance.OnDialogueEnd
            );
        }
    } // This closes your Interact() method completely

    private void ResetTriggerState()
    {
        hasTriggered = false;
    }

    private void TriggerFlightSequence(PipHint.HintObjective nextObjective, bool isFinale)
    {
        Transform destinationTarget = isFinale ? finaleFountainTarget : (nextObjective != null ? nextObjective.hoverLocation : null);

        if (destinationTarget == null) return;

        // Safely fetch script component context directly during execution frame only 
        Charactercontroller activePlayer = FindFirstObjectByType<Charactercontroller>();
        if (activePlayer != null) activePlayer.canControl = false;

        Camera mainCam = Camera.main;
        if (DialogueManager.Instance.pipCutsceneCamera != null)
        {
            if (mainCam != null) mainCam.gameObject.SetActive(false);
            DialogueManager.Instance.pipCutsceneCamera.gameObject.SetActive(true);
        }

        pipHintSystem.pip.MoveToTarget(destinationTarget, () => {
            if (DialogueManager.Instance.pipCutsceneCamera != null)
            {
                DialogueManager.Instance.pipCutsceneCamera.gameObject.SetActive(false);
                if (mainCam != null) mainCam.gameObject.SetActive(true);
            }

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