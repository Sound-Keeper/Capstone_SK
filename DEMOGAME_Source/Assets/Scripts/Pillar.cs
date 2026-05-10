using UnityEngine;

public class Pillar : MonoBehaviour
{
    public Transform slotPoint;
    public string expectedLetter = "";
    public bool isFilled = false;

    public void PlaceLetter(PlayerHold hold)
    {
        if (hold == null || hold.held == null) return;
        if (isFilled) return;

        LetterG carriedLetter = hold.held;

        // CHECK IF CORRECT LETTER PAG HINDI BABALIK SA PWESTO
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

            HouseManager.Instance.LetterPlaced();
        }
        else
        {
            Debug.Log("Wrong letter! Returning to original position.");

            hold.ClearHeld();
            carriedLetter.ReturnToStart();
        }
    }
}