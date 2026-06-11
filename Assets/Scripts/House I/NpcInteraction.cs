using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NpcInteraction : MonoBehaviour
{
    //reusable npc interaction - works for any npc, just change the lines in Inspector

    //which house's puzzle this NPC reacts to for its thank-you lines (Penny=House I, Sheriff=House A)
    public enum PuzzleFlag { HouseI, HouseA }

    [Header("Interaction Range")]
    public float interactionRange = 3f;
    public float fadeSpeed = 5f;

    [Header("References")]
    public CanvasGroup promptCanvas;
    public Transform player;

    [Header("Dialogue")]
    public string npcName = "NPC";
    public string playerName = "You";
    [Tooltip("Face shown when the NPC is speaking. Optional.")]
    public Sprite npcPortrait;
    [Tooltip("Face shown when the Player is speaking. Optional.")]
    public Sprite playerPortrait;
    [Tooltip("Add each line and pick who speaks it (NPC or Player) in the Inspector.")]
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    [Tooltip("If the House I puzzle is solved, these lines play instead (e.g. Penny Cil thanking the player). Optional.")]
    public List<DialogueLine> dialogueLinesAfterSolved = new List<DialogueLine>();
    public Camera dialogueCamera;

    [Header("Scene Transition (Optional)")]
    public string sceneToLoad;
    public Vector3 spawnPoint;

    [Header("Thank-You After Puzzle (Optional)")]
    [Tooltip("Which house's puzzle, when solved, makes this NPC play its 'after solved' (thank-you) lines. Penny Cil = House I, Sheriff Sans = House A.")]
    public PuzzleFlag solvedFlag = PuzzleFlag.HouseI;
    [Tooltip("If ON and that puzzle is solved, this NPC starts its 'after solved' lines BY ITSELF shortly after the scene loads - no key press. Turn ON for the thank-you when you return to MainWorld.")]
    public bool autoPlayWhenSolved = false;
    [Tooltip("Seconds to wait after the scene loads before auto-playing, so the screen can fade in and the camera settles first.")]
    public float autoPlayDelay = 1.5f;

    [Header("Gizmo (Editor Only)")]
    public Color gizmoColor = Color.yellow;

    bool playerInRange = false;
    bool hasTriggered = false;

    void Start()
    {
        //start na invisible ang prompt
        if (promptCanvas != null)
        {
            promptCanvas.alpha = 0f;
            promptCanvas.gameObject.SetActive(true); //keep active so we can fade it in/out
        }

        //auto-find player if not assigned
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        //returning to MainWorld with the puzzle solved? play the thank-you on our own.
        if (autoPlayWhenSolved && IsSolvedFlagSet() && dialogueLinesAfterSolved.Count > 0)
        {
            StartCoroutine(AutoPlayAfterSolved());
        }
    }

    IEnumerator AutoPlayAfterSolved()
    {
        //wait for the scene-transition fade to finish so the camera handoff to the
        //dialogue camera is clean (SceneController re-enables the player camera first).
        yield return new WaitForSeconds(autoPlayDelay);

        //Interact() picks the 'after solved' lines and clears the solved flag,
        //so this only fires once.
        if (IsSolvedFlagSet() && !hasTriggered)
        {
            Interact();
        }
    }

    //read/clear this NPC's chosen house solved-flag, so the same script serves any house
    bool IsSolvedFlagSet()
    {
        switch (solvedFlag)
        {
            case PuzzleFlag.HouseA: return PuzzleProgress.HouseASolved;
            default:                return PuzzleProgress.HouseISolved;
        }
    }

    void ClearSolvedFlag()
    {
        switch (solvedFlag)
        {
            case PuzzleFlag.HouseA: PuzzleProgress.HouseASolved = false; break;
            default:                PuzzleProgress.HouseISolved = false; break;
        }
    }

    void Update()
    {
        //the serialized player ref points to the scene-baked wizard, which PersistPlayer's
        //singleton guard destroys when we return to MainWorld from House I. Re-bind to the
        //surviving (persistent) player by tag whenever we've lost it, so NPCs keep working
        //(e.g. Penny's thank-you lines after the puzzle).
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player == null || promptCanvas == null) return;

        //check distance of player and npc
        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;

        //fade in when player is in range, fade out when away or while talking
        float targetAlpha = (playerInRange && !hasTriggered) ? 1f : 0f;
        promptCanvas.alpha = Mathf.MoveTowards(
            promptCanvas.alpha,
            targetAlpha,
            fadeSpeed * Time.deltaTime
        );

        //don't let the hidden prompt block clicks/raycasts
        bool visible = promptCanvas.alpha > 0.001f;
        promptCanvas.interactable = visible;
        promptCanvas.blocksRaycasts = visible;

        //press E to talk
        if (playerInRange && !hasTriggered && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    void Interact()
    {
        Debug.Log("Talking to " + npcName);
        hasTriggered = true;

        if (DialogueHouseI.Instance == null)
        {
            Debug.LogError("DialogueHouseI.Instance is NULL!");
            return;
        }

        //if the puzzle was just solved, play the thank-you lines once
        List<DialogueLine> linesToPlay = dialogueLines;
        if (IsSolvedFlagSet() && dialogueLinesAfterSolved.Count > 0)
        {
            linesToPlay = dialogueLinesAfterSolved;
            ClearSolvedFlag();
        }

        DialogueHouseI.Instance.StartDialogue(
            linesToPlay.ToArray(),
            npcName,
            playerName,
            npcPortrait,
            playerPortrait,
            dialogueCamera,
            OnDialogueComplete
        );
    }

    void OnDialogueComplete()
    {
        //pwedeng makausap ulit - but wait one frame first. Advancing dialogue and
        //starting it both read the E key, so the SAME E press that closed this
        //conversation could be seen by Update() and instantly re-open it (Unity's
        //update order between the two scripts is undefined). Deferring a frame lets
        //that press clear before we listen for E again.
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(ReEnableInteractNextFrame());
            return;
        }

        //optional - teleport + scene swap kapag may sceneToLoad
        {
            if (player != null)
            {
                //make sure the player's own camera is back on before we swap scenes.
                //the dialogue camera switch (DialogueHouseI) disables it, and the scene we
                //load (e.g. House I) has no camera of its own - so if it isn't re-enabled
                //we transition into a scene with no active camera and the game looks frozen
                //on a black "loading" screen even though the swap actually succeeded.
                Camera playerCam = player.GetComponentInChildren<Camera>(true);
                if (playerCam != null) playerCam.gameObject.SetActive(true);

                Charactercontroller cc = player.GetComponent<Charactercontroller>();
                if (cc != null)
                {
                    cc.enabled = false;
                    player.position = spawnPoint;
                    cc.enabled = true;
                }
                else
                {
                    player.position = spawnPoint;
                }
            }

            SceneController.Instance
                .NewTransition()
                .Load(SceneDatabase.Slots.SessionContent, sceneToLoad, setActive: true)
                .WithOverlay()
                .WithClearUnusedAssets()
                .Perform();
        }
    }

    //let this NPC be talked to again, but only AFTER the frame that closed the
    //dialogue - so the E press that ended it can't immediately restart it.
    IEnumerator ReEnableInteractNextFrame()
    {
        yield return null;
        hasTriggered = false;
    }

    //show interaction range in Scene view when selected (editor only, walang effect sa game)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
