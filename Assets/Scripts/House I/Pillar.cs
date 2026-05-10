using UnityEngine;

public class Pillar : MonoBehaviour
{
    public Transform slotPoint;
    public string expectedLetter = "";
    public bool isFilled = false;

    [Header("Puzzle Reference")]
    public PuzzleManager puzzleManager;

    public void PlaceLetter(PlayerHold hold)
    {
        if (hold == null || hold.held == null) return;
        if (isFilled) return;

        LetterPickup carriedLetter = hold.held;

        // Check if correct letter — if wrong, return to original position
        if (carriedLetter.letter == expectedLetter)
        {
            carriedLetter.transform.position = slotPoint.position;
            carriedLetter.transform.rotation = slotPoint.rotation;
            carriedLetter.transform.localScale = Vector3.one;

            carriedLetter.transform.SetParent(slotPoint);

            Collider col = carriedLetter.GetComponent<Collider>();
            if (col != null)
                col.enabled = true;

            hold.ClearHeld();
            isFilled = true;

            // Notify this pillar's puzzle manager
            if (puzzleManager != null)
                puzzleManager.LetterPlaced();
        }
        else
        {
            Debug.Log("Wrong letter! Returning to original position.");

            hold.ClearHeld();
            carriedLetter.ReturnToStart();
        }
    }
}
