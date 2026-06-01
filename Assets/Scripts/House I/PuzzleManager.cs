using UnityEngine;
using UnityEngine.Events;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager ActivePuzzle;

    [Header("Puzzle Info")]
    public string puzzleName = "House I";

    [Header("UI")]
    public GameObject winPanel;

    [Header("Puzzle")]
    public int lettersNeeded = 2;
    private int lettersPlaced = 0;

    [Header("Puzzle Objects")]
    public GameObject[] puzzleObjects;

    [Header("Events")]
    public UnityEvent OnPuzzleComplete;

    public bool IsPuzzleActive
    {
        get { return ActivePuzzle == this; }
    }

    void Awake()
    {
        // Hide win panel
        if (winPanel != null)
            winPanel.SetActive(false);

        // Hide all puzzle objects at start
        SetPuzzleObjectsActive(false);
    }

    public void ActivatePuzzle()
    {
        // already solved once before? keep the puzzle locked so it can't be replayed.
        if (PuzzleProgress.HouseIComplete)
        {
            Debug.Log($"{puzzleName} already solved - puzzle stays locked.");
            return;
        }

        if (ActivePuzzle == this) return; // Prevent double activation

        // Deactivate any other active puzzle first
        if (ActivePuzzle != null)
            ActivePuzzle.ClosePuzzle();

        ActivePuzzle = this;
        lettersPlaced = 0;

        SetPuzzleObjectsActive(true);

        Debug.Log($"{puzzleName} puzzle activated!");
    }

    public void LetterPlaced()
    {
        lettersPlaced++;
        Debug.Log($"{puzzleName}: Letter placed! {lettersPlaced}/{lettersNeeded}");

        if (lettersPlaced >= lettersNeeded)
        {
            CompletePuzzle();
        }
    }

    void CompletePuzzle()
    {
        if (PuzzleProgress.HouseIComplete) return; // only ever complete once

        PuzzleProgress.HouseIComplete = true;      // permanent lock - can't be redone

        ShowWin();

        Debug.Log($"{puzzleName} puzzle complete!");
        OnPuzzleComplete?.Invoke();   // hook the stone reward + return-to-MainWorld here
    }

    void ShowWin()
    {
        if (winPanel != null)
            winPanel.SetActive(true);
    }

    public void ClosePuzzle()
    {
        ActivePuzzle = null;

        if (winPanel != null)
            winPanel.SetActive(false);

        SetPuzzleObjectsActive(false);

        Debug.Log($"{puzzleName} puzzle closed.");
    }

    void SetPuzzleObjectsActive(bool active)
    {
        if (puzzleObjects == null) return;

        foreach (GameObject obj in puzzleObjects)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }
}
