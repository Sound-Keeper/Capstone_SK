using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private bool hasHeardFinaleInstructions = false;
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
        // --- 1. TURN CHECKMARK GREEN IMMEDIATELY ON INTERACT ---
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.CompleteCurrentObjective();
        }

        // --- 2. AFTER-CUTSCENE INTERACTION WITH ARCHMAGE ---
        if (PlayerPrefs.GetInt("IsEndGameCompleted", 0) == 1)
        {
            DialogueManager.Instance.SetPlayerControlState(false);

            DialogueLine thankYouLine = new DialogueLine
            {
                speaker = Speaker.NPC,
                text = "Thank you again, Sound Keeper! You have saved Word Valley. It is time for you to rest."
            };

            Sprite archmagePortrait = EndGameCutscene.Instance != null ? EndGameCutscene.Instance.archmagePortrait : null;

            DialogueManager.Instance.StartDialogue(
                new DialogueLine[] { thankYouLine },
                "Archmage",
                archmagePortrait,
                DialogueManager.Instance.pennPortrait,
                null,
                OnArchmageFarewellComplete
            );

            return;
        }

        // Check if Pip is at the finale fountain target or all house objectives are clear
        PipHint.HintObjective nextObjective = pipHintSystem != null ? pipHintSystem.GetActiveObjective() : null;
        bool isAtFinale = arrivedAtFountainFinale || (nextObjective == null && hasSaidHouseUCompletionInMapTest);

        // --- 3. FOUNTAIN FINALE DIALOGUE & CUTSCENE LAUNCH ---
        if (isAtFinale)
        {
            DialogueManager.Instance.SetPlayerControlState(false);

            // 3A. SECOND TALK AT FOUNTAIN: Start the Chant Cutscene!
            if (hasHeardFinaleInstructions)
            {
                DialogueLine readyLine = new DialogueLine
                {
                    speaker = Speaker.NPC,
                    text = "Let the Ancient Valley Sound Chant begin!"
                };

                DialogueManager.Instance.OnDialogueEnd = () =>
                {
                    ResetTriggerState();
                    if (EndGameCutscene.Instance != null)
                    {
                        EndGameCutscene.Instance.StartFountainRitual();
                    }
                };

                DialogueManager.Instance.StartDialogue(
                    new DialogueLine[] { readyLine },
                    pipName,
                    pipPortrait,
                    null,
                    null,
                    DialogueManager.Instance.OnDialogueEnd
                );

                return;
            }

            // 3B. FIRST TALK AT FOUNTAIN: Read instructions first!
            System.Collections.Generic.List<DialogueLine> finaleLines = new System.Collections.Generic.List<DialogueLine>();
            foreach (string textLine in pipInstructionsLines)
            {
                DialogueLine line = new DialogueLine();
                line.text = textLine;
                line.speaker = Speaker.NPC;
                finaleLines.Add(line);
            }

            DialogueManager.Instance.OnDialogueEnd = () =>
            {
                ResetTriggerState();
                hasHeardFinaleInstructions = true;

                if (ObjectiveManager.Instance != null)
                {
                    ObjectiveManager.Instance.SetObjective(QuestState.TalkToPipAtEnd);
                }

                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.SetPlayerControlState(true);
                }
            };

            DialogueManager.Instance.StartDialogue(
                finaleLines.ToArray(),
                pipName,
                pipPortrait,
                null,
                null,
                DialogueManager.Instance.OnDialogueEnd
            );

            return;
        }

        // --- 4. STANDARD GAMEPLAY QUEST DIALOGUE & FLIGHT LOGIC ---
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
                // Simple standard flight dialogue without forced custom text loops
                lines = new string[] {
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
            lines = new string[] {
                "Sensational! All house puzzles are clear!",
                "I'll meet you over at the center fountain right away!"
            };

            shouldTriggerFlight = true;
            headingToFountainFinale = true;
            hasSaidHouseUCompletionInMapTest = true;
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

                if (ObjectiveManager.Instance != null)
                {
                    string currentNPC = ObjectiveManager.Instance.GetCurrentActiveNPCName();
                    if (!string.IsNullOrEmpty(currentNPC))
                    {
                        ObjectiveManager.Instance.SetObjective(QuestState.TalkToNPC, currentNPC);
                    }
                    else
                    {
                        ObjectiveManager.Instance.SetObjective(QuestState.TalkToPipAtEnd);
                    }
                }

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

            DialogueManager.Instance.StartDialogue(
                structuredLines.ToArray(),
                pipName,
                pipPortrait,
                null,
                null,
                DialogueManager.Instance.OnDialogueEnd
            );
        }
    }

    private void OnArchmageFarewellComplete()
    {
        Debug.Log("[Archmage] Returning to Main Menu via SceneController...");

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.SetPlayerControlState(true);
        }

        Time.timeScale = 1.0f;

        // Unlock mouse cursor for Main Menu navigation
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (SceneController.Instance != null)
        {
            SceneController.Instance
                .NewTransition()
                .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
                .Unload(SceneDatabase.Slots.Session)
                .Unload(SceneDatabase.Slots.SessionContent)
                .WithClearUnusedAssets()
                .WithOverlay()
                .Perform();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
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
            if (ObjectiveManager.Instance != null)
            {
                if (isFinale)
                {
                    ObjectiveManager.Instance.SetObjective(QuestState.TalkToPipAtEnd);
                }
                else
                {
                    ObjectiveManager.Instance.SetObjective(QuestState.FollowPipToDestination);
                }
            }

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