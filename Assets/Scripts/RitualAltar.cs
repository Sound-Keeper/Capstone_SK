using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class RitualAltar : MonoBehaviour
{
    //the ending - the ritual only works once the player has gathered all 5 vowel stones

    [Header("Interaction Range")]
    public float interactionRange = 3f;
    public float fadeSpeed = 5f;

    [Header("References")]
    [Tooltip("'Press E' prompt (optional).")]
    public CanvasGroup promptCanvas;
    public Transform player;

    [Header("Not enough stones yet (optional)")]
    public string lockedSpeaker = "Pip";
    [TextArea(2, 5)]
    [Tooltip("Played when the player interacts WITHOUT all 5 stones.")]
    public string[] lockedLines;

    [Header("Ritual")]
    [Tooltip("Fires once when the player starts the ritual with all 5 stones. Hook the ending cutscene / win panel here.")]
    public UnityEvent onRitualStart;

    bool playerInRange = false;
    bool ritualDone = false;

    void Start()
    {
        if (promptCanvas != null)
        {
            promptCanvas.alpha = 0f;
            promptCanvas.gameObject.SetActive(true);
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        //re-bind to the surviving player if the baked one was destroyed (PersistPlayer)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;

        if (promptCanvas != null)
        {
            float targetAlpha = (playerInRange && !ritualDone) ? 1f : 0f;
            promptCanvas.alpha = Mathf.MoveTowards(promptCanvas.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            bool visible = promptCanvas.alpha > 0.001f;
            promptCanvas.interactable = visible;
            promptCanvas.blocksRaycasts = visible;
        }

        if (playerInRange && !ritualDone && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    void Interact()
    {
        //gate: the ritual only triggers with all 5 vowel stones
        if (!PuzzleProgress.HasAllVowelStones)
        {
            if (lockedLines != null && lockedLines.Length > 0 && DialogueManager.Instance != null)
                DialogueManager.Instance.StartDialogue(lockedSpeaker, lockedLines);
            return;
        }

        ritualDone = true;
        onRitualStart?.Invoke();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
