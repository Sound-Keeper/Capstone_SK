using NUnit.Framework.Constraints;
using UnityEngine;

public class PipInteraction : MonoBehaviour, IInteractable
{
    [Header("UI Setup")]
    public string pipName = "Pip";
    public Sprite pipPortrait;
    [Tooltip("Pip's unique high-pitched bird voice sound asset.")]
    public AudioClip pipVoiceSFX;

    [Header("Interaction Prompt UI")]
    [SerializeField] private CanvasGroup promptCanvas;
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private float interactionDistance = 3f;

    [Header("References")]
    public Transform player;

    [Header("Finale Cutscene Target")]
    public Transform finaleFountainTarget;

    [Header("Inspector Editable Instructions")]
    public System.Collections.Generic.List<string> pipInstructionsLines = new System.Collections.Generic.List<string> {
        "We are here, Sound Keeper! The fountain sits at the absolute heart of Word Valley.",
        "All five Vowel Stones are glowing in your wand. But to break Miss Spell's curse completely, you must recite the Ancient Valley Sound Chant.",
        "Come talk to me again whenever you're ready."
    };

    private PipHint pipHintSystem;
    private bool hasSaidHouseUCompletionInMapTest = false;
    private bool arrivedAtFountainFinale = false;
    private bool hasTriggered = false;
    private bool playerInRange = false;

    void Start()
    {
        pipHintSystem = FindAnyObjectByType<PipHint>();
        FindPlayerFallback();

        if (pipPortrait == null && DialogueManager.Instance != null)
        {
            pipPortrait = DialogueManager.Instance.pipIntroPortrait;
        }

        // 1. Check if the game was already completed in a saved session
        bool isGameDone = PlayerPrefs.GetInt("IsEndGameCompleted", 0) == 1;

        // 2. Check if Pip is physically close to his finale fountain target
        bool isAtFinaleTarget = (finaleFountainTarget != null && Vector3.Distance(transform.position, finaleFountainTarget.position) < 1.0f);

        // If either condition is true, set internal flags so Pip doesn't repeat his dialogue/flight
        if (isGameDone || isAtFinaleTarget)
        {
            hasSaidHouseUCompletionInMapTest = true;
            arrivedAtFountainFinale = true;
        }

        if (promptCanvas != null)
        {
            promptCanvas.alpha = 0f;
            promptCanvas.interactable = false;
            promptCanvas.blocksRaycasts = false;
        }
    }

    void Update()
    {
        FindPlayerFallback();

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            playerInRange = distance <= interactionDistance;
        }
        else
        {
            playerInRange = false;
        }

        if (promptCanvas != null)
        {
            float targetAlpha = (playerInRange && !hasTriggered) ? 1f : 0f;
            promptCanvas.alpha = Mathf.MoveTowards(promptCanvas.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

            bool visible = promptCanvas.alpha > 0.001f;
            promptCanvas.interactable = visible;
            promptCanvas.blocksRaycasts = visible;
        }
    }

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

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MapTest")
        {
            return;
        }

        hasTriggered = true;

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
            if (!hasSaidHouseUCompletionInMapTest)
            {
                string finalHouseDialogue = "Sensational! House U is clear!";

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
                hasSaidHouseUCompletionInMapTest = true;
            }
            else
            {
                lines = pipInstructionsLines.ToArray();
                shouldTriggerFlight = false;
                arrivedAtFountainFinale = true;
            }
        }

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
                    Charactercontroller activePlayer = FindAnyObjectByType<Charactercontroller>();
                    if (activePlayer != null) activePlayer.canControl = true;
                }
            };
        }

        if (DialogueManager.Instance != null)
        {
            System.Collections.Generic.List<DialogueLine> structuredLines = new System.Collections.Generic.List<DialogueLine>();
            foreach (string textLine in lines)
            {
                DialogueLine newline = new DialogueLine();
                newline.text = textLine;
                newline.speaker = Speaker.NPC;
                structuredLines.Add(newline);
            }

            // Pip passes her custom 'pipVoiceSFX' forward right here
            DialogueManager.Instance.StartDialogue(
                structuredLines.ToArray(),
                pipName,
                pipPortrait,
                null,
                null,
                DialogueManager.Instance.OnDialogueEnd,
                pipVoiceSFX
            );
        }
    }

    private void ResetTriggerState()
    {
        hasTriggered = false;
    }

    private void TriggerFlightSequence(PipHint.HintObjective nextObjective, bool isFinale)
    {
        Transform destinationTarget = isFinale ? finaleFountainTarget : (nextObjective != null ? nextObjective.hoverLocation : null);

        if (destinationTarget == null) return;

        Charactercontroller activePlayer = FindAnyObjectByType<Charactercontroller>();
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