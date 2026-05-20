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

        if (carriedLetter.letter == expectedLetter)
        {
            carriedLetter.transform.position = slotPoint.position;
            carriedLetter.transform.rotation = slotPoint.rotation;
            carriedLetter.transform.localScale = carriedLetter.startScale;
            carriedLetter.transform.SetParent(slotPoint);

            Collider col = carriedLetter.GetComponent<Collider>();
            if (col != null)
                col.enabled = true;

            hold.ClearHeld();
            isFilled = true;

            
            Debug.Log("Correct piece placed!");
        }
        else
        {
            Debug.Log("Wrong piece!");
            hold.ClearHeld();
            carriedLetter.ReturnToStart();
        }
    }
}


