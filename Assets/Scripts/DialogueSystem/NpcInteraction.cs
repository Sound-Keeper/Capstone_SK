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

    bool playerInRange = false;
    bool hasTriggered = false;

    void Start()
    {
        if (promptCanvas != null)
        {
            promptCanvas.alpha = 0f;
            promptCanvas.gameObject.SetActive(true);
        }

        FindPlayerFallback();

        if (autoPlayWhenSolved && IsHouseSolved() && dialogueLinesAfterSolved.Count > 0)
        {
            StartCoroutine(AutoPlayAfterSolved());
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

    void ClearSolvedFlag()
    {
        switch (associatedHouse)
        {
            case PuzzleFlag.HouseA: PuzzleProgress.HouseASolved = false; break;
            case PuzzleFlag.HouseE: PuzzleProgress.HouseESolved = false; break;
            case PuzzleFlag.HouseI: PuzzleProgress.HouseISolved = false; break;
            case PuzzleFlag.HouseO: PuzzleProgress.HouseOSolved = false; break;
            case PuzzleFlag.HouseU: PuzzleProgress.HouseUSolved = false; break;
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

    void Interact()
    {
        hasTriggered = true;

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager.Instance is missing from your scene!");
            return;
        }

        List<DialogueLine> linesToPlay = dialogueLines;

        if (IsHouseSolved() && dialogueLinesAfterSolved.Count > 0)
        {
            linesToPlay = dialogueLinesAfterSolved;
            ClearSolvedFlag();
        }

        DialogueManager.Instance.StartDialogue(
            linesToPlay.ToArray(),
            npcName,
            npcPortrait,
            playerPortrait,
            dialogueCamera,
            OnDialogueComplete
        );
    }

    void OnDialogueComplete()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(ReEnableInteractNextFrame());
            return;
        }

        if (player != null)
        {
            Camera playerCam = player.GetComponentInChildren<Camera>(true);
            if (playerCam != null) playerCam.gameObject.SetActive(true);
        }

        // Trigger your custom scene transition framework
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.SessionContent, sceneToLoad, setActive: true)
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