using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NpcInteraction : MonoBehaviour, IInteractable
{
    public enum PuzzleFlag { HouseA, HouseE, HouseI, HouseO, HouseU }

    [Header("Interaction Range")]
    public float interactionRange = 3f;
    public float fadeSpeed = 5f;

    [Header("References")]
    public CanvasGroup promptCanvas;
    public Transform player;

    [Header("Scene Location Context")]
    [Tooltip("CHECK THIS box for the NPC instance inside the House scene. UNCHECK THIS box for the NPC out in MapTest.")]
    public bool isInPuzzleHouse = false;

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

    bool hasTriggered = false;
    bool playerInRange = false;
    private bool waitingForSolvedExit = false;

    void Start()
    {
        if (promptCanvas != null)
        {
            promptCanvas.alpha = 0f;
            promptCanvas.gameObject.SetActive(true);
        }

        FindPlayerFallback();

        // ONLY auto-play the thank-you text if we are out in MapTest (NOT inside the puzzle house)
        if (!isInPuzzleHouse && autoPlayWhenSolved && IsHouseSolved() && dialogueLinesAfterSolved.Count > 0)
        {
            StartCoroutine(AutoPlayAfterSolved());
        }

        // Swap dialogue lines to post-completion if complete
        if (PuzzleProgress.IsHouseComplete(houseLetter))
        {
            if (dialogueLinesAfterSolved != null && dialogueLinesAfterSolved.Count > 0)
            {
                dialogueLines = dialogueLinesAfterSolved;
            }
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

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            playerInRange = distance <= interactionRange;
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
        if (hasTriggered || !playerInRange) return;
        hasTriggered = true;

        if (promptCanvas != null) promptCanvas.alpha = 0f;

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
        // 1. IF INSIDE HOUSE: Warp player to MapTest on completion
        if (isInPuzzleHouse && (IsHouseSolved() || PuzzleProgress.IsHouseComplete(houseLetter) || (waitingForSolvedExit && IsHouseSolved())))
        {
            StartCoroutine(WaitAndWarpRoutine());
            return;
        }

        // 2. IF OUT IN MAPTEST BEFORE PUZZLE IS DONE: Entry warp to enter the house
        if (!isInPuzzleHouse && !PuzzleProgress.IsHouseComplete(houseLetter) && !string.IsNullOrEmpty(sceneToLoad) && sceneToLoad != "MapTest")
        {
            ExecuteSceneWarp(sceneToLoad);
            return;
        }

        // 3. IF OUT IN MAPTEST AFTER PUZZLE IS DONE: Just say lines, end dialogue, and do not warp anywhere!
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
        ExecuteSceneWarp("MapTest");
    }

    private void ExecuteSceneWarp(string destinationScene)
    {
        if (player != null)
        {
            if (destinationScene != "MapTest" && CoreManager.Instance != null)
            {
                CoreManager.Instance.SavePlayerPosition(player.position, player.rotation);
            }

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

        Charactercontroller activePlayer = FindFirstObjectByType<Charactercontroller>();
        if (activePlayer != null)
        {
            activePlayer.canControl = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}