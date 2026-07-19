using UnityEngine;

public class Pillar : MonoBehaviour
{
    public Transform slotPoint;
    public string expectedLetter = "";
    public bool isFilled = false;

    [Header("Puzzle Reference")]
    public PuzzleManager puzzleManager;

    [Header("Hint (on wrong placement)")]
    [Tooltip("Escalating hint lines shown upper-left on each wrong try.")]
    public string[] wrongHints;
    [Tooltip("Object that glows after enough wrong tries (e.g. the correct letter).")]
    public HintGlow correctAnswerGlow;

    // --- NEW VALIDATION AUDIO SLOTS ---
    [Header("Audio")]
    [Tooltip("Plays when the matching letter block snaps into place successfully.")]
    public AudioClip correctSFX;
    [Tooltip("Plays when a mismatched wrong letter block is offered.")]
    public AudioClip wrongSFX;

    public void PlaceLetter(PlayerHold hold)
    {
        if (hold == null || hold.held == null) return;
        if (isFilled) return;

        LetterPickup carriedLetter = hold.held;

        if (carriedLetter.letter == expectedLetter)
        {
            Transform t = carriedLetter.transform;

            t.SetParent(slotPoint);
            t.position = slotPoint.position;
            t.rotation = slotPoint.rotation;

            SetWorldScale(t, carriedLetter.startScale);

            Collider col = carriedLetter.GetComponent<Collider>();
            if (col != null)
                col.enabled = false;

            hold.ClearHeld();
            isFilled = true;

            Debug.Log("Correct piece placed!");

            // --- PLAY CORRECT SFX ---
            if (correctSFX != null)
            {
                CoreAudioManager.PlaySFX(correctSFX);
            }

            if (puzzleManager != null)
                puzzleManager.LetterPlaced();
        }
        else
        {
            Debug.Log("Wrong piece!");
            hold.ClearHeld();
            carriedLetter.ReturnToStart();

            // --- PLAY WRONG SFX ---
            if (wrongSFX != null)
            {
                CoreAudioManager.PlaySFX(wrongSFX);
            }

            if (PuzzleHint.Instance != null)
                PuzzleHint.Instance.WrongAnswer(wrongHints, correctAnswerGlow);
        }
    }

    static void SetWorldScale(Transform t, Vector3 worldScale)
    {
        t.localScale = Vector3.one;
        Vector3 lossy = t.lossyScale;
        t.localScale = new Vector3(
            lossy.x != 0f ? worldScale.x / lossy.x : worldScale.x,
            lossy.y != 0f ? worldScale.y / lossy.y : worldScale.y,
            lossy.z != 0f ? worldScale.z / lossy.z : worldScale.z
        );
    }
}