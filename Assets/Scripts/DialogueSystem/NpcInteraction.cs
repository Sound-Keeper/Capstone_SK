using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NpcInteraction : MonoBehaviour
{
    public enum PuzzleFlag { HouseA, HouseE, HouseI, HouseO, HouseU }

    [Header("Interaction Range")]
    public float interactionRange = 3f;
    public float fadeSpeed = 5f;

    [Header("References")]
    public CanvasGroup promptCanvas;
    public Transform player;

    [Header("Dialogue Config")]
    public string npcName = "NPC";
    [Tooltip("Face shown when the NPC is speaking. Optional.")]
    public Sprite npcPortrait;
    [Tooltip("Face shown when the Player is speaking. Optional.")]
    public Sprite playerPortrait;

    [Tooltip("Standard lines before the puzzle is finished.")]
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();

    [Tooltip("Lines played after this specific house puzzle is solved.")]
    public List<DialogueLine> dialogueLinesAfterSolved = new List<DialogueLine>();

    public Camera dialogueCamera;

    [Header("Scene Transition (Optional)")]
    public string sceneToLoad;

    [Header("Quest Assignment")]
    [Tooltip("Which house's puzzle checklist flag belongs to this NPC?")]
    public PuzzleFlag associatedHouse = PuzzleFlag.HouseA;

    [Tooltip("If true, this NPC starts their thank-you dialogue immediately when you return to MainWorld without pressing E.")]
    public bool autoPlayWhenSolved = false;
    public float autoPlayDelay = 1.5f;

    [Header("Solved Settings")]
    [Tooltip("Check if this specific house is solved (e.g., 'A', 'E', 'I')")]
    public string houseLetter = "A";

    bool playerInRange = false;
    bool hasTriggered = false;
    private bool waitingForSolvedExit = false;

    void Start()
    {
        if (promptCanvas != null)
        {
            promptCanvas.alpha = 0f;
            promptCanvas.gameObject.SetActive(true);
        }

        FindPlayerFallback();

        // 🌟 Fix: Only auto-play if THIS specific house is the one that was solved!
        if (autoPlayWhenSolved && IsHouseSolved() && dialogueLinesAfterSolved.Count > 0)
        {
            StartCoroutine(AutoPlayAfterSolved());
        }

        // Check if THIS specific house's puzzle has been solved
        if (PuzzleProgress.IsHouseComplete(houseLetter))
        {
            if (dialogueLinesAfterSolved != null && dialogueLinesAfterSolved.Count > 0)
            {
                dialogueLines = dialogueLinesAfterSolved;
            }

            // Point this specific NPC's target back to MainWorld since it's already cleared
            sceneToLoad = "MainWorld";
        }
    }

    IEnumerator AutoPlayAfterSolved()
    {
        yield return new WaitForSeconds(autoPlayDelay);
        if (IsHouseSolved() && !hasTriggered)
        {
            Interact();
        }
    }

    bool IsHouseSolved()
    {
        switch (associatedHouse)
        {
            case PuzzleFlag.HouseA: return PuzzleProgress.HouseASolved;
            case PuzzleFlag.HouseE: return PuzzleProgress.HouseESolved;
            case PuzzleFlag.HouseI: return PuzzleProgress.HouseISolved;
            case PuzzleFlag.HouseO: return PuzzleProgress.HouseOSolved;
            case PuzzleFlag.HouseU: return PuzzleProgress.HouseUSolved;
            default: return false;
        }
    }

    void Update()
    {
        FindPlayerFallback();

        if (player == null || promptCanvas == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;

        float targetAlpha = (playerInRange && !hasTriggered) ? 1f : 0f;
        promptCanvas.alpha = Mathf.MoveTowards(promptCanvas.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

        bool visible = promptCanvas.alpha > 0.001f;
        promptCanvas.interactable = visible;
        promptCanvas.blocksRaycasts = visible;

        if (playerInRange && !hasTriggered && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
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
        if (PuzzleProgress.IsHouseComplete(houseLetter))
        {
            if (dialogueLinesAfterSolved != null && dialogueLinesAfterSolved.Count > 0)
            {
                dialogueLines = dialogueLinesAfterSolved;
            }

            waitingForSolvedExit = true;
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                dialogueLines.ToArray(),
                npcName,
                npcPortrait,
                playerPortrait,
                dialogueCamera,
                OnDialogueComplete
            );
        }
    }

    void OnDialogueComplete()
    {
        // 🌟 THE FIX: Only hardcode "MainWorld" if THIS SPECIFIC house is actually complete!
        // We check IsHouseSolved() or IsHouseComplete(houseLetter) directly here.
        if (IsHouseSolved() || PuzzleProgress.IsHouseComplete(houseLetter) || (waitingForSolvedExit && IsHouseSolved()))
        {
            StartCoroutine(WaitAndWarpRoutine());
            return;
        }

        // 🌟 If THIS house is NOT complete, behave normally! 
        // Go to the puzzle scene (e.g., House E) if it's set in the inspector.
        if (!string.IsNullOrEmpty(sceneToLoad) && sceneToLoad != "MainWorld")
        {
            ExecuteSceneWarp(sceneToLoad);
            return;
        }

        // Default fallback if inside a house room
        StartCoroutine(ReEnableInteractNextFrame());
    }

    private System.Collections.IEnumerator WaitAndWarpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        if (DialogueManager.Instance != null && DialogueManager.Instance.dialoguePanel != null)
        {
            while (DialogueManager.Instance.dialoguePanel.activeSelf)
            {
                yield return null;
            }
        }

        waitingForSolvedExit = false;
        ExecuteSceneWarp("MainWorld");
    }

    private void ExecuteSceneWarp(string destinationScene)
    {
        if (player != null)
        {
            // 💾 FIX: Check destinationScene! If we are going ANYWHERE except MainWorld, save the location.
            if (destinationScene != "MainWorld" && CoreManager.Instance != null)
            {
                CoreManager.Instance.SavePlayerPosition(player.position, player.rotation);
            }
            // 🧹 If we are actually returning back to MainWorld, do NOT clear the data here.
            // Let your player prefab script read the data, use it, and have the player script clear it!

            Camera playerCam = player.GetComponentInChildren<Camera>(true);
            if (playerCam != null) playerCam.gameObject.SetActive(true);
        }

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.SessionContent, destinationScene, setActive: true)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }

    IEnumerator ReEnableInteractNextFrame()
    {
        yield return null;
        hasTriggered = false;
    }
}