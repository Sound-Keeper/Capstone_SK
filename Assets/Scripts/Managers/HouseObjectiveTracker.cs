using UnityEngine;
using TMPro;
using BookChoice; // Required for HouseUPuzzleManager namespace

public class HouseObjectiveTracker : MonoBehaviour
{
    public enum HouseType { HouseA, HouseI, HouseU }

    [Header("House Config")]
    [Tooltip("Select which house puzzle this tracker should listen to.")]
    public HouseType houseType = HouseType.HouseA;

    [Header("UI References")]
    [Tooltip("Drag the TextMeshPro text element here.")]
    public TMP_Text objectiveText;
    [Tooltip("Outer frame/box GameObject.")]
    public GameObject highlightBox;
    [Tooltip("Green checkmark fill GameObject inside the box.")]
    public GameObject completeHighlight;

    [Header("Objective Display Config")]
    [Tooltip("The label shown after the numbers (e.g. 'letters placed').")]
    public string progressLabel = "letters placed";

    private int currentCount = 0;
    private int totalCount = 0;

    void Start()
    {
        if (highlightBox != null) highlightBox.SetActive(true);
        if (completeHighlight != null) completeHighlight.SetActive(false);

        InitializeTracker();
        UpdateUI();
    }

    void Update()
    {
        DetectProgress();
    }

    private void InitializeTracker()
    {
        switch (houseType)
        {
            case HouseType.HouseA:
                PuzzleManagerA puzzleA = FindAnyObjectByType<PuzzleManagerA>();
                if (puzzleA != null) totalCount = puzzleA.piecesNeeded;
                break;

            case HouseType.HouseI:
                PuzzleManager puzzleI = FindAnyObjectByType<PuzzleManager>();
                if (puzzleI != null) totalCount = puzzleI.lettersNeeded;
                break;

            case HouseType.HouseU:
                totalCount = 3;
                break;
        }
    }

    private void DetectProgress()
    {
        int detectedCount = 0;

        switch (houseType)
        {
            case HouseType.HouseA:
                PuzzleManagerA puzzleA = FindAnyObjectByType<PuzzleManagerA>();
                if (puzzleA != null)
                {
                    var field = typeof(PuzzleManagerA).GetField("piecesPlaced", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null) detectedCount = (int)field.GetValue(puzzleA);
                }
                break;

            case HouseType.HouseI:
                PuzzleManager puzzleI = FindAnyObjectByType<PuzzleManager>();
                if (puzzleI != null)
                {
                    var field = typeof(PuzzleManager).GetField("lettersPlaced", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null) detectedCount = (int)field.GetValue(puzzleI);
                }
                break;

            case HouseType.HouseU:
                HouseUPuzzleManager puzzleU = HouseUPuzzleManager.Instance != null ? HouseUPuzzleManager.Instance : FindAnyObjectByType<HouseUPuzzleManager>();
                if (puzzleU != null)
                {
                    var field = typeof(HouseUPuzzleManager).GetField("completedShelvesCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null) detectedCount = (int)field.GetValue(puzzleU);
                }
                break;
        }

        currentCount = detectedCount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        bool isComplete = currentCount >= totalCount && totalCount > 0;

        // Keep displaying "X/Y letters placed"
        if (objectiveText != null)
        {
            objectiveText.text = $"{currentCount}/{totalCount} {progressLabel}";
        }

        // Checkmark activates as soon as current count meets total count
        if (completeHighlight != null)
        {
            completeHighlight.SetActive(isComplete);
        }
    }
}