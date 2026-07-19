using UnityEngine;

public class PieceSlot : MonoBehaviour
{
    public string expectedPieceID;
    [Tooltip("Where the piece sits when placed. Leave empty to use this object's transform.")]
    public Transform slotPoint;
    public bool isFilled = false;

    [Header("Puzzle")]
    [Tooltip("The House A puzzle manager. Notified when a correct piece is placed.")]
    public PuzzleManagerA puzzle;

    [Header("Hint (on wrong placement)")]
    [Tooltip("Escalating hint lines shown upper-left on each wrong try.")]
    public string[] wrongHints;
    [Tooltip("Object that glows after enough wrong tries (e.g. the correct piece).")]
    public HintGlow correctAnswerGlow;

    // --- NEW VALIDATION AUDIO SLOTS ---
    [Header("Audio")]
    [Tooltip("Plays when the matching piece snaps into place successfully.")]
    public AudioClip correctSFX;
    [Tooltip("Plays when a mismatched wrong piece is offered.")]
    public AudioClip wrongSFX;

    void Awake()
    {
        if (slotPoint == null) slotPoint = transform;
    }

    public void PlacePiece(PieceHold hold)
    {
        if (hold == null || hold.held == null) return;
        if (isFilled) return;

        BrokenPiece piece = hold.held;
        if (piece.IsBusy) return;

        if (piece.pieceID == expectedPieceID)
        {
            hold.ClearHeld();

            piece.transform.SetParent(slotPoint, true);
            piece.transform.position = slotPoint.position;
            piece.transform.rotation = slotPoint.rotation;

            Collider col = piece.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            isFilled = true;
            Debug.Log("Correct piece placed: " + expectedPieceID);

            // --- PLAY CORRECT SFX ---
            if (correctSFX != null)
            {
                CoreAudioManager.PlaySFX(correctSFX);
            }

            if (puzzle != null) puzzle.PiecePlaced();
        }
        else
        {
            Debug.Log("Wrong piece for slot: " + expectedPieceID);
            hold.ClearHeld();
            piece.VibrateAndReturn();

            // --- PLAY WRONG SFX ---
            if (wrongSFX != null)
            {
                CoreAudioManager.PlaySFX(wrongSFX);
            }

            if (PuzzleHint.Instance != null)
                PuzzleHint.Instance.WrongAnswer(wrongHints, correctAnswerGlow);
        }
    }
}