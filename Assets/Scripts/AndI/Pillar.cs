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

            //keep the piece's ORIGINAL world size so a scaled pillar
            //doesn't squeeze/stretch it
            SetWorldScale(t, carriedLetter.startScale);

            Collider col = carriedLetter.GetComponent<Collider>();
            if (col != null)
                col.enabled = true;

            hold.ClearHeld();
            isFilled = true;

            Debug.Log("Correct piece placed!");

            //tell the puzzle manager so it can check for completion
            if (puzzleManager != null)
                puzzleManager.LetterPlaced();
        }
        else
        {
            Debug.Log("Wrong piece!");
            hold.ClearHeld();
            carriedLetter.ReturnToStart();

            if (PuzzleHint.Instance != null)
                PuzzleHint.Instance.WrongAnswer(wrongHints, correctAnswerGlow);
        }
    }

    //sets an object's world (lossy) scale no matter how its parent is scaled
    static void SetWorldScale(Transform t, Vector3 worldScale)
    {
        t.localScale = Vector3.one;
        Vector3 lossy = t.lossyScale; //now equals the parent's effective scale
        t.localScale = new Vector3(
            lossy.x != 0f ? worldScale.x / lossy.x : worldScale.x,
            lossy.y != 0f ? worldScale.y / lossy.y : worldScale.y,
            lossy.z != 0f ? worldScale.z / lossy.z : worldScale.z
        );
    }
}


