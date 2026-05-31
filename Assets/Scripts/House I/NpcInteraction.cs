using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NpcInteraction : MonoBehaviour
{
    //reusable npc interaction - works for any npc, just change the lines in Inspector

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
    }

    void Update()
    {
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
        if (PuzzleProgress.HouseISolved && dialogueLinesAfterSolved.Count > 0)
        {
            linesToPlay = dialogueLinesAfterSolved;
            PuzzleProgress.HouseISolved = false;
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
        //pwedeng makausap ulit
        hasTriggered = false;

        //optional - teleport + scene swap kapag may sceneToLoad
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            if (player != null)
            {
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

    //show interaction range in Scene view when selected (editor only, walang effect sa game)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
