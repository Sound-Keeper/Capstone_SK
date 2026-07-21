using System.Collections;
using UnityEngine;
using TMPro;

public enum QuestState
{
    TalkToPipAtStart,
    FollowPipToDestination,
    TalkToNPC,
    TalkToPipAtEnd,
    Completed
}

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private GameObject highlightBox;        // Outer checkbox frame (stays ON with text)
    [SerializeField] private GameObject completeHighlight;   // Green checkmark fill inside

    public QuestState CurrentState { get; private set; } = QuestState.TalkToPipAtStart;
    public string CurrentTargetNPC { get; private set; } = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Ensure the outer checkbox frame is visible at launch
        if (highlightBox != null) highlightBox.SetActive(true);

        // Set initial objective at launch
        SetObjective(QuestState.TalkToPipAtStart);
    }

    /// <summary>
    /// Returns the name of the NPC associated with the current uncompleted house.
    /// Order: Sheriff Sans (A) > Judge Mental (E) > Penny Cil (I) > Grandma Phonetics (O) > Connie Sonant (U)
    /// </summary>
    public string GetCurrentActiveNPCName()
    {
        if (!PuzzleProgress.HouseAComplete) return "Sheriff Sans";
        if (!PuzzleProgress.HouseEComplete) return "Judge Mental";
        if (!PuzzleProgress.HouseIComplete) return "Penny Cil";
        if (!PuzzleProgress.HouseOComplete) return "Grandma Phonetics";
        if (!PuzzleProgress.HouseUComplete) return "Connie Sonant";

        return ""; // All houses complete!
    }

    /// <summary>
    /// Sets a new active objective text and resets (unchecks) the checkmark box.
    /// </summary>
    public void SetObjective(QuestState newState, string npcName = "")
    {
        CurrentState = newState;
        CurrentTargetNPC = npcName;

        // Keep the checkbox frame active
        if (highlightBox != null) highlightBox.SetActive(true);

        // Clear green checkmark for the new task
        if (completeHighlight != null) completeHighlight.SetActive(false);

        // Update display text
        if (objectiveText != null)
        {
            objectiveText.text = GetObjectiveMessage(newState, npcName);
        }
    }

    /// <summary>
    /// Call this when the player finishes the task to show the green checkmark inside the box!
    /// </summary>
    public void CompleteCurrentObjective()
    {
        if (completeHighlight != null)
        {
            completeHighlight.SetActive(true);
        }
    }

    private string GetObjectiveMessage(QuestState state, string npcName)
    {
        switch (state)
        {
            case QuestState.TalkToPipAtStart:
                return "Talk to Pip";

            case QuestState.FollowPipToDestination:
                return "Follow Pip and talk to him";

            case QuestState.TalkToNPC:
                return string.IsNullOrEmpty(npcName) ? "Talk to the NPC" : $"Talk to {npcName}";

            case QuestState.TalkToPipAtEnd:
                return "Talk to Pip at the Fountain";

            case QuestState.Completed:
                return "All Objectives Complete!";

            default:
                return "";
        }
    }
}