using UnityEngine;
using System.Collections;

public class PuzzleAComplete : MonoBehaviour
{
    /// <summary>
    /// Call this from your PuzzleManagerA's OnPuzzleComplete() event!
    /// </summary>
    public void StartDelayedWarp()
    {
        StartCoroutine(WaitForDialogueThenWarp());
    }

    private IEnumerator WaitForDialogueThenWarp()
    {
        // 1. Give the dialogue manager a frame or two to spin up 
        yield return null;
        yield return null;

        // 2. Wait here as long as the dialogue box is active on screen
        if (DialogueManager.Instance != null && DialogueManager.Instance.dialoguePanel != null)
        {
            while (DialogueManager.Instance.dialoguePanel.activeSelf)
            {
                yield return null; // Wait for the next frame
            }
        }

        // 3. Tiny pause for visual breathing room after the text box vanishes
        yield return new WaitForSeconds(0.1f);

        // 4. Fire the scene transition to load the MainWorld
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.SessionContent, "MapTest", setActive: true)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }
}