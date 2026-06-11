using System.Collections;
using UnityEngine;

// Drop this in the HouseI scene and hook ReturnToMainWorld() to
// PuzzleManager.OnPuzzleComplete (in the Inspector).
// It waits a moment, moves the player back near Penny Cil, flags the puzzle
// as solved, then fades back to MainWorld.
public class PuzzleCompleteReturn : MonoBehaviour
{
    [Header("Return Scene")]
    public string mainWorldScene = SceneDatabase.Scenes.MainWorld;
    public string sceneSlot = SceneDatabase.Slots.SessionContent;
    public bool useFade = true;
    public bool clearUnusedAssets = true;

    [Header("Where the player lands (near Penny Cil)")]
    public Vector3 returnPosition;
    public Vector3 returnEulerRotation;

    [Header("Timing")]
    [Tooltip("Seconds to wait (e.g. to show a win panel) before going back.")]
    public float delayBeforeReturn = 1.5f;

    public string playerTag = "Player";

    [Tooltip("ON for House I (Penny's thank-you lines). Turn OFF when reusing this for House A.")]
    public bool markHouseISolved = true;

    public void ReturnToMainWorld()
    {
        StartCoroutine(ReturnRoutine());
    }

    IEnumerator ReturnRoutine()
    {
        yield return new WaitForSeconds(delayBeforeReturn);

        //move the persistent player back near Penny
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
        {
            Charactercontroller cc = p.GetComponent<Charactercontroller>();
            if (cc != null) cc.enabled = false;

            p.transform.position = returnPosition;
            p.transform.rotation = Quaternion.Euler(returnEulerRotation);

            if (cc != null) cc.enabled = true;
        }

        //remember it's solved so Penny gives the thank-you lines
        PuzzleProgress.HouseISolved = true;

        if (SceneController.Instance == null)
        {
            Debug.LogError("PuzzleCompleteReturn: SceneController.Instance is NULL!");
            yield break;
        }

        var plan = SceneController.Instance.NewTransition()
            .Load(sceneSlot, mainWorldScene, setActive: true);

        if (useFade) plan = plan.WithOverlay();
        if (clearUnusedAssets) plan = plan.WithClearUnusedAssets();

        plan.Perform();
    }
}
