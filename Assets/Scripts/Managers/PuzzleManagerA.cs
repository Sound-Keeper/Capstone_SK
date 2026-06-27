using UnityEngine;
using UnityEngine.Events;

// House A puzzle brain. Counts how many correct pieces have been placed; when the
// broken object is fully repaired it fires OnPuzzleComplete (hook a
// PuzzleCompleteReturn there to fade back to MainWorld, like House I).
public class PuzzleManagerA : MonoBehaviour
{
    [Header("Puzzle")]
    public string puzzleName = "House A";
    [Tooltip("How many correct pieces complete the object (count your PieceSlots).")]
    public int piecesNeeded = 3;
    int piecesPlaced = 0;

    [Header("References")]
    [Tooltip("Drag your VowelStone GameObject here.")]
    public VowelStone vowelStone;

    [Header("UI (optional)")]
    public GameObject winPanel;

    [Header("Events")]
    [Tooltip("Fired once when all correct pieces are placed.")]
    public UnityEvent OnPuzzleComplete;

    void Awake()
    {
        if (winPanel != null) winPanel.SetActive(false);
    }

    // Called by each PieceSlot when its correct piece is placed.
    public void PiecePlaced()
    {
        if (PuzzleProgress.HouseAComplete) return; // already solved once - stays locked

        piecesPlaced++;
        Debug.Log(puzzleName + ": piece placed " + piecesPlaced + "/" + piecesNeeded);

        if (piecesPlaced >= piecesNeeded)
            Complete();
    }

    void Complete()
    {
        if (PuzzleProgress.HouseAComplete) return; // only ever complete once

        PuzzleProgress.HouseAComplete = true;      // permanent lock - can't be redone

        Debug.Log(puzzleName + " complete!");
        if (winPanel != null) winPanel.SetActive(true);
        PuzzleProgress.HouseASolved = true;        // one-shot: triggers Sheriff's thank-you
        OnPuzzleComplete?.Invoke();                // hook VowelStone.GiveReward here
    }
    public void TriggerVowelStoneReward()
    {
        if (vowelStone != null)
        {
            Debug.Log("[PuzzleAComplete] Puzzle solved! Triggering Vowel Stone cutscene.");
            vowelStone.GiveReward();
        }
        else
        {
            Debug.LogError("[PuzzleAComplete] VowelStone reference is missing in the Inspector!");
        }
    }
}
