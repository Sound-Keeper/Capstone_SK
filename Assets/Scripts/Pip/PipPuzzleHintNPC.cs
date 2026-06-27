using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PipPuzzleHintNPC : MonoBehaviour, IInteractable
{
    [Header("Identity")]
    public string pipName = "Pip";
    [Tooltip("Assign Pip's puzzle face sprite here.")]
    public Sprite pipPortrait;

    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public float fadeSpeed = 5f;
    public CanvasGroup promptCanvas;

    [Header("Dialogue Configuration")]
    [Tooltip("The lines Pip cycles through inside this puzzle house.")]
    [TextArea(2, 4)]
    public List<string> hints = new List<string>();

    [Tooltip("Dialogue when everything in this specific house puzzle is completed.")]
    [TextArea(2, 4)]
    public string completionLine = "Fantastic! The vowel stone is secure. Talk to the local villager so we can fly to the next house!";

    [Header("Puzzle Completion Check")]
    [Tooltip("Which house letter does this puzzle scene belong to? (A, E, I, O, U)")]
    public string houseLetter = "U";

    private Transform playerTransform;
    private bool playerInRange = false;
    private bool isInteracting = false;

    void Start()
    {
        if (promptCanvas != null)
        {
            promptCanvas.alpha = 0f;
            promptCanvas.gameObject.SetActive(true);
        }

        FindPlayer();
    }

    void Update()
    {
        if (playerTransform == null)
        {
            FindPlayer();
        }

        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            playerInRange = distance <= interactionRange;
        }
        else
        {
            playerInRange = false;
        }

        // Handle prompt visibility based on proximity
        if (promptCanvas != null)
        {
            float targetAlpha = (playerInRange && !isInteracting) ? 1f : 0f;
            promptCanvas.alpha = Mathf.MoveTowards(promptCanvas.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

            bool visible = promptCanvas.alpha > 0.001f;
            promptCanvas.interactable = visible;
            promptCanvas.blocksRaycasts = visible;
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    public void Interact()
    {
        if (!playerInRange || isInteracting || DialogueManager.Instance == null) return;

        isInteracting = true;
        if (promptCanvas != null) promptCanvas.alpha = 0f;

        string[] contextLines;

        // Check if this house has already been solved
        if (PuzzleProgress.IsHouseComplete(houseLetter))
        {
            contextLines = new string[] { completionLine };
        }
        else if (hints != null && hints.Count > 0)
        {
            contextLines = hints.ToArray();
        }
        else
        {
            contextLines = new string[] { "Keep searching! You can find the scattered vowels nearby." };
        }

        // Register custom clear callback before starting dialogue layout
        DialogueManager.Instance.OnDialogueEnd = OnDialogueFinished;
        DialogueManager.Instance.StartDialogue(pipName, contextLines, pipPortrait);
    }

    private void OnDialogueFinished()
    {
        StartCoroutine(ResetInteractionRoutine());
    }

    private IEnumerator ResetInteractionRoutine()
    {
        yield return null; // Wait for frame update separation
        isInteracting = false;

        // Secure character control and snap mouse properties back to standard locked gameplay
        DialogueManager.Instance.SetPlayerControlState(true);
    }
}